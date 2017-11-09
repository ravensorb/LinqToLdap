using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public class LdapFilterFormatter : LdapExpressionVisitor {
		private StringBuilder _sb;

		public static string Format( Expression expression ) {
			var formatter = new LdapFilterFormatter();
			formatter.Visit( expression );
			return formatter._sb.ToString();
		}

		private LdapFilterFormatter() {
			_sb = new StringBuilder();
		}

		protected override Expression VisitMethodCall( MethodCallExpression node ) {
			throw new NotSupportedException( string.Format( Properties.Resources.MethodNotSupported, node.Method.Name ) );
		}

		protected override Expression VisitUnary( UnaryExpression node ) {
			switch( node.NodeType ) {
				case ExpressionType.Not:
					_sb.Append( "(!" );
					this.Visit( node.Operand );
					_sb.Append( ")" );
					break;
				default:
					throw new NotSupportedException( string.Format( "The unary operator '{0}' is not supported.", node.NodeType ) );
			}
			return node;
		}

		protected override Expression VisitAttributePresent( LdapAttributePresentExpression node ) {
			_sb.Append( "(" );
			if (node.Operand.NodeType != (ExpressionType)LdapExpressionType.Attribute)
				throw new NotSupportedException(string.Format("Cannot have expression of type {0} as part of a filter.", node.Operand.NodeType));
			this.Visit( node.Operand );
			_sb.Append( "=*)" );
			return node;
		}

		protected override Expression VisitExtensibleMatch( LdapExtensibleMatch node ) {
			_sb.Append( "(" );
			if (node.Attribute.NodeType != (ExpressionType)LdapExpressionType.Attribute)
				throw new NotSupportedException(string.Format("Cannot have expression of type {0} as part of a filter.", node.Attribute.NodeType));
			this.Visit( node.Attribute );
			if( node.IncludeDistinguishedName )
				_sb.Append( ":dn" );
			_sb.Append( ":" );
			_sb.Append( node.MatchingRule.FilterExpression );
			_sb.Append( ":=" );
			this.Visit( node.Value );
			_sb.Append( ")" );
			return node;
		}

		protected override Expression VisitBinary( BinaryExpression node ) {
			_sb.Append( "(" );

			if( node.Left == null )
				throw new NotSupportedException( string.Format( "Cannot have null expression as part of a filter." ) );
			if( node.Left.NodeType != (ExpressionType)LdapExpressionType.Attribute )
				throw new NotSupportedException( string.Format( "Cannot have expression of type {0} as part of a filter.", node.Left.NodeType ) );
			this.Visit( node.Left );

			switch( node.NodeType ) {
				case ExpressionType.Equal:
					_sb.Append( "=" );
					break;
				case ExpressionType.LessThanOrEqual:
					_sb.Append( "<=" );
					break;
				case ExpressionType.GreaterThanOrEqual:
					_sb.Append( ">=" );
					break;
				default:
					throw new NotSupportedException( string.Format( "The binary operator '{0}' is not supported.", node.NodeType ) );
			}

			if( node.Right == null)
				throw new NotSupportedException( string.Format( "Cannot have null expression as part of a filter." ) );
			if ((LdapExpressionType)node.Right.NodeType != LdapExpressionType.Substring 
				&& node.Right.NodeType != ExpressionType.Constant )
				throw new NotSupportedException( string.Format( "Cannot have expression of type {0} as part of a filter.", node.Left.NodeType ) );
			this.Visit( node.Right );
			_sb.Append( ")" );

			return node;
		}

		protected override Expression VisitApprox( LdapApproxExpression node ) {
			this.Visit( node.Left );
			_sb.Append( "~=" );
			this.Visit( node.Right );
			return node;
		}

		protected override Expression VisitConstant( ConstantExpression node ) {
			if( node.Value == null ) {
				// umm...
				throw new NotImplementedException();
			}
			if( node.Type == typeof( string ) ) {
				_sb.Append( EscapeString( node.Value.ToString() ) );
			} else if( node.Type == typeof( Guid ) ) {
				var array = ( (Guid)node.Value ).ToByteArray();
				for( int i = 0; i < array.Length; i++ ) {
					_sb.AppendFormat( @"\{0}", array[i].ToString( "x2" ) );
				}
			} else {
				throw new NotSupportedException( string.Format( "The constant value '{0}' is not supported.", node.Value ) );
			}

			return node;
		}

		protected override Expression VisitAttribute( LdapAttributeExpression node ) {
			_sb.Append( node.Name );
			return node;
		}

		protected override Expression VisitNary( NaryExpression node ) {
			switch( (LdapExpressionType)node.NodeType ) {
				case LdapExpressionType.And:
					_sb.Append( "(&" );
					this.Visit( node.Clauses );
					_sb.Append( ")" );
					break;
				case LdapExpressionType.Or:
					_sb.Append( "(|" );
					this.Visit( node.Clauses );
					_sb.Append( ")" );
					break;
				default:
					throw new NotSupportedException( string.Format( "The N-ary operator '{0}' is not supported.", node.NodeType ) );
			}
			return node;
		}

		protected override Expression VisitSubstring( SubstringExpression node ) {
			// Assert: node.Parts.Count > 0
			if( node.WildcardAtStart )
				_sb.Append( "*" );
			this.Visit( node.Parts[0] );
			foreach( var part in node.Parts.Skip(1) ) {
				_sb.Append( "*" );
				this.Visit( part );
			}
			if( node.WildcardAtEnd )
				_sb.Append( "*" );
			return node;
		}

		private static string EscapeString( string p ) {
			return p
				.Replace( @"\", @"\5c" )
				.Replace( "*", @"\2a" )
				.Replace( "(", @"\28" )
				.Replace( ")", @"\29" )
				.Replace( ( (char)0 ).ToString(), @"\00" );
		}

	}
}
