using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public class LdapExpressionVisitor : ExpressionVisitor {
		public override Expression Visit( Expression node ) {
			if( node == null )
				return null;

			switch( (LdapExpressionType)node.NodeType ) {
				case LdapExpressionType.And:
				case LdapExpressionType.Or:
					return this.VisitNary( (NaryExpression)node );
				case LdapExpressionType.Substring:
					return this.VisitSubstring( (SubstringExpression)node );
				case LdapExpressionType.Approx:
					return this.VisitApprox( (LdapApproxExpression)node );
				case LdapExpressionType.Present:
					return this.VisitAttributePresent( (LdapAttributePresentExpression)node );
				case LdapExpressionType.Extensible:
					return this.VisitExtensibleMatch( (LdapExtensibleMatch)node );
				case LdapExpressionType.Attribute:
					return this.VisitAttribute( (LdapAttributeExpression)node );
				case LdapExpressionType.DirectorySearcher:
					return this.VisitDirectorySearcher( (DirectorySearcherExpression)node );
				case LdapExpressionType.Projection:
					return this.VisitProjection( (ProjectionExpression)node );
				case LdapExpressionType.Sort:
					return this.VisitSort((LdapSortExpression)node);
				default:
					return base.Visit( node );
			}
		}

		protected virtual Expression VisitProjection( ProjectionExpression node ) {
			return node.Update(
				(DirectorySearcherExpression)this.Visit( node.Searcher ),
				this.Visit( node.Projector ),
				(LambdaExpression)this.Visit( node.Aggregator )
			);
		}

		protected virtual Expression VisitApprox( LdapApproxExpression node ) {
			return node.Update(this.Visit( node.Left ), this.Visit( node.Right ));
		}

		protected virtual Expression VisitAttributePresent( LdapAttributePresentExpression node ) {
			return node.Update( this.Visit( node.Operand ) );
		}

		protected virtual Expression VisitExtensibleMatch( LdapExtensibleMatch node ) {
			return node.Update( this.Visit( node.Attribute ), this.Visit( node.Value ) );
		}

		protected virtual Expression VisitDirectorySearcher( DirectorySearcherExpression node ) {
			return node.Update(
				Visit( node.Filter ),
				node.AttributesToLoad,
				(LdapSortExpression) Visit( node.Sort ),
				Visit( node.Skip ),
				Visit( node.Take )
			);
		}

		protected virtual Expression VisitNary( NaryExpression node ) {
			return node.Update( this.Visit( node.Clauses ) );
		}

		protected virtual Expression VisitSubstring( SubstringExpression node ) {
			return node.Update( this.Visit( node.Parts ) );
		}

		protected virtual Expression VisitAttribute( LdapAttributeExpression node ) {
			return node;
		}

		protected virtual Expression VisitSort(LdapSortExpression node) {
			return node.Update(this.Visit( node.Expression ));
		}
	}
}
