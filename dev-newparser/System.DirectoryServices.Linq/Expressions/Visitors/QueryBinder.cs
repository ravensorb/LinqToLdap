using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	class QueryBinder : ExpressionVisitor {
		private readonly IQueryProvider _provider;
		private Dictionary<ParameterExpression, Expression> _projectionMap;

		public QueryBinder( IQueryProvider provider ) {
			this._provider = provider;
			this._projectionMap = new Dictionary<ParameterExpression, Expression>();
		}

		public static Expression Bind( IQueryProvider provider, Expression expression ) {
			return new QueryBinder( provider ).Visit( expression );
		}

		private static Expression StripQuotes( Expression node ) {
			while( node.NodeType == ExpressionType.Quote ) {
				node = ( (UnaryExpression)node ).Operand;
			}
			return node;
		}

		protected override Expression VisitMethodCall( MethodCallExpression node ) {
			if( node.Method.DeclaringType == typeof( Queryable ) || node.Method.DeclaringType == typeof( Enumerable ) ) {
				switch( node.Method.Name ) {
					case "Where":
						return this.BindWhere( node.Type, node.Arguments[0], (LambdaExpression)StripQuotes( node.Arguments[1] ) );
					case "Select":
						return this.BindSelect( node.Type, node.Arguments[0], (LambdaExpression)StripQuotes( node.Arguments[1] ) );
					case "Count":
					case "First":
					case "FirstOrDefault":
					case "Single":
					case "SingleOrDefault":
					case "Last":
					case "LastOrDefault":
						if( node.Arguments.Count == 1 ){
							return this.BindSingle( node.Arguments[0], node.Method );
						} else if( node.Arguments.Count == 2 ) {
							var lambda = (LambdaExpression)StripQuotes( node.Arguments[1] );
							return this.BindSingle( node.Arguments[0], node.Method, lambda );
						}
						break;
					case "Skip":
					case "Take":
						return this.BindSkipTake( node.Arguments[0], node.Arguments[1], node.Method );
					case "OrderBy":
						return this.BindOrderBy( node.Type, node.Arguments[0], (LambdaExpression)StripQuotes( node.Arguments[1] ), SortDirection.Ascending );
					case "OrderByDescending":
						return this.BindOrderBy( node.Type, node.Arguments[0], (LambdaExpression)StripQuotes( node.Arguments[1] ), SortDirection.Descending );
					case "ThenBy":
					case "ThenByDescending":
						throw new NotSupportedException( "Can only order by one attribute." );
					default:
						throw new NotSupportedException();
				}
			}
			return base.VisitMethodCall( node );
		}

		private ProjectionExpression BindWhere( Type type, Expression source, LambdaExpression predicate ) {
			ProjectionExpression sourceProjection = this.VisitSequence( source );
			// Map lambda parameters to projection expressions.
			this._projectionMap[predicate.Parameters[0]] = sourceProjection.Projector;
			Expression where = this.Visit( predicate.Body );

			if( sourceProjection.Searcher.Filter != null )
				where = Expression.And( sourceProjection.Searcher.Filter, where );

			return sourceProjection.Update(
				sourceProjection.Searcher.Update(where)
			);
		}

		private ProjectionExpression BindOrderBy( Type type, Expression source, LambdaExpression orderSelector, SortDirection direction ) {
			ProjectionExpression sourceProjection = this.VisitSequence( source );
			if( sourceProjection.Searcher.Sort != null )
				throw new NotSupportedException( "Cannot order by more than one attribute." );
			this._projectionMap[orderSelector.Parameters[0]] = sourceProjection.Projector;
			var sort = new LdapSortExpression( this.Visit( orderSelector.Body ), direction );
			return sourceProjection.Update(
				sourceProjection.Searcher.Update(
					sourceProjection.Searcher.Filter,
					sourceProjection.Searcher.AttributesToLoad,
					sort
				)
			);
		}

		private ProjectionExpression BindSelect( Type type, Expression source, LambdaExpression selector ) {
			ProjectionExpression sourceProjection = this.VisitSequence( source );
			this._projectionMap[selector.Parameters[0]] = sourceProjection.Projector;

			Expression projector = this.Visit( selector.Body );

			return sourceProjection.Update(
				sourceProjection.Searcher,
				projector
			);
		}

		private ProjectionExpression BindSingle( Expression source, MethodInfo method ) {
			return BindSingle( source, method, null );
		}

		private ProjectionExpression BindSingle( Expression source, MethodInfo method, LambdaExpression predicate ) {
			ProjectionExpression sourceProjection;
			if( predicate == null ) {
				sourceProjection = this.VisitSequence( source );
			} else {
				sourceProjection = this.BindWhere( source.Type, source, predicate );
			}

			// Create an aggregate expression, which just calls the code in System.Linq.Enumerable.
			Type elementType = sourceProjection.Projector.Type;
			ParameterExpression p = Expression.Parameter( typeof( IEnumerable<> ).MakeGenericType( elementType ), "p" );
			LambdaExpression aggregator = Expression.Lambda(
				Expression.Call( typeof( Enumerable ), method.Name, new Type[] { elementType }, p ),
				p
			);

			return sourceProjection.Update(
				sourceProjection.Searcher,
				sourceProjection.Projector,
				aggregator
			);
		}

		private ProjectionExpression BindSkipTake( Expression source, Expression value, MethodInfo method ) {
			ProjectionExpression sourceProjection = this.VisitSequence( source );
			value = this.Visit( value );
			Expression skip = sourceProjection.Searcher.Skip;
			Expression take = sourceProjection.Searcher.Take;
			switch( method.Name ) {
				case "Skip":
					skip = value;
					break;
				case "Take":
					take = value;
					break;
				default:
					throw new NotSupportedException( string.Format( "{0} is not supported.", method.Name ) );
			}
			return sourceProjection.Update(
				sourceProjection.Searcher.Update(
					sourceProjection.Searcher.Filter,
					sourceProjection.Searcher.AttributesToLoad,
					sourceProjection.Searcher.Sort,
					skip,
					take
				)
			);
		}

		private ProjectionExpression VisitSequence( Expression source ) {
			return this.ConvertToSequence( this.Visit( source ) );
		}

		private ProjectionExpression ConvertToSequence( Expression expr ) {
			switch( expr.NodeType ) {
				case (ExpressionType)LdapExpressionType.Projection:
					return (ProjectionExpression)expr;
				default:
					throw new Exception( string.Format( "The expression of type '{0}' is not a sequence", expr.Type ) );
			}
		}

		protected override Expression VisitParameter( ParameterExpression node ) {
			Expression e;
			if( this._projectionMap.TryGetValue( node, out e ) )
				return e;
			else
				return node;
		}

		protected override Expression VisitConstant( ConstantExpression node ) {
			if( this.IsEntrySet( node.Value ) ) {
				if( typeof(EntryObjects.EntrySet).IsAssignableFrom(node.Type) )
					return EntrySetBinder.GetEntrySetProjection( (EntryObjects.EntrySet)node.Value );
				else if( typeof( EntryObjects.EntryCollection2 ).IsAssignableFrom( node.Type ) )
					return EntrySetBinder.GetEntryCollectionProjection( (EntryObjects.EntryCollection2)node.Value );
			}
			return node;
		}

		private bool IsEntrySet( object value ) {
			IQueryable q = value as IQueryable;
			return q != null && q.Provider == this._provider && q.Expression.NodeType == ExpressionType.Constant;
		}

		protected override Expression VisitMember( MemberExpression node ) {
			Expression source = this.Visit( node.Expression );

			switch( source.NodeType ) {
				case ExpressionType.New:
					NewExpression newExp = (NewExpression)source;
					if( newExp.Members != null ) {
						var newBinding = newExp.Members.Zip( newExp.Arguments, ( member, arg ) => new { Member = member, Expression = arg } )
							.FirstOrDefault( b => MembersMatch( b.Member, node.Member ) );
						if( newBinding != null )
							return newBinding.Expression;
					}
					break;
				case ExpressionType.MemberInit:
					MemberInitExpression miExpression = (MemberInitExpression)source;
					if( miExpression.Bindings != null ) {
						var initBinding = miExpression.Bindings.OfType<MemberAssignment>()
							.FirstOrDefault( b => MembersMatch( b.Member, node.Member ) );
						if( initBinding != null )
							return initBinding.Expression;
					}
					break;
			}
			if( source == node.Expression ) {
				return node;
			}

			return Expression.MakeMemberAccess( source, node.Member );
		}

		private bool MembersMatch( MemberInfo a, MemberInfo b ) {
			if( a == b )
				return true;
			if( a is MethodInfo && b is PropertyInfo )
				return a == ( (PropertyInfo)b ).GetGetMethod();
			if( a is PropertyInfo && b is MethodInfo )
				return ( (PropertyInfo)a ).GetGetMethod() == b;
			return false;
		}

	}
}
