using System.Collections.Generic;
using System.DirectoryServices.Linq.Attributes;
using System.Linq;
using System.Reflection;

namespace System.DirectoryServices.Linq.EntryObjects
{
	public class EntrySetCollection<TEntry> : EntrySet<TEntry> where TEntry : class
	{
		#region Fields

		private bool _isLoaded;
		private readonly EntryObject _entryObject;
		private PropertyInfo _property;
		private List<TEntry> _items = new List<TEntry>();

		#endregion

		#region Constructors

		public EntrySetCollection(EntryObject entryObject, PropertyInfo property) : base(entryObject.Context)
		{
			_entryObject = entryObject;
			_property = property;
		}

		#endregion

		#region Methods

		protected override IQueryProvider GetProvider()
		{
			var attribute = _property.AssertGetAttribute<EntryCollectionPropertyAttribute>();
			return new EntrySetCollectionQueryProvider(_entryObject, attribute.Scope, Context);
		}

		#endregion
	}
}