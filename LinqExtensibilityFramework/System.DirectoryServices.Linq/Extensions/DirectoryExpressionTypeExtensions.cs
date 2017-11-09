using System.DirectoryServices.Linq.Expressions;
using System.Linq.Expressions;

namespace System.DirectoryServices.Linq
{
	internal static class DirectoryExpressionTypeExtensions
	{
		internal static bool Is(this ExpressionType type, ExpressionType nodeType)
		{
			return type == nodeType;
		}

		internal static bool Is(this ExpressionType type, DirectoryExpressionType directoryType)
		{
			return type == (ExpressionType)directoryType;
		}
	}
}
