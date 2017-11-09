using System.Linq.Expressions;

namespace System.DirectoryServices.Linq.Expressions
{
	public class SelectExpressionVisitor : ExpressionVisitor
	{
		#region Methods

		protected Expression VisitLambda(LambdaExpression lambda)
		{
			Expression body = Visit(lambda.Body);

			if (body != lambda.Body)
			{
				lambda = Expression.Lambda(lambda.Type, body, lambda.Parameters);
			}

			return new SelectExpression(lambda);
		}

		public SelectExpression VisitSelect(LambdaExpression select)
		{
			var node = VisitLambda(select);

			if (node.NodeType.Is(DirectoryExpressionType.Select))
			{
				return node as SelectExpression;
			}

			throw new Exception();
		}

		#endregion
	}
}
