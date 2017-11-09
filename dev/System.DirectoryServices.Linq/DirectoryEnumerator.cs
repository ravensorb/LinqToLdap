using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices.Linq.Expressions;

namespace System.DirectoryServices.Linq
{
    internal class DirectoryEnumerator : DisposableObject, IEnumerator
    {
        #region Fields

        private object _current;
        private readonly SearchResultCollection _searchResults;
        private IEnumerator _resultEnumerator;

        #endregion

        #region Constructors

        public DirectoryEnumerator(SearchResultCollection searchResults)
        {
            _searchResults = searchResults;
            _resultEnumerator = searchResults.GetEnumerator();
        }

        #endregion

        #region Properties

        public object Current
        {
            get
            {
                if (_current == null)
                {
                    throw new InvalidOperationException();
                }

                return _current;
            }
        }

        #endregion

        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_searchResults != null)
                {
                    _searchResults.Dispose();
                }

                if (_resultEnumerator != null && _resultEnumerator is IDisposable)
                {
                    ((IDisposable)_resultEnumerator).Dispose();
                }
            }
        }

        public bool MoveNext()
        {
            if (_resultEnumerator.MoveNext())
            {
                // TODO Parse first/next result.
                _current = null;

                return true;
            }

            return false;
        }

        public void Reset()
        {
            _resultEnumerator = _searchResults.GetEnumerator();
        }

        #endregion
    }

    public class DirectoryEnumerator<T> : DisposableObject, IEnumerator<T>
    {
        #region Fields

        private readonly Type _origionalType;
        private readonly Delegate _projection;
        private readonly IResultMapper _resultMapper;
        private readonly DirectoryExpression _expression;
        private readonly IEnumerable<SearchResult> _searchResults;
        private IEnumerator<SearchResult> _resultEnumerator;
        private T _current;

        #endregion

        #region Constructors

        public DirectoryEnumerator(TranslatorContext context, IResultMapper resultMapper, IEnumerable<SearchResult> searchResults)
        {
            _resultMapper = resultMapper;
            _searchResults = searchResults;
            _expression = context.Expression;

            if (_expression != null && _expression.Select != null)
            {
                _origionalType = _expression.GetOrigionalType();

                if (_origionalType != typeof(T))
                {
                    _projection = _expression.Select.Projection.Compile();
                }
            }
        }

        #endregion

        #region Properties

        public T Current
        {
            get
            {
                if (Equals(_current, default(T)))
                {
                    throw new InvalidOperationException();
                }

                return _current;
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
            if (disposing)
            {
                if (_resultEnumerator != null)
                {
                    _resultEnumerator.Dispose();
                }
            }
        }

        public bool MoveNext()
        {
            if (_resultEnumerator == null)
            {
                Reset();
            }

            if (_resultEnumerator != null && _resultEnumerator.MoveNext())
            {
                var currentResult = _resultEnumerator.Current;

                if (_projection != null)
                {
                    var currentObject = _resultMapper.Map(_origionalType, currentResult);
                    _current = (T)_projection.DynamicInvoke(currentObject);
                }
                else
                {
                    _current = _resultMapper.Map<T>(currentResult);
                }

                return !Equals(_current, default(T));
            }

            return false;
        }

        public void Reset()
        {
            _resultEnumerator = _searchResults.GetEnumerator();
        }

        #endregion
    }
}
