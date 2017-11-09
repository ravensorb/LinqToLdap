using System.Text;

namespace System.DirectoryServices.Linq.Filters
{
	public class FilterBuilder
	{
		#region Fields

		private readonly Type _objectClassType;
		private readonly StringBuilder _builder = new StringBuilder();

		#endregion

		#region Constructors

		public FilterBuilder(Type type)
		{
			_objectClassType = type;
		}

		#endregion

		#region Methods

		public Type GetObjectClassType()
		{
			return _objectClassType;
		}

		public void Append(string value)
		{
			_builder.Append(value);
		}

		public void AddBuilder(AttributeBuilder filter)
		{
			Append(filter.ToString());
		}

		public AttributeBuilder CreateBuilder()
		{
			return new AttributeBuilder(this);
		}

		public override string ToString()
		{
			return _builder.ToString();
		}

		#endregion
	}
}
