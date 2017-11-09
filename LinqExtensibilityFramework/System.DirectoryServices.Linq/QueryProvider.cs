using System.Linq;
using System.Linq.Expressions;

namespace System.DirectoryServices.Linq
{
	public abstract class QueryProvider : IQueryProvider
	{
		public abstract object Execute(Expression expression);

		public abstract IQueryable CreateQuery(Expression expression);

		public abstract IQueryable<TElement> CreateQuery<TElement>(Expression expression);

		public abstract TResult Execute<TResult>(Expression expression);
	}
}
