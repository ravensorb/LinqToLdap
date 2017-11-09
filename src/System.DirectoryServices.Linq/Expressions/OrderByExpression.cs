using System.Linq.Expressions;

namespace System.DirectoryServices.Linq.Expressions
{
	public class OrderByExpression : DirectoryExpressionBase
	{
		#region Constructors

		public OrderByExpression(MemberExpression expression) : this(expression, OrderByDirection.Ascending)
		{
		}

		public OrderByExpression(MemberExpression expression, OrderByDirection direction) : base(expression, DirectoryExpressionType.OrderBy)
		{
			Direction = direction;
		}

		#endregion

		#region Properties

		public OrderByDirection Direction { get; internal set; }

		public MemberExpression OrderByProperty
		{
			get
			{
				return (MemberExpression)RootExpression;
			}
		}

		#endregion
	}
}
