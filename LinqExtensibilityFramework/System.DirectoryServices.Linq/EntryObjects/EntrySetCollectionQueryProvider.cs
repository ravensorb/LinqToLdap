using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace System.DirectoryServices.Linq.EntryObjects
{
	public class EntrySetCollectionQueryProvider : DirectoryQueryProvider
	{
		private readonly EntryObject _entry;
		private readonly SearchScope _scope;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="scope"></param>
		/// <param name="context"></param>
		public EntrySetCollectionQueryProvider(EntryObject entry, SearchScope scope, DirectoryContext context) : base(context)
		{
			_entry = entry;
			_scope = scope;
		}

		/// <summary>
		/// Create a new <see cref="EntrySetCollectionQueryState"/> for this provider, based on the given <see cref="Expression"/>.
		/// </summary>
		/// <param name="expression">The expression to get the query state for.</param>
		/// <returns>A new <see cref="EntryQueryState"/> object.</returns>
		protected override EntryQueryState CreateQueryState(Expression expression) {
			return new EntrySetCollectionQueryState(Context, _entry, _scope, expression);
		}

		/// <summary>
		/// Create a new <see cref="EntrySetCollectionQueryState"/> for this provider, based on the given <see cref="Expression"/>.
		/// </summary>
		/// <param name="expression">The expression to get the query state for.</param>
		/// <returns>A new <see cref="EntryQueryState"/> object.</returns>
		protected override EntryQueryState CreateQueryState<TElement>(Expression expression) {
			return new EntrySetCollectionQueryState(Context, _entry, _scope, typeof(TElement), expression);
		}

		/// <summary>
		/// Execute a <see cref="SingleResultExpression"/> expression in the current context.
		/// </summary>
		/// <param name="directoryExpression">The expression to execute.</param>
		/// <returns>The result of executing the expression.</returns>
		protected override object Execute(Expressions.SingleResultExpression directoryExpression) {
			return Context.QueryExecutor.Execute(_entry.Entry, _scope, directoryExpression);
		}

		/// <summary>
		/// Execute a <see cref="SingleResultExpression"/> expression in the current context.
		/// </summary>
		/// <param name="directoryExpression">The expression to execute.</param>
		/// <returns>The result of executing the expression.</returns>
		protected override TResult Execute<TResult>(Expressions.SingleResultExpression directoryExpression) {
			return Context.QueryExecutor.Execute<TResult>(_entry.Entry, _scope, directoryExpression);
		}
	}
}
