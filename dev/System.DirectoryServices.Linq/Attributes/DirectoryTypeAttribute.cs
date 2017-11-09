namespace System.DirectoryServices.Linq.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
	public class DirectoryTypeAttribute : DirectoryAttribute
	{
		private readonly string _schemaName;

		public DirectoryTypeAttribute(string name) : base(name)
		{
		}

		public DirectoryTypeAttribute(string name, string schema) : base(name)
		{
			_schemaName = schema;
		}

		public string SchemaName
		{
			get
			{
				if (!string.IsNullOrEmpty(_schemaName))
				{
					return _schemaName;
				}

				return Name;
			}
		}
	}
}