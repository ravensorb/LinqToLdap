using System.Linq.Expressions;

namespace System.DirectoryServices.Linq.Expressions
{
	public class SelectExpression : DirectoryExpressionBase
	{
		#region Constructors

		public SelectExpression(LambdaExpression projection) : base(projection, DirectoryExpressionType.Select)
		{
		}

		#endregion

		#region Properties

		public LambdaExpression Projection
		{
			get
			{
				return (LambdaExpression)RootExpression;
			}
		}

		#endregion
	}
}
