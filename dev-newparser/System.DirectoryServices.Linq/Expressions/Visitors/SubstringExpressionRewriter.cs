using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public class SubstringExpressionRewriter : LdapExpressionVisitor {
		public static Expression Rewrite( Expression node ) {
			return new SubstringExpressionRewriter().Visit( node );
		}

		protected override Expression VisitMethodCall( MethodCallExpression node ) {
			if( ( node.Method.DeclaringType == typeof( string ) )
				&& ( node.Method.CallingConvention & System.Reflection.CallingConventions.HasThis ) == System.Reflection.CallingConventions.HasThis
				&& node.Arguments.Count == 1 ) {

				Expression objectExpression = this.Visit( node.Object );
				Expression argumentExpression = this.Visit( node.Arguments[0] );
				Expression substringExpression;

				switch( node.Method.Name ) {
					case "StartsWith":
						substringExpression = LdapExpressionFactory.Substring( false, new[] { argumentExpression }, true );
						break;
					case "EndsWith":
						substringExpression = LdapExpressionFactory.Substring( true, new[] { argumentExpression }, false );
						break;
					case "Contains":
						substringExpression = LdapExpressionFactory.Substring( true, new[] { argumentExpression }, true );
						break;
					default:
						return base.VisitMethodCall( node );
				}

				return Expression.Equal( objectExpression, substringExpression );
			} else
				return base.VisitMethodCall( node );
		}
	}

}
