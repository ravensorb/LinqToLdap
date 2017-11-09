using System.Collections.Generic;
using System.DirectoryServices.Linq.EntryObjects;

namespace System.DirectoryServices.Linq
{
	/// <summary>
	/// 
	/// </summary>
    public class RelationshipManager : IRelationshipManager
    {
        private readonly Type _parentType;
        private readonly IDictionary<string, object> _relationships = new Dictionary<string, object>();

		/// <summary>
		/// Initializes a new instance of the <see cref="RelationshipManager"/> class.
		/// </summary>
		/// <param name="entryParent">The entry parent.</param>
        public RelationshipManager(EntryObject entryParent)
        {
            EntryObject = entryParent;
            _parentType = entryParent.GetType();
        }

		/// <summary>
		/// Gets the entry object.
		/// </summary>
		/// <value>
		/// The entry object.
		/// </value>
        public EntryObject EntryObject { get; private set; }

		/// <summary>
		/// Gets an entry reference.
		/// </summary>
		/// <typeparam name="TEntry">The type of the entry.</typeparam>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
        public EntryReference<TEntry> GetEntryReference<TEntry>(string propertyName) where TEntry : EntryObject
        {
            var property = _parentType.GetProperty(propertyName);
            return new EntryReference<TEntry>(EntryObject, property);
        }

		/// <summary>
		/// Gets an entry collection.
		/// </summary>
		/// <typeparam name="TEntry">The type of the entry.</typeparam>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
        public EntryCollection<TEntry> GetEntryCollection<TEntry>(string propertyName) where TEntry : EntryObject
        {
            if (!_relationships.ContainsKey(propertyName))
            {
                var property = _parentType.GetProperty(propertyName);
                var result = new EntryCollection<TEntry>(EntryObject, property);
                _relationships.Add(propertyName, result);

                return result;
            }

            return (EntryCollection<TEntry>)_relationships[propertyName];
        }

		public EntrySetCollection<TEntry> GetEntrySetCollection<TEntry>(string propertyName) where TEntry : EntryObject
		{
			if (!_relationships.ContainsKey(propertyName))
			{
				var property = _parentType.GetProperty(propertyName);
				var result = new EntrySetCollection<TEntry>(EntryObject, property);
				_relationships.Add(propertyName, result);

				return result;
			}

			return (EntrySetCollection<TEntry>)_relationships[propertyName];
		}
	}
}
