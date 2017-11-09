using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.DirectoryServices.Linq.Expressions;

namespace System.DirectoryServices.Linq
{
    internal class SearchResults : IEnumerable<SearchResult>
    {
        #region Fields

        private readonly DirectoryExpression _expression;
        private readonly SearchResultCollection _searchResults;

        #endregion

        #region Constructors

        public SearchResults(SearchResultCollection searchResults)
        {
            _searchResults = searchResults;
        }

		public SearchResults(DirectoryExpression expression, SearchResultCollection searchResults) : this(searchResults)
        {
            _expression = expression;
        }

        #endregion

        #region Properties

        public int Count
        {
            get
            {
                return _searchResults.Count;
            }
        }

        #endregion

        #region Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<SearchResult> GetEnumerator()
        {
            if (_expression == null)
            {
                return new SearchResultEnumerator(null, null, _searchResults);
            }

            return new SearchResultEnumerator(_expression.Skip, _expression.Take, _searchResults);
        }

        #endregion

        private class SearchResultEnumerator : DisposableObject, IEnumerator<SearchResult>
        {
            #region Fields

            private readonly SkipTakeExpression _skip;
            private readonly SkipTakeExpression _take;
            private readonly SearchResultCollection _searchResults;

            private int _takeCounter;
            private int _totalTake;
            private int _totalCount;
            private int _currentIndex = -1;

            #endregion

            #region Constructors

            public SearchResultEnumerator(SkipTakeExpression skip, SkipTakeExpression take, SearchResultCollection searchResults)
            {
                _skip = skip;
                _take = take;
                _searchResults = searchResults;
                _totalCount = _searchResults.Count;

                Initialize();
            }

            #endregion

            #region Properties

            public SearchResult Current
            {
                get
                {
                    return _searchResults[_currentIndex];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            #endregion

            #region Methods

            protected override void Dispose(bool disposing)
            {
                if (disposing && _searchResults != null)
                {
                    _searchResults.Dispose();
                }
            }

            public bool MoveNext()
            {
                return ++_currentIndex < _totalCount && _takeCounter++ < _totalTake;
            }

            public void Reset()
            {
                _currentIndex = -1;
                _takeCounter = 0;
                _totalTake = _totalCount;

                Initialize();
            }

            private void Initialize()
            {
                if (_skip != null && _totalCount > _skip.Amount)
                {
                    _currentIndex = _skip.Amount - 1;
                }

                if (_take != null && _totalCount > (_currentIndex + _take.Amount))
                {
                    _totalTake = _take.Amount;
                }
                else
                {
                    _totalTake = (_totalCount - (_currentIndex + 1));
                }
            }

            #endregion
        }
    }
}
