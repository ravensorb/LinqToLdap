using System;
using System.Linq.Expressions;

namespace System.DirectoryServices.Linq.Expressions
{
	public abstract class DirectoryExpressionBase : Expression
	{
		#region Fields

		private readonly Expression _expression;
		private readonly DirectoryExpressionType _expressionType;

		#endregion

		#region Constructors

		protected DirectoryExpressionBase(Expression expression, DirectoryExpressionType type)
		{
			_expression = expression;
			_expressionType = type;
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

		public override Type Type
		{
			get 
			{ 
				 return _expression.Type;
			}
		}

		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)_expressionType;
			}
		}

		#endregion

		#region Methods

		public override Expression Reduce()
		{
			return _expression;
		}

		#endregion
	}
}
