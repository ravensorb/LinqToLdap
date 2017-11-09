using System.DirectoryServices.Linq.Attributes;

namespace System.DirectoryServices.Linq.EntryObjects
{
	internal class NamedEntryObject : EntryObject
	{
		[DirectoryProperty("samaccountname")]
		public string Name { get; set; }

		internal override DirectoryEntry Entry { get; set; }
		internal override DirectoryContext Context { get; set; }
	}
}
