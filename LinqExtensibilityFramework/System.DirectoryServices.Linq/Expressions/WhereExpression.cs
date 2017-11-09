using System.Linq.Expressions;

namespace System.DirectoryServices.Linq.Expressions
{
	public class WhereExpression : DirectoryExpressionBase
	{
		#region Constructors

		public WhereExpression(LambdaExpression where) : base(where, DirectoryExpressionType.Where)
		{
		}

		#endregion

		#region Properties

		public LambdaExpression Where
		{
			get
			{
				return (LambdaExpression)RootExpression;
			}
		}

		#endregion

		#region Methods

		public Type GetParameterType()
		{
			if (Where.Parameters.Count == 1)
			{
				return Where.Parameters[0].Type;
			}

			return null;
		}

		#endregion
	}
}
