using System.Linq.Expressions;
using System.Linq;

namespace System.DirectoryServices.Linq.Expressions
{
	public class DirectoryExpressionVisitor : ExpressionVisitor
	{
		#region Fields

		private DirectoryExpression _root;
		private OrderByExpressionVisitor _orderByVisitor;
		private SelectExpressionVisitor _selectVisitor;
		private SkipTakeExpressionVisitor _skipTakeVisitor;
		private WhereExpressionVisitor _whereVisitor;

		#endregion

		#region Methods

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			switch (node.Method.Name)
			{
				case "Where":
				{
					VisitWhere(((UnaryExpression)node.Arguments[1]).Operand as LambdaExpression);
					break;
				}
				case "Select":
				{
					VisitSelect(((UnaryExpression)node.Arguments[1]).Operand as LambdaExpression);
					break;
				}
				case "Last":
				case "First":
				case "Single":
				case "FirstOrDefault":
				case "LastOrDefault":
				case "SingleOrDefault":
				{
					VisitSingle(node);
					break;
				}
				case "Count":
				{
					VisitSingle(node);
					break;
				}
				case "Skip":
				{
					VisitSkip(node);
					break;
				}
				case "Take":
				{
					VisitTake(node);
					break;
				}
				case "OrderBy":
				{
					VisitOrderBy(((UnaryExpression)node.Arguments[1]).Operand as LambdaExpression);
					break;
				}
				case "OrderByDescending":
				{
					VisitOrderBy(((UnaryExpression)node.Arguments[1]).Operand as LambdaExpression, OrderByDirection.Decending);
					break;
				}
				default:
				{
					if (node.Method.DeclaringType == typeof(Queryable))
					{
						throw new NotSupportedException(string.Format(Properties.Resources.MethodNotSupported, node.Method.Name));
					}

					break;
				}
			}

			return base.VisitMethodCall(node);
		}

		private void VisitOrderBy(LambdaExpression lambda)
		{
			VisitOrderBy(lambda, OrderByDirection.Ascending);
		}

		private void VisitOrderBy(LambdaExpression lambda, OrderByDirection direction)
		{
			var member = lambda.Body as MemberExpression;

			if (member != null)
			{
				if (_orderByVisitor == null)
				{
					_orderByVisitor = new OrderByExpressionVisitor();
				}

				_root.SetOrderBy(_orderByVisitor.VisitOrderBy(member, direction));
			}
		}

		private void VisitSingle(MethodCallExpression method)
		{
			if (_root.NodeType.Is(DirectoryExpressionType.SingleResult))
			{
				var root = (SingleResultExpression)_root;
				SingleResultType resultType;

				try
				{
					resultType = (SingleResultType)Enum.Parse((typeof(SingleResultType)), method.Method.Name);
				}
				catch
				{
					resultType = SingleResultType.Single;
				}

				root.SingleResultType = resultType;
			}

			if (method.Arguments.Count > 1)
			{
				VisitWhere(((UnaryExpression)method.Arguments[1]).Operand as LambdaExpression);
			}
		}

		private void VisitSkip(MethodCallExpression method)
		{
			if (_skipTakeVisitor == null)
			{
				_skipTakeVisitor = new SkipTakeExpressionVisitor();
			}

			_root.SetSkip(_skipTakeVisitor.VisitSkip(method));
		}

		private void VisitTake(MethodCallExpression method)
		{
			if (_skipTakeVisitor == null)
			{
				_skipTakeVisitor = new SkipTakeExpressionVisitor();
			}

			_root.SetTake(_skipTakeVisitor.VisitTake(method));
		}

		private void VisitWhere(LambdaExpression lambdaExpression)
		{
			if (_whereVisitor == null)
			{
				_whereVisitor = new WhereExpressionVisitor();
			}

			_root.AddWhere(_whereVisitor.VisitWhere(lambdaExpression));
		}

		private void VisitSelect(LambdaExpression lambdaExpression)
		{
			if (_selectVisitor == null)
			{
				_selectVisitor = new SelectExpressionVisitor();
			}

			_root.SetSelect(_selectVisitor.VisitSelect(lambdaExpression));
		}

		public SingleResultExpression VisitSingleResult(SingleResultExpression node)
		{
			_root = node;
			base.Visit(node.RootExpression);
			return node;
		}

		public DirectoryExpression VisitDirectory(DirectoryExpression node)
		{
			_root = node;
			base.Visit(node.RootExpression);
			return node;
		}

		public override Expression Visit(Expression node)
		{
			if (node is DirectoryExpression && _root != node)
			{
				_root = (DirectoryExpression)node;
				return base.Visit(_root.RootExpression);
			}

			return base.Visit(node);
		}

		#endregion
	}
}
