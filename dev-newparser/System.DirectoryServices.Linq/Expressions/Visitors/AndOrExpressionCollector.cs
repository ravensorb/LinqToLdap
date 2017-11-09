using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public class AndOrExpressionCollector : LdapExpressionVisitor {
		internal static Expression Rewrite( Expression node ) {
			return new AndOrExpressionCollector().Visit( node );
		}

		protected override Expression VisitBinary( BinaryExpression node ) {
			// Note that we're turning shortcutting operators into, well, not.
			switch( node.NodeType ) {
				case ExpressionType.And:
				case ExpressionType.AndAlso:
					if( node.Left.Type == typeof( bool ) && node.Right.Type == typeof( bool ) )
						return this.Visit( LdapExpressionFactory.And( new[] { node.Left, node.Right } ) );
					break;
				case ExpressionType.Or:
				case ExpressionType.OrElse:
					if( node.Left.Type == typeof( bool ) && node.Right.Type == typeof( bool ) )
						return this.Visit( LdapExpressionFactory.Or( new[] { node.Left, node.Right } ) );
					break;
				default:
					break;
			}
			return base.VisitBinary( node );
		}

		protected override Expression VisitNary( NaryExpression naryExpression ) {
			Expression result = base.VisitNary( naryExpression );
			if( result == null )
				return result;

			switch( (LdapExpressionType)result.NodeType ) {
				case LdapExpressionType.And:
				case LdapExpressionType.Or:
					return CollapseNary( (NaryExpression)result );
				default:
					return result;
			}
		}

		/// <summary>
		/// Collapse an N-ary expression down, if it contains N-ary expressions of the same type.
		/// </summary>
		/// <param name="node"></param>
		/// <returns>
		/// Null if the N-ary expression contains no sub-clauses;
		/// an Expression sub-clause, if it contains precisely one sub-clause;
		/// an equivalent N-ary expression, if it contains an N-ary expression of the same type;
		/// or the original expression, otherwise.
		/// </returns>
		private Expression CollapseNary( NaryExpression node ) {
			// TODO: check this is okay...
			if( node.Clauses.Count == 0 )
				return null;
			if( node.Clauses.Count == 1 )
				return node.Clauses[0];

			List<Expression> clauses = new List<Expression>();
			bool changed = false;
			foreach( var clause in node.Clauses ) {
				// If the clause is null, just ignore it.
				if( clause == null ) {
					// Don't add the clause to the new list.
					// Set changed to true, since we've actually changed the clause list by excluding this.
					changed = true;
					continue;
				}

				// If the clause isn't an N-ary clause of the same type as this one, we can't reduce any further.
				NaryExpression naryClause = clause as NaryExpression;
				if( naryClause == null || naryClause.NodeType != node.NodeType ) {
					clauses.Add( clause );
					continue;
				}

				// In this case, just take each sub-clause in the N-ary clause and add them to our list.
				foreach( var subclause in naryClause.Clauses ) {
					changed = true;
					clauses.Add( subclause );
				}
			}

			if( changed ) {
				// Check the clauses list again, as we could have reduced it all away...
				if( clauses.Count == 0 )
					return null;
				if( clauses.Count == 1 )
					return clauses[0];
				return node.Update( clauses );
			} else {
				return node;
			}
		}
	}
}
