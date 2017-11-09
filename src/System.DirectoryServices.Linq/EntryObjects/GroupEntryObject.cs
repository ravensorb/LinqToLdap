using System.Runtime.InteropServices;

namespace System.DirectoryServices.Linq.EntryObjects
{
	public class GroupEntryObject : EntryObject
	{
		public bool Add(EntryObject obj)
		{
			if (obj != null && obj.Entry != null)
			{
				var addUserResult = Entry.Invoke("Add", obj.Entry.Path);
				var result = addUserResult == null;

				if (!result)
				{
					Marshal.ReleaseComObject(addUserResult);
				}

				return result;
			}

			return false;
		}

		public bool Remove(EntryObject obj)
		{
			if (obj != null && obj.Entry != null)
			{
				var removeUserResult = Entry.Invoke("Remove", obj.Entry.Path);
				var result = removeUserResult == null;

				if (!result)
				{
					Marshal.ReleaseComObject(removeUserResult);
				}

				return result;
			}

			return false;
		}
	}
}
