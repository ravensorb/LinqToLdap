using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public class ProjectionBuilder : LdapExpressionVisitor {
		private static readonly MethodInfo getattributemethodgenericness = typeof( ResultMapper2 ).GetMethod( "GetAttribute" );
		ParameterExpression _parameterExpression;
		private readonly DirectoryContext _context;

		public ProjectionBuilder( DirectoryContext context ) {
			_parameterExpression = Expression.Parameter( typeof( System.DirectoryServices.SearchResult ), "result" );
			this._context = context;
		}

		public static LambdaExpression Build( DirectoryContext context, Expression node ) {
			var builder = new ProjectionBuilder( context );
			Expression body = builder.Visit( node );
			return Expression.Lambda( body, builder._parameterExpression );
		}

		protected override Expression VisitMemberInit( MemberInitExpression node ) {
			if( node.Type.IsSubclassOf( typeof( EntryObjects.EntryObject ) ) ) {
				var newBindings = new List<MemberBinding>();

				// Now need to go through and replace any instances of IEnumerable<EntryObject> with something else.
				foreach( var binding in node.Bindings ) {
					if( IsEnumerableOfEntryObjects( binding ) ) {
						Type type = GetEnumerableType(binding);
						var newBinding = Expression.Bind(
							binding.Member,
							Expression.New(
								typeof(EntryObjects.EntryCollection2<>).MakeGenericType(type).GetConstructor(new Type[]{typeof(IQueryProvider)}),
								Expression.Constant(_context.QueryProvider)
							)
						);
						newBindings.Add(newBinding);
					} else {
						newBindings.Add( binding );
					}
				}

				//entryObject.Context = Context;
				newBindings.Add( BindExpression( "Context", Expression.Constant( this._context ) ) );
				//entryObject.ChangeState = ChangeState.None;
				// -- strictly speaking, no need for this, since it will default to None anyway.
				newBindings.Add( BindExpression( "ChangeState", Expression.Constant( EntryObjects.ChangeState.None ) ) );
				//entryObject.ADPath = result.Path;
				PropertyInfo pathProperty = typeof( System.DirectoryServices.SearchResult ).GetProperty( "Path" );
				newBindings.Add( BindExpression( "ADPath", Expression.MakeMemberAccess( _parameterExpression, pathProperty ) ) );
				//entryObject.Entry = result.GetDirectoryEntry();
				MethodInfo getDirectoryEntryMethod = typeof( System.DirectoryServices.SearchResult ).GetMethod( "GetDirectoryEntry" );
				newBindings.Add( BindExpression( "Entry", Expression.Call( _parameterExpression, getDirectoryEntryMethod ) ) );
				//entryObject.SetParent( entryObject.Entry.Parent ); // -- no need for this any more
				//Context.ChangeTracker.TrackChanges( entryObject ); // -- no need for this any more

				node = node.Update( node.NewExpression, newBindings );
			}

			return base.VisitMemberInit( node );
		}

		private static bool IsEnumerableOfEntryObjects( MemberBinding binding ) {
			var propInfo = binding.Member as PropertyInfo;
			if( propInfo == null )
				return false;
			Type type = propInfo.PropertyType;

			return typeof( IEnumerable<EntryObjects.EntryObject> ).IsAssignableFrom( type );
		}

		private static Type GetEnumerableType( MemberBinding binding ) {
			var propInfo = binding.Member as PropertyInfo;
			Type type = propInfo.PropertyType;
			// TODO: this is crap, change this!
			return type.GenericTypeArguments[0];
		}

		private static MemberAssignment BindExpression( string propertyName, Expression boundExpression ) {
			MemberInfo memberInfo = typeof( EntryObjects.EntryObject ).GetProperty( propertyName, BindingFlags.Instance | BindingFlags.NonPublic );
			return Expression.Bind( memberInfo, boundExpression );
		}

		protected override Expression VisitAttribute( LdapAttributeExpression node ) {
			var getattributemethod = getattributemethodgenericness.MakeGenericMethod( node.Type );
			return Expression.Call( getattributemethod, _parameterExpression, Expression.Constant( node.Name ) );
		}

	}



	internal class ResultMapper2 {
		public static TResult GetAttribute<TResult>( SearchResult result, string attributeName ) {
			// TODO: is this right? Or should it be an exception?
			if( !result.Properties.Contains( attributeName ) )
				return default( TResult );

			 //TODO: what about typeof(TResult).GetInterface(typeof(IEnumerable<>).FullName) == ...
			if( typeof( TResult ).IsGenericType && typeof( TResult ).IsAssignableFrom( typeof( IEnumerable<> ).MakeGenericType( typeof( TResult ).GenericTypeArguments[0] ) ) ) {
				Type type = typeof( TResult ).GenericTypeArguments[0];

				MethodInfo mi = typeof( ResultMapper2 ).GetMethod( "GetListAttribute", BindingFlags.NonPublic | BindingFlags.Static ).MakeGenericMethod( type );
				return (TResult)mi.Invoke( null, new object[] { result.Properties[attributeName] } );
			}

			return GetSimpleAttribute<TResult>( result.Properties[attributeName] );
		}

		private static TResult GetSimpleAttribute<TResult>( ResultPropertyValueCollection resultPropertyCollection ) {
			if( resultPropertyCollection.Count == 1 ) {
				object value = resultPropertyCollection[0];

				return (TResult)GetResultValue( typeof( TResult ), value );
			} else {
				throw new NotImplementedException();
			}
		}

		private static IEnumerable<TResult> GetListAttribute<TResult>( ResultPropertyValueCollection resultPropertyCollection ) {
			if( typeof( TResult ).IsSubclassOf( typeof( EntryObjects.EntryObject ) ) ) {
				throw new NotImplementedException();
			}

			var resultEnumerable = new List<TResult>();
			foreach( var propertyValue in resultPropertyCollection ) {
				resultEnumerable.Add( (TResult)GetResultValue( typeof( TResult ), propertyValue ) );
			}
			return resultEnumerable;
		}

		private static object GetResultValue( Type propertyType, object value ) {
			if( value != null ) {
				var valueType = value.GetType();

				if( valueType != propertyType ) {
					if( valueType == typeof( byte[] ) && propertyType == typeof( Guid ) ) {
						return new Guid( (byte[])value );
					}

					if( propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof( Nullable<> ) ) {
						return Activator.CreateInstance( propertyType, value );// new[] { value });
					}

					return Convert.ChangeType( value, propertyType );
				}
			}

			return value;
		}

	}



}

