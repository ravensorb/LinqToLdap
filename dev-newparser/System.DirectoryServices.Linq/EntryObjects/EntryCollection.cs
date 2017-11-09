using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices.Linq.Attributes;
using System.DirectoryServices.Linq.Filters;
using System.Reflection;

namespace System.DirectoryServices.Linq.EntryObjects
{
    public class EntryCollection<TEntry> : IEnumerable<TEntry> where TEntry : class
    {
        #region Fields

        private bool _isLoaded;
        private readonly EntryObject _entryObject;
        private PropertyInfo _property;
        private List<TEntry> _items = new List<TEntry>();

        #endregion

        #region Constructors

        public EntryCollection(EntryObject entryObject, PropertyInfo property)
        {
            _entryObject = entryObject;
            _property = property;
        }

        #endregion

        #region Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<TEntry> GetEnumerator()
        {
            if (!_isLoaded)
            {
                LoadCollection();
            }

            return _items.GetEnumerator();
        }

        private void LoadCollection()
        {
			var propertyAttribute = _property.AssertGetAttribute<EntryCollectionPropertyAttribute>();
			var filter = CreateFilter(typeof(TEntry), propertyAttribute);

			throw new NotImplementedException();
			//using (var enumerator = GetEntryEnumerator(queryExecutor, propertyAttribute, filter))
			//{
			//	while (enumerator.MoveNext())
			//	{
			//		_items.Add(enumerator.Current);
			//	}
			//}

			//_isLoaded = true;
		}

		//private IEnumerator<TEntry> GetEntryEnumerator(IQueryExecutor queryExecutor, EntryCollectionPropertyAttribute propertyAttribute, string filter)
		//{
		//	if (propertyAttribute.MatchingRule == MatchingRuleType.Children)
		//	{
		//		return queryExecutor.ExecuteQuery<TEntry>(_entryObject.Entry, propertyAttribute.Scope, filter);
		//	}

		//	return queryExecutor.ExecuteQuery<TEntry>(filter);
		//}

		private string CreateFilter(Type entryType, EntryCollectionPropertyAttribute propertyAttribute)
		{
			var builder = new FilterBuilder(entryType);
			var attributeBuilder = builder.CreateBuilder();
			var attribute = entryType.AssertGetAttribute<DirectoryTypeAttribute>();
			attributeBuilder.AddObjectClass(attribute.Name);

			if (propertyAttribute.MatchingRule != MatchingRuleType.Children)
			{
				// Example AttributeName: "member:1.2.840.113556.1.4.1941:"
				var attributeName = string.Concat(propertyAttribute.Name, propertyAttribute.MatchingRuleValue);
				attributeBuilder.AddAttribute(attributeName, FilterOperator.Equals, _entryObject.InternalDn);
			}

			builder.AddBuilder(attributeBuilder);

			return builder.ToString();
		}

		#endregion
	}
}
