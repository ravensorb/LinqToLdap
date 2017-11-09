using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.DirectoryServices.Linq.Expressions
{
	public abstract class DirectoryExpressionBase : Expression
	{
		#region Fields

		private readonly Expression _expression;

		#endregion

		#region Constructors

		protected DirectoryExpressionBase(Expression expression, DirectoryExpressionType type) : base((ExpressionType)type, expression.Type)
		{
			_expression = expression;
		}

		#endregion

		#region Properties

		public Expression RootExpression
		{
			get
			{
				return _expression;
			}
		}

		#endregion
	}
}
