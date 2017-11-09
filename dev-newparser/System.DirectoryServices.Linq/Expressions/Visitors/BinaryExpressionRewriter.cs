using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public class BinaryExpressionRewriter : LdapExpressionVisitor {
		public static Expression Rewrite( Expression node ) {
			return new BinaryExpressionRewriter().Visit( node );
		}

		protected override Expression VisitBinary( BinaryExpression node ) {
			Expression left = this.Visit( node.Left );
			Expression right = this.Visit( node.Right );

			switch( node.NodeType ) {
				case ExpressionType.GreaterThan:
					return Expression.Not( Expression.LessThanOrEqual( left, right ) );
				// Alternative: 
				//return Expression.And( Expression.GreaterThanOrEqual( left, right ), Expression.Not( Expression.Equal( left, right ) ) );
				case ExpressionType.LessThan:
					return Expression.Not( Expression.GreaterThanOrEqual( left, right ) );
				// Alternative: 
				//return Expression.And( Expression.LessThanOrEqual( left, right ), Expression.Not( Expression.Equal( left, right ) ) );
				case ExpressionType.NotEqual:
					return Expression.Not( Expression.Equal( left, right ) );
				default:
					return base.VisitBinary( node );
			}
		}
	}
}
