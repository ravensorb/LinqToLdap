
namespace System.DirectoryServices.Linq.EntryObjects
{
	public interface IEntryWithRelationships
	{
		IRelationshipManager RelationshipManager { get; }
	}
}
