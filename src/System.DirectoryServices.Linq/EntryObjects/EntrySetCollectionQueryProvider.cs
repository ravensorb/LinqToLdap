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
		/// Executes the query represented by a specified expression tree.
		/// </summary>
		/// <returns>
		/// The value that results from executing the specified query.
		/// </returns>
		/// <param name="expression">An expression tree that represents a LINQ query.</param>
		public override object Execute(Expression expression)
		{
			return Execute(_entry, expression);
		}

		/// <summary>
		/// Executes the strongly-typed query represented by a specified expression tree.
		/// </summary>
		/// <returns>
		/// The value that results from executing the specified query.
		/// </returns>
		/// <param name="expression">An expression tree that represents a LINQ query.</param><typeparam name="TResult">The type of the value that results from executing the query.</typeparam>
		public override TResult Execute<TResult>(Expression expression)
		{
			return Execute<TResult>(_entry, expression);
		}

		/// <summary>
		/// Constructs an <see cref="T:System.Linq.IQueryable"/> object that can evaluate the query represented by a specified expression tree.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Linq.IQueryable"/> that can evaluate the query represented by the specified expression tree.
		/// </returns>
		/// <param name="expression">An expression tree that represents a LINQ query.</param>
		public override IQueryable CreateQuery(Expression expression)
		{
			Type type = GetQueryableType(expression);
			return (IQueryable)Activator.CreateInstance(
				typeof(EntryQuery<>).MakeGenericType(type),
				new object[] { _entry, new EntrySetCollectionQueryState(Context, _entry, _scope, expression) }
			);
		}
		
		/// <summary>
		/// Constructs an <see cref="T:System.Linq.IQueryable`1"/> object that can evaluate the query represented by a specified expression tree.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Linq.IQueryable`1"/> that can evaluate the query represented by the specified expression tree.
		/// </returns>
		/// <param name="expression">An expression tree that represents a LINQ query.</param><typeparam name="TElement">The type of the elements of the <see cref="T:System.Linq.IQueryable`1"/> that is returned.</typeparam>
		public override IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			return CreateQuery<TElement>(_entry, expression);
		}

		/// <summary>
		/// Constructs an <see cref="T:System.Linq.IQueryable`1"/> object that can evaluate the query represented by a specified expression tree.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Linq.IQueryable`1"/> that can evaluate the query represented by the specified expression tree.
		/// </returns>
		/// <param name="entry">The <see cref="EntryObject"/> to use as the root.</param>
		/// <param name="expression">An expression tree that represents a LINQ query.</param><typeparam name="TElement">The type of the elements of the <see cref="T:System.Linq.IQueryable`1"/> that is returned.</typeparam>
		public virtual IQueryable<TElement> CreateQuery<TElement>(EntryObject entry, Expression expression)
		{
			return new EntryQuery<TElement>(new EntrySetCollectionQueryState(Context, _entry, _scope, typeof(TElement), expression));
		}

		/// <summary>
		/// Executes the query represented by a specified expression tree.
		/// </summary>
		/// <returns>
		/// The value that results from executing the specified query.
		/// </returns>
		/// <param name="entry"></param>
		/// <param name="expression">An expression tree that represents a LINQ query.</param>
		public virtual object Execute(EntryObject entry, Expression expression)
		{
			var queryState = new EntryQueryState(Context, expression.Type, expression);
			var directoryExpression = queryState.GetSingleResultExpression();
			return Context.QueryExecutor.Execute(entry.Entry, _scope, directoryExpression);
		}

		/// <summary>
		/// Executes the strongly-typed query represented by a specified expression tree.
		/// </summary>
		/// <returns>
		/// The value that results from executing the specified query.
		/// </returns>
		/// <param name="entry"></param>
		/// <param name="expression">An expression tree that represents a LINQ query.</param><typeparam name="TResult">The type of the value that results from executing the query.</typeparam>
		public virtual TResult Execute<TResult>(EntryObject entry, Expression expression)
		{
			var queryState = new EntryQueryState(Context, typeof(TResult), expression);
			var directoryExpression = queryState.GetSingleResultExpression();
			return Context.QueryExecutor.Execute<TResult>(entry.Entry, _scope, directoryExpression);
		}
	}
}
