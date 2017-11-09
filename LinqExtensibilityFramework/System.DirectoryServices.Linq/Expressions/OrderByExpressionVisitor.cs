using System.Linq.Expressions;

namespace System.DirectoryServices.Linq.Expressions
{
	public class OrderByExpressionVisitor : ExpressionVisitor
	{
		public OrderByExpression VisitOrderBy(MemberExpression member, OrderByDirection direction)
		{
			var node = VisitMember(member);

			if (node.NodeType.Is(DirectoryExpressionType.OrderBy))
			{
				var orderBy = (OrderByExpression)node;
				orderBy.Direction = direction;
				return orderBy;
			}

			throw new Exception();
		}

		protected override Expression VisitMember(MemberExpression orderBy)
		{
			var member = Visit(orderBy.Expression);

			if (member != orderBy.Expression)
			{
				orderBy = Expression.MakeMemberAccess(member, orderBy.Member);
			}

			return new OrderByExpression(orderBy);
		}
	}
}
