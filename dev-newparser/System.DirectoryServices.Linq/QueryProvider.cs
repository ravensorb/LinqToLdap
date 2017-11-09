using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.DirectoryServices.Linq
{
	public abstract class QueryProvider : IQueryProvider
	{
		IQueryable<TElement> IQueryProvider.CreateQuery<TElement>( Expression expression ) {
			return new Query<TElement>(this, expression);
		}

		IQueryable IQueryProvider.CreateQuery( Expression expression ) {
			Type elementType = TypeSystem.GetElementType( expression.Type );
			try {
				return (IQueryable)Activator.CreateInstance( typeof( Query<> ).MakeGenericType( elementType ), new object[] { this, expression } );
			} catch( TargetInvocationException tie ) {
				throw tie.InnerException;
			}
		}

		TResult IQueryProvider.Execute<TResult>( Expression expression ) {
			return (TResult)this.Execute( expression );
		}

		object IQueryProvider.Execute( Expression expression ) {
			return this.Execute( expression );
		}

		//public abstract string GetFilterText( Expression expression );
		public abstract object Execute( Expression expression );

	}

	/// <summary>
	/// A default implementation of IQueryable for use with QueryProvider
	/// </summary>
	public class Query<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T> {
		QueryProvider provider;
		Expression expression;

		public Query( QueryProvider provider ) {
			if( provider == null ) {
				throw new ArgumentNullException( "provider" );
			}
			this.provider = provider;
			this.expression = Expression.Constant( this );
		}

		public Query( QueryProvider provider, Expression expression ) {
			if( provider == null ) {
				throw new ArgumentNullException( "provider" );
			}
			if( expression == null ) {
				throw new ArgumentNullException( "expression" );
			}
			if( !typeof( IQueryable<T> ).IsAssignableFrom( expression.Type ) ) {
				throw new ArgumentOutOfRangeException( "expression" );
			}
			this.provider = provider;
			this.expression = expression;
		}

		Expression IQueryable.Expression {
			get { return this.expression; }
		}

		Type IQueryable.ElementType {
			get { return typeof( T ); }
		}

		IQueryProvider IQueryable.Provider {
			get { return this.provider; }
		}

		public IEnumerator<T> GetEnumerator() {
			return ( (IEnumerable<T>)this.provider.Execute( this.expression ) ).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return ( (IEnumerable)this.provider.Execute( this.expression ) ).GetEnumerator();
		}

		public override string ToString() {
			return this.expression.ToString();
		}
	}

}

