using System.Collections.Generic;
using System.DirectoryServices.Linq.Attributes;
using System.DirectoryServices.Linq.Expressions;
using System.Reflection;

namespace System.DirectoryServices.Linq
{
	public interface IQueryExecutor
	{
		T Execute<T>(string query);
		T Execute<T>(SingleResultExpression expression);
		T Execute<T>(DirectoryEntry entry, SingleResultExpression expression);
		T Execute<T>(DirectoryEntry entry, SearchScope scope, SingleResultExpression expression);

		object Execute(string query, Type elementType);
		object Execute(SingleResultExpression expression);
		object Execute(DirectoryEntry entry, SingleResultExpression expression);
		object Execute(DirectoryEntry entry, SearchScope scope, SingleResultExpression expression);

		//IEnumerable<T> ExecuteCommand<T>(string query);

		IEnumerator<T> ExecuteQuery<T>(string query);
		IEnumerator<T> ExecuteQuery<T>(DirectoryExpression expression);
		IEnumerator<T> ExecuteQuery<T>(DirectoryEntry entry, DirectoryExpression expression);
		IEnumerator<T> ExecuteQuery<T>(DirectoryEntry entry, SearchScope scope, DirectoryExpression expression);
		IEnumerator<T> ExecuteQuery<T>(DirectoryEntry entry, string query);
		IEnumerator<T> ExecuteQuery<T>(DirectoryEntry entry, SearchScope scope, string query);

		string GetAttributeName<TAttribute>(MemberInfo info) where TAttribute : DirectoryAttribute;
	}
}
