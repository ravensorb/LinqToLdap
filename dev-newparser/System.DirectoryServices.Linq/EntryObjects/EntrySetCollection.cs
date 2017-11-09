using System.Collections.Generic;
using System.DirectoryServices.Linq.Attributes;
using System.Linq;
using System.Reflection;

namespace System.DirectoryServices.Linq.EntryObjects
{
	public class EntrySetCollection<TEntry> : EntrySet<TEntry> where TEntry : class
	{
		#region Fields

		private readonly EntryObject _entryObject;
		private PropertyInfo _property;
		private SearchScope? _scope = null;

		#endregion

		#region Constructors

		public EntrySetCollection(EntryObject entryObject, PropertyInfo property) : base(entryObject.Context)
		{
			_entryObject = entryObject;
			_property = property;

			var attribute = _property.GetAttribute<EntryCollectionPropertyAttribute>();
			if (attribute != null) {
				_scope = attribute.Scope;
			}
		}

		#endregion

		#region Properties

		internal override DirectoryEntry RootEntry { get { return _entryObject.Entry; } }

		internal override SearchScope? Scope {
			get {
				return this._scope;
			}
		}

		#endregion

		public override void AddEntry( string samAccountName, TEntry entry ) {
			var entryObject = entry as EntryObject;

			if( entryObject != null ) {
				entryObject.Parent = _entryObject;
				Context.AddObject<TEntry>( samAccountName, entry );
			}
		}

	}
}