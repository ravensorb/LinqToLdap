using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace System.DirectoryServices.Linq.EntryObjects
{
	public abstract class EntryQuery : IEntryQuery, IQueryable, IOrderedQueryable
	{
		private readonly EntryQueryState _queryState;
		private readonly IQueryProvider _provider;

		internal protected EntryQuery(IQueryProvider provider, EntryQueryState queryState)
		{
			if( queryState == null )
				throw new ArgumentNullException( "queryState" );
			if( provider == null )
				throw new ArgumentNullException( "provider" );
			_queryState = queryState;
			_provider = provider;
		}

		EntryQueryState IEntryQuery.QueryState
		{
			get
			{
				return _queryState;
			}
		}

		DirectoryContext IEntryQuery.Context
		{
			get
			{
				return _queryState.Context;
			}
		}

		Type IQueryable.ElementType
		{
			get
			{
				return GetElementType();
			}
		}

		Expression IQueryable.Expression
		{
			get
			{
				return GetExpression();
			}
		}

		IQueryProvider IQueryable.Provider
		{
			get
			{
				return _provider;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumeratorCore();
		}

		protected virtual Expression GetExpression()
		{
			return _queryState.Expression;
		}

		protected abstract Type GetElementType();

		protected abstract IEnumerator GetEnumeratorCore();
	}

	public class EntryQuery<T> : EntryQuery, IEntryQuery<T>, IQueryable<T>, IOrderedQueryable<T>
	{
		public EntryQuery(IQueryProvider provider, EntryQueryState queryState) : base(provider, queryState)
		{
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumeratorCore();
		}

		protected override Type GetElementType()
		{
			return typeof(T);
		}

		protected override IEnumerator GetEnumeratorCore()
		{
			return GetEnumerator();
		}

		internal virtual IEnumerator<T> GetResultsEnumerator()
		{
			var queryState = ((IEntryQuery)this).QueryState;
			var expression = queryState.GetExpression();

			if (queryState is EntrySetCollectionQueryState)
			{
				var entryQueryState = (EntrySetCollectionQueryState)queryState;
				return ((IEntryQuery)this).Context.QueryExecutor.ExecuteQuery<T>(entryQueryState.EntryObject.Entry, entryQueryState.Scope, expression);
			}

			return ((IEntryQuery)this).Context.QueryExecutor.ExecuteQuery<T>(expression);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return GetResultsEnumerator();
		}
	}
}
