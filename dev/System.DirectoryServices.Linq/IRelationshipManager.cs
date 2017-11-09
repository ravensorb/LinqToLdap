using System.DirectoryServices.Linq.EntryObjects;

namespace System.DirectoryServices.Linq
{
	public interface IRelationshipManager
	{
		EntryObject EntryObject { get; }
		EntryCollection<TEntry> GetEntryCollection<TEntry>(string propertyName) where TEntry : EntryObject;
		EntrySetCollection<TEntry> GetEntrySetCollection<TEntry>(string propertyName) where TEntry : EntryObject;
		EntryReference<TEntry> GetEntryReference<TEntry>(string propertyName) where TEntry : EntryObject;
	}
}
