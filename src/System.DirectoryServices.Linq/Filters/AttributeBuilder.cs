using System.Text;

namespace System.DirectoryServices.Linq.Filters
{
	public class AttributeBuilder
	{
		#region Fields

		private readonly FilterCollection _attributes = new FilterCollection();
		private readonly AttributeFilter _objectClass = new AttributeFilter(FilterConstants.ObjectClass);
		private readonly FilterCollection _objectClasses = new FilterCollection();

		#endregion

		#region Constructors

		public AttributeBuilder(FilterBuilder parent)
		{
			Parent = parent;
		}

		#endregion

		#region Properties

		public FilterBuilder Parent { get; set; }

		#endregion

		#region Methods

		public void AddObjectClass(string @class)
		{
			if (!string.IsNullOrEmpty(@class))
			{
				_objectClass.Value = @class;
			}
		}

		public void Add(Filter filter)
		{
			_attributes.Add(filter);
		}

		public void AddAttribute(string attr, FilterOperator @operator, object value)
		{
			_attributes.Add(attr, @operator, value);
		}

		public override string ToString()
		{
			if (_attributes.Count > 0)
			{
				var builder = new StringBuilder("(&");

				if (_objectClasses.Count == 0)
				{
					_objectClasses.Add(_objectClass);
				}

				for (int i = 0; i < _objectClasses.Count; i++)
				{
					builder.Append(_objectClasses[i]);
				}

				for (int i = 0; i < _attributes.Count; i++)
				{
					builder.Append(_attributes[i]);
				}

				return builder.Append(")").ToString();
			}

			return _objectClass.ToString();
		}

		#endregion
	}
}
