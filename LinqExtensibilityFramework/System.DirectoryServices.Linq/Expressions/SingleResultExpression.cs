using System.Linq.Expressions;

namespace System.DirectoryServices.Linq.Expressions
{
	public class SingleResultExpression : DirectoryExpression
	{
		#region Constructors

		public SingleResultExpression(Expression expression) : base(expression, DirectoryExpressionType.SingleResult)
		{
		}

		#endregion

		#region Properties

		public SingleResultType SingleResultType { get; internal set; }

		public bool ThrowIfNotFound
		{
			get
			{
				var resultType = SingleResultType;
				return resultType == SingleResultType.First
					|| resultType == SingleResultType.Single
					|| resultType == SingleResultType.Last;
			}
		}

		public bool ThrowIfNotSingle
		{
			get
			{
				var resultType = SingleResultType;
				return resultType == SingleResultType.Single
					|| resultType == SingleResultType.SingleOrDefault;
			}
		}

		#endregion
	}
}
