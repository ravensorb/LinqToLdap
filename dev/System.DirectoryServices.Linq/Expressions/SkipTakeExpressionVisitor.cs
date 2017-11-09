using System.Linq.Expressions;

namespace System.DirectoryServices.Linq.Expressions
{
	public class SkipTakeExpressionVisitor : ExpressionVisitor
	{
		#region Methods

		private SkipTakeExpression VisitSkip(ConstantExpression constant)
		{
			return CreateSkipTake(constant, DirectoryExpressionType.Skip);
		}

		private SkipTakeExpression VisitTake(ConstantExpression constant)
		{
			return CreateSkipTake(constant, DirectoryExpressionType.Take);
		}

		private SkipTakeExpression CreateSkipTake(ConstantExpression expression, DirectoryExpressionType expressionType)
		{
			if (expression.Type == typeof(int))
			{
				return new SkipTakeExpression(expression, expressionType);
			}

			return null;
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			switch (node.Method.Name)
			{
				case "Skip":
				{
					return VisitSkip((ConstantExpression)node.Arguments[1]);
				}
				case "Take":
				{
					return VisitTake((ConstantExpression)node.Arguments[1]);
				}
				default:
				{
					throw new Exception("Not supported skip or take method.");
				}
			}
		}

		public SkipTakeExpression VisitSkip(MethodCallExpression skip)
		{
			return VisitMethod(skip, DirectoryExpressionType.Skip);
		}

		public SkipTakeExpression VisitTake(MethodCallExpression take)
		{
			return VisitMethod(take, DirectoryExpressionType.Take);
		}

		private SkipTakeExpression VisitMethod(MethodCallExpression method, DirectoryExpressionType expressionType)
		{
			var node = VisitMethodCall(method);

			if (node.NodeType.Is(expressionType))
			{
				return node as SkipTakeExpression;
			}

			throw new Exception();
		}

		#endregion
	}
}
