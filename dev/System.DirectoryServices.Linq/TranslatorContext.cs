using System.Collections.Generic;
using System.DirectoryServices.Linq.Expressions;
using System.Linq;

namespace System.DirectoryServices.Linq
{
    public class TranslatorContext
    {
        #region Constructors

        public TranslatorContext(DirectorySearcher directorySearcher)
        {
            DirectorySearcher = directorySearcher;
        }

        public TranslatorContext(DirectoryExpression expression, DirectorySearcher directorySearcher)
        {
            Expression = expression;
            DirectorySearcher = directorySearcher;
        }

        #endregion

        #region Properties

        public DirectoryExpression Expression { get; private set; }

        public DirectorySearcher DirectorySearcher { get; private set; }

        #endregion

        #region Methods

        public SearchResult FindOne()
        {
            var results = GetAllSearchResults();
            return GetSingleSearchResult(results);
        }

        public IEnumerable<SearchResult> FindAll()
        {
            return GetAllSearchResults();
        }

        private SearchResult GetSingleSearchResult(SearchResults results)
        {
            if (Expression != null && Expression.NodeType.Is(DirectoryExpressionType.SingleResult))
            {
                var single = (SingleResultExpression)Expression;

                if (results.Count > 1 && single.ThrowIfNotSingle)
                {
                    throw new MoreThanOneResultException();
                }

                if (results.Count == 0 && single.ThrowIfNotFound)
                {
                    throw new ResultNotFoundException();
                }

                var resultType = single.SingleResultType;

                if (resultType == SingleResultType.Last || resultType == SingleResultType.LastOrDefault)
                {
                    return results.LastOrDefault();
                }

                return results.FirstOrDefault();
            }

            return results.FirstOrDefault();
        }

        private SearchResults GetAllSearchResults()
        {
            return new SearchResults(Expression, DirectorySearcher.FindAll());
        }

        #endregion
    }
}
