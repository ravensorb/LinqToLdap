using System.Linq.Expressions;

namespace System.DirectoryServices.Linq.Expressions
{
	public class SkipTakeExpression : DirectoryExpressionBase
	{
		#region Constructors

		public SkipTakeExpression(ConstantExpression skipTake, DirectoryExpressionType nodeType) : base(skipTake, nodeType)
		{
			if (nodeType != DirectoryExpressionType.Take && nodeType != DirectoryExpressionType.Skip)
			{
				throw new Exception();
			}
		}

		#endregion

		#region Properties

		public int Amount
		{
			get
			{
				return (int)SkipTake.Value;
			}
		}

		public ConstantExpression SkipTake
		{
			get
			{
				return (ConstantExpression)RootExpression;
			}
		}

		#endregion
	}
}
