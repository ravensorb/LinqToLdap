using System.Linq.Expressions;

namespace System.DirectoryServices.Linq.Expressions
{
	public class WhereExpressionVisitor : ExpressionVisitor
	{
		#region Methods

		protected Expression VisitLambda(LambdaExpression lambda)
		{
			Expression body = Visit(lambda.Body);

			if (body != lambda.Body)
			{
				lambda = Expression.Lambda(lambda.Type, body, lambda.Parameters);
			}

			return new WhereExpression(lambda);
		}

		public WhereExpression VisitWhere(LambdaExpression select)
		{
			var node = VisitLambda(select);

			if (node.NodeType.Is(DirectoryExpressionType.Where))
			{
				return node as WhereExpression;
			}

			throw new Exception();
		}

		#endregion
	}
}
