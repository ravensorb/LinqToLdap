using System.Text;

namespace System.DirectoryServices.Linq.Filters
{
	public class AttributeFilter : Filter
	{
		#region Fields

		private object _value;

		#endregion

		#region Constructors

		public AttributeFilter() : base(null)
		{
		}

		public AttributeFilter(string attr) : this()
		{
			Attribute = attr;
			Value = "*";
		}

		#endregion

		#region Properties

		public string Attribute { get; set; }

		public FilterOperator Operator { get; set; }

		public object Value
		{
			get
			{
				if (_value is Guid)
				{
					var builder = new StringBuilder();
					var array = ((Guid)_value).ToByteArray();

					for (int i = 0; i < array.Length; i++)
					{
						builder.AppendFormat(@"\{0}", array[i].ToString("x2"));
					}

					_value = builder.ToString();
				}

				return _value;
			}
			set
			{
				_value = value;
			}
		}

		#endregion

		#region Methods

		public AttributeFilter Clone()
		{
			return new AttributeFilter
			{
				Attribute = Attribute,
				Operator = Operator,
				Value = Value
			};
		}

		public override string ToString()
		{
			switch (Operator)
			{
				case FilterOperator.NotEquals:
				{
					if (string.IsNullOrEmpty(Convert.ToString(Value)))
					{
						return string.Format("({0}=*)", Attribute);
					}

					return string.Format("(!{0}={1})", Attribute, Value);
				}
				case FilterOperator.GreaterThan:
				{
					return string.Format("({0}>={1})", Attribute, Value);
				}
				case FilterOperator.GreaterThanOrEqual:
				{
					return string.Format("(|({0}>={1})({0}={1}))", Attribute, Value);
				}
				case FilterOperator.LessThan:
				{
					return string.Format("({0}<={1})", Attribute, Value);
				}
				case FilterOperator.LessThanOrEqual:
				{
					return string.Format("(|({0}<={1})({0}={1}))", Attribute, Value);
				}
				case FilterOperator.Contains:
				{
					return string.Format("({0}=*{1}*)", Attribute, Value);
				}
				case FilterOperator.StartsWith:
				{
					return string.Format("({0}={1}*)", Attribute, Value);
				}
				case FilterOperator.EndsWith:
				{
					return string.Format("({0}=*{1})", Attribute, Value);
				}
				default:
				{
					if (string.IsNullOrEmpty(Convert.ToString(Value)))
					{
						return string.Format("(!({0}=*))", Attribute);
					}

					return string.Format("({0}={1})", Attribute, Value);
				}
			}
		}

		#endregion
	}
}
