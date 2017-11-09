using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public static class ExpressionExtensions {
		private static bool IsTypedConstant<T>( this Expression node ) {
			return node.IsConstant() && node.Type == typeof( T );
		}

		private static T GetTypedConstantValue<T>( this Expression node ) {
			return ( (T)( (ConstantExpression)node ).Value );
		}

		public static bool IsConstant( this Expression node ) {
			return node != null && node.NodeType == ExpressionType.Constant;
		}

		public static bool IsConstant<T>( this Expression node, T value ) {
			return IsTypedConstant<T>(node) && GetTypedConstantValue<T>(node).Equals( value );
		}

		public static bool IsConstantNull( this Expression node ) {
			return IsConstant(node) && ( (ConstantExpression)node ).Value == null;
		}

		public static bool IsConstantNullOrEmpty( this Expression node ) {
			//return IsTypedConstant<string>( node ) && string.IsNullOrEmpty(GetTypedConstantValue<string>( node ));
			return IsConstantNull(node) || IsConstant(node, string.Empty);
		}
	}
}
