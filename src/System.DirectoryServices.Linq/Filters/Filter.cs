
namespace System.DirectoryServices.Linq.Filters
{
	public class Filter
	{
		private readonly string _value;

		//public static readonly Filter OpenGroup = new Filter("(");
		public static readonly Filter OpenAndGroup = new Filter("(&(");
		public static readonly Filter OpenOrGroup = new Filter("(|(");
		public static readonly Filter OpenNotGroup = new Filter("(!(");
		public static readonly Filter CloseGroup = new Filter("))");

		protected Filter(string value)
		{
			_value = value;
		}

		public override string ToString()
		{
			return _value.ToString();
		}
	}
}
