using System.Collections.Generic;
using System.DirectoryServices.Linq.Attributes;
using System.DirectoryServices.Linq.Expressions;
using System.Reflection;

namespace System.DirectoryServices.Linq
{
    public class QueryExecutor : IQueryExecutor
    {
        public QueryExecutor(DirectoryContext context)
        {
            Context = context;
        }

        public DirectoryContext Context { get; private set; }

        public IQueryTranslator Translator
        {
            get
            {
                return Context.Translator;
            }
        }

        public T Execute<T>(string query)
        {
            using (var searcher = CreateDirectorySearcher(query, typeof(T)))
            {
                return Translator.TranslateOne<T>(searcher);
            }
        }

        public T Execute<T>(SingleResultExpression expression)
        {
            using (var searcher = CreateDirectorySearcher(expression))
            {
                return Translator.TranslateOne<T>(expression, searcher);
            }
        }

		public T Execute<T>(DirectoryEntry entry, SingleResultExpression expression)
		{
			return Execute<T>(entry, SearchScope.Subtree, expression);
		}

		public T Execute<T>(DirectoryEntry entry, SearchScope scope, SingleResultExpression expression)
		{
			using (var searcher = CreateDirectorySearcher(entry, scope, expression))
			{
				return Translator.TranslateOne<T>(expression, searcher);
			}
		}
        public object Execute(string query, Type elementType)
        {
            using (var searcher = CreateDirectorySearcher(query, elementType))
            {
                return Translator.TranslateOne(elementType, searcher);
            }
        }

        public object Execute(SingleResultExpression expression)
        {
            using (var searcher = CreateDirectorySearcher(expression))
            {
                return Translator.TranslateOne(expression, searcher);
            }
        }

		public object Execute(DirectoryEntry entry, SingleResultExpression expression)
		{
			using (var searcher = CreateDirectorySearcher(entry, expression))
			{
				return Translator.TranslateOne(expression, searcher);
			}
		}

		public object Execute(DirectoryEntry entry, SearchScope scope, SingleResultExpression expression)
		{
			using (var searcher = CreateDirectorySearcher(entry, scope, expression))
			{
				return Translator.TranslateOne(expression, searcher);
			}
		}
        //public IEnumerable<T> ExecuteCommand<T>(string query)
        //{

        //}

        public IEnumerator<T> ExecuteQuery<T>(string query)
        {
            using (var searcher = CreateDirectorySearcher(query, typeof(T)))
            {
                return Translator.Translate<T>(searcher);
            }
        }

		public IEnumerator<T> ExecuteQuery<T>(DirectoryEntry entry, string query)
		{
			using (var searcher = CreateDirectorySearcher(entry, query, typeof(T)))
			{
				return Translator.Translate<T>(searcher);
			}
		}

		public IEnumerator<T> ExecuteQuery<T>(DirectoryEntry entry, SearchScope scope, string query)
		{
			using (var searcher = CreateDirectorySearcher(entry, scope, query, typeof(T)))
			{
				return Translator.Translate<T>(searcher);
			}
		}
        public IEnumerator<T> ExecuteQuery<T>(DirectoryExpression expression)
        {
			return ExecuteQuery<T>(Context.RootEntry, expression);
		}

		public IEnumerator<T> ExecuteQuery<T>(DirectoryEntry entry, DirectoryExpression expression)
		{
			return ExecuteQuery<T>(entry, SearchScope.Subtree, expression);
		}

		public IEnumerator<T> ExecuteQuery<T>(DirectoryEntry entry, SearchScope scope, DirectoryExpression expression)
		{
			using (var searcher = CreateDirectorySearcher(entry, scope, expression))
            {
                return Translator.Translate<T>(expression, searcher);
            }
        }

        private DirectorySearcher CreateDirectorySearcher(DirectoryExpression expression)
        {
            var origionalType = expression.GetOrigionalType();
            return CreateDirectorySearcher(null, origionalType);
        }

		private DirectorySearcher CreateDirectorySearcher(DirectoryEntry entry, DirectoryExpression expression)
		{
			return CreateDirectorySearcher(entry, SearchScope.Subtree, expression);
		}

		private DirectorySearcher CreateDirectorySearcher(DirectoryEntry entry, SearchScope scope, DirectoryExpression expression)
		{
			var origionalType = expression.GetOrigionalType();
			return CreateDirectorySearcher(entry, scope, null, origionalType);
		}
        private DirectorySearcher CreateDirectorySearcher(string filter, Type elementType)
        {
			return CreateDirectorySearcher(Context.RootEntry, SearchScope.Subtree, filter, elementType);
		}

		private DirectorySearcher CreateDirectorySearcher(DirectoryEntry entry, string filter, Type elementType)
		{
			return CreateDirectorySearcher(entry, SearchScope.Subtree, filter, elementType);
		}

		private DirectorySearcher CreateDirectorySearcher(DirectoryEntry entry, SearchScope scope, string filter, Type elementType)
		{
			var properties = GetPropertiesFromType(elementType);
			return new DirectorySearcher(entry, filter, properties)
			{
				PageSize = 1000,
				SearchScope = scope,
			};
		}

        private string[] GetPropertiesFromType(Type type)
        {
            var list = new List<string>();

			foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attributeName = GetAttributeName<DirectoryPropertyAttribute>(property);

                if (!string.IsNullOrEmpty(attributeName))
                {
                    list.Add(attributeName);
                }
            }

            return list.ToArray();
        }

        public string GetAttributeName<TAttribute>(MemberInfo info) where TAttribute : DirectoryAttribute
        {
            var attribute = info.GetAttribute<TAttribute>();

            if (attribute != null && !string.IsNullOrEmpty(attribute.Name))
            {
                return attribute.Name;
            }

            return null;
        }
    }
}
