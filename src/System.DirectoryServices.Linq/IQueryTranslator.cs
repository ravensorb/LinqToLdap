using System.DirectoryServices.Linq.Expressions;

namespace System.DirectoryServices.Linq
{
	public interface IQueryTranslator
	{
		T TranslateOne<T>(TranslatorContext context);
		object TranslateOne(TranslatorContext context);
		DirectoryEnumerator<T> Translate<T>(TranslatorContext context);

		T TranslateOne<T>(DirectorySearcher searcher);
		object TranslateOne(Type elementType, DirectorySearcher searcher);
		DirectoryEnumerator<T> Translate<T>(DirectorySearcher searcher);

		T TranslateOne<T>(SingleResultExpression expression, DirectorySearcher searcher);
		object TranslateOne(SingleResultExpression expression, DirectorySearcher searcher);
		DirectoryEnumerator<T> Translate<T>(DirectoryExpression expression, DirectorySearcher searcher);
	}
}
