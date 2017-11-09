using System;

namespace System.DirectoryServices.Linq.Attributes
{
	public class DirectoryAttribute : Attribute
	{
		public DirectoryAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }
	}
}
