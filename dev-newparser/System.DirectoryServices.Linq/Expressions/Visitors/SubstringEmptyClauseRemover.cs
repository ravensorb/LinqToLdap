using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public class SubstringEmptyClauseRemover : LdapExpressionVisitor {
		public static Expression Rewrite( Expression node ) {
			return new SubstringEmptyClauseRemover().Visit( node );
		}

		protected override Expression VisitSubstring( SubstringExpression node ) {
			// If any of the clauses that aren't the first or last one are null or empty, remove them.
			// If the first or last ones are empty, should alter the expression to add wildcards at those points.

			if( node.Parts.Count == 0 ) {
				// TODO: is this right?
				if( !node.WildcardAtStart && !node.WildcardAtEnd )
					return Expression.Constant( string.Empty );
				return node;
			}

			if( node.Parts[0].IsConstantNullOrEmpty() ) {
				return this.VisitSubstring( node.Update( true, node.Parts.Skip( 1 ), node.WildcardAtEnd ) );
			}

			if( node.Parts.Last().IsConstantNullOrEmpty() ) {
				return this.VisitSubstring( node.Update( node.WildcardAtStart, node.Parts.Take( node.Parts.Count - 1 ), true ) );
			}

			// If there is just one part with no associated wildcards, just use that.
			if( node.Parts.Count == 1 && !node.WildcardAtStart && !node.WildcardAtEnd && node.Parts[0].IsConstant() ) {
				return node.Parts[0];
			}

			// If we get here, we know that neither the first or last part are empty!
			var newParts = node.Parts.Where( p => !p.IsConstantNullOrEmpty() );
			return node.Update( newParts );
		}

		protected override Expression VisitBinary( BinaryExpression node ) {
			Expression result = base.VisitBinary( node );
			node = result as BinaryExpression;
			if( node == null )
				return result;

			switch( node.NodeType ) {
				case ExpressionType.Equal:
					if( node.Right.IsConstantNullOrEmpty() ) {
						return Expression.Not( LdapExpressionFactory.Present( node.Left ) );
					}
					if( node.Right.NodeType == (ExpressionType)LdapExpressionType.Substring ) {
						SubstringExpression substring = node.Right as SubstringExpression;
						/* If it's a substring expression with no parts to it, it's of one of the following forms:
						 * X=*.  X=*.*  X=.*  X=.
						 * (where the dot represents the empty parts list).
						 * In the first three cases, this translates to Present(X) expression;
						 * in the last one, to Not(Present(X)).  */
						if( substring.Parts.Count == 0 ) {
							if( substring.WildcardAtStart || substring.WildcardAtEnd ) {
								return LdapExpressionFactory.Present( node.Left );
							} else {
								return Expression.Not( LdapExpressionFactory.Present( node.Left ) );
							}
						}
					}
					break;
			}

			return node;
		}
	}
}
