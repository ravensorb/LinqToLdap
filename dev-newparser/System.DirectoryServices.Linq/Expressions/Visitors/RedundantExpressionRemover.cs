using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public class RedundantExpressionRemover : LdapExpressionVisitor {
		private static readonly ConstantExpression False = Expression.Constant( false );
		private static readonly ConstantExpression True = Expression.Constant( true );

		internal static Expression Rewrite( Expression node ) {
			return new RedundantExpressionRemover().Visit( node );
		}

		protected override Expression VisitDirectorySearcher( DirectorySearcherExpression node ) {
			Expression result = base.VisitDirectorySearcher( node );
			if( result.NodeType != node.NodeType )
				return result;
			node = (DirectorySearcherExpression)result;
			if( node.Filter.IsConstant( true ) ) {
				return node.Update( null );
			}
			if( node.Filter.IsConstant( false ) ) {
				return null;
			}
			return node;
		}

		protected override Expression VisitUnary( UnaryExpression node ) {
			Expression result = base.VisitUnary( node );
			node = result as UnaryExpression;
			if( node == null )
				return result;

			switch( node.NodeType ) {
				case ExpressionType.Not:
					if( node.Operand != null && node.Operand.NodeType == ExpressionType.Not ) {
						// Not(Not(X)) -> X
						return ( (UnaryExpression)node.Operand ).Operand;
					}
					if( node.Operand != null && node.Operand.IsConstant( true ) ) {
						// Not(true) -> false
						return False;
					}
					if( node.Operand != null && node.Operand.IsConstant( false ) ) {
						// Not(true) -> false
						return True;
					}
					break;
			}
			return node;
		}

		protected override Expression VisitNary( NaryExpression node ) {
			Expression result = base.VisitNary( node );
			node = result as NaryExpression;
			if( node == null )
				return result;

			switch( (LdapExpressionType)node.NodeType ) {
				case LdapExpressionType.And:
					// (& X, false) -> false
					if( node.Clauses.Any( c => c.IsConstant( false ) ) )
						return False;
					// If any of the sub-clauses is true, then the 'and' is not affected by them.
					var andClauses = node.Clauses.Where( c => !c.IsConstant( true ) );
					switch( andClauses.Count() ) {
						case 0:
							// This is trivially true.
							// (& true) -> true
							return True;
						case 1:
							// Just return the sub-clause.
							// (& true, X) -> X
							return andClauses.First();
						default:
							if( andClauses.Count() == node.Clauses.Count ) {
								// No changes
								// (& X, Y) -> (& X, Y)
								return node;
							} else {
								// (& true, X, Y) -> (& X, Y)
								//return node.Update( andClauses );
								return LdapExpressionFactory.And( andClauses );
							}
					}
				case LdapExpressionType.Or:
					// (| X, true) -> true
					if( node.Clauses.Any( c => c.IsConstant( true ) ) )
						return True;
					// If any of the sub-clauses is false, then the 'or' is not affected by them.
					var orClauses = node.Clauses.Where( c => !c.IsConstant( false ) );
					switch( orClauses.Count() ) {
						case 0:
							// This is trivially false
							// (| false) -> false
							return False;
						case 1:
							// Just return the sub-clause.
							// (| false, X) -> X
							return orClauses.First();
						default:
							if( orClauses.Count() == node.Clauses.Count ) {
								// No changes
								// (| X, Y) -> (| X, Y)
								return node;
							} else {
								// (| X, Y, false) -> (| X, Y)
								//return node.Update( orClauses );
								return LdapExpressionFactory.And( orClauses );
							}
					}
				default:
					// Some other type of Nary expression...
					return node;
			}
		}
	}
}
