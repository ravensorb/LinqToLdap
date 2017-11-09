using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public class NaryExpression : Expression {
		private readonly ReadOnlyCollection<Expression> _clauses;
		private readonly ExpressionType _nodeType;
		private readonly System.Type _type;

		public NaryExpression( ExpressionType nodeType, Type type, IEnumerable<Expression> clauses ) {
			this._clauses = clauses as ReadOnlyCollection<Expression>;
			if( this._clauses == null )
				this._clauses = new List<Expression>( clauses ).AsReadOnly();
			this._nodeType = nodeType;
			this._type = type;
		}

		public ReadOnlyCollection<Expression> Clauses {
			get {
				return _clauses;
			}
		}

		#region Expression details

		public override ExpressionType NodeType {
			get {
				return _nodeType;
			}
		}

		public override Type Type {
			get {
				return _type;
			}
		}

		public override string ToString() {
			if( NodeType == (ExpressionType)LdapExpressionType.Or )
				return GetConjunctionString( "Or" );
			if( NodeType == (ExpressionType)LdapExpressionType.And )
				return GetConjunctionString( "And" );

			return string.Format(
				"({0} {1})",
				NodeType,
				string.Join( " ", Clauses.Select( c => c.ToString() ) )
			);
		}

		private string GetConjunctionString( string conjunction ) {
			return "(" + string.Join( " " + conjunction + " ", this.Clauses ) + ")";
		}

		#endregion

		#region Update methods

		internal NaryExpression Update( IEnumerable<Expression> clauses ) {
			if( clauses == this.Clauses )
				return this;

			return new NaryExpression( this.NodeType, this.Type, clauses );
		}

		#endregion
	}
}
