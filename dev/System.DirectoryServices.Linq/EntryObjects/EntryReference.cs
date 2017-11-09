using System.Reflection;

namespace System.DirectoryServices.Linq.EntryObjects
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TEntry"></typeparam>
	public class EntryReference<TEntry> : EntryObject where TEntry : EntryObject
	{
		private EntryObject _entryObject;
		private PropertyInfo _property;

		/// <summary>
		/// Initializes a new instance of the <see cref="EntryReference{TEntry}"/> class.
		/// </summary>
		/// <param name="entryObject">The entry object.</param>
		/// <param name="property">The property of the parent <see cref="EntryObject"/>.</param>
		public EntryReference(EntryObject entryObject, PropertyInfo property)
		{
			_entryObject = entryObject;
			_property = property;
		}

		public TEntry Value { get; set; }
	}
}
