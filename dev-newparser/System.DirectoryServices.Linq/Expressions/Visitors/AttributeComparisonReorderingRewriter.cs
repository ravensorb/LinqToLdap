using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public class AttributeComparisonReorderingRewriter : LdapExpressionVisitor {
		public static Expression Rewrite( Expression node ) {
			return new AttributeComparisonReorderingRewriter().Visit( node );
		}

		protected override Expression VisitBinary( BinaryExpression node ) {
			Expression left = this.Visit( node.Left );
			Expression right = this.Visit( node.Right );

			if( right.NodeType == (ExpressionType)LdapExpressionType.Attribute
				&& left.NodeType != (ExpressionType)LdapExpressionType.Attribute
			) {
				switch( node.NodeType ) {
					case ExpressionType.Equal:
						return Expression.Equal( right, left );
					case ExpressionType.LessThanOrEqual:
						return Expression.GreaterThanOrEqual( right, left );
					case ExpressionType.GreaterThanOrEqual:
						return Expression.LessThanOrEqual( right, left );
					default:
						// Don't make any changes.
						break;
				}
			}

			return node.Update( left, node.Conversion, right );
		}
	}
}
