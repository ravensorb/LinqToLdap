using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public class NullComparisonRewriter : LdapExpressionVisitor {
		public static Expression Rewrite( Expression node ) {
			return new NullComparisonRewriter().Visit( node );
		}

		protected override Expression VisitBinary( BinaryExpression node ) {
			Expression left = this.Visit( node.Left );
			Expression right = this.Visit( node.Right );

			if (node.NodeType == ExpressionType.Equal
				&& left.NodeType == (ExpressionType)LdapExpressionType.Attribute
				&& right.NodeType == ExpressionType.Constant
			) {
				ConstantExpression constant = (ConstantExpression)right;
				if( constant.Value == null ) {
					return Expression.Not( LdapExpressionFactory.Present( left ) );
				}
			}

			return node.Update( left, node.Conversion, right );
		}
	}
}
