using System.Collections;
using System.Collections.Generic;

namespace System.DirectoryServices.Linq.Filters
{
	public class FilterCollection : IEnumerable<Filter>
	{
		#region Fields

		private readonly List<Filter> _attributes = new List<Filter>();

		#endregion

		#region Properties

		public Filter this[int index]
		{
			get
			{
				return _attributes[index];
			}
		}

		public int Count
		{
			get
			{
				return _attributes.Count;
			}
		}

		#endregion

		#region Methods

		public void Add(string attr, FilterOperator @operator, object value)
		{
			Add(new AttributeFilter
			{
				Attribute = attr,
				Operator = @operator,
				Value = value
			});
		}

		public void Add(Filter attr)
		{
			_attributes.Add(attr);
		}

		public IEnumerator<Filter> GetEnumerator()
		{
			return _attributes.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
