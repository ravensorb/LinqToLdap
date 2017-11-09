using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace System.DirectoryServices.Linq.Expressions {
	public class LdapSortExpression : Expression {
		private readonly Expression _expression;
		private readonly SortDirection _direction;

		public LdapSortExpression( Expression expression, SortDirection direction ) {
			this._expression = expression;
			this._direction = direction;
		}

		public Expression Expression { get { return _expression; } }
		public SortDirection Direction { get { return _direction; } }

		#region Expression details

		public override ExpressionType NodeType {
			get {
				return (ExpressionType) LdapExpressionType.Sort;
			}
		}

		public override string ToString() {
			return string.Format( "Sort {0} {1}", Direction==SortDirection.Ascending ? "asc" : "desc", Expression );
		}

		#endregion

		#region Update methods

		public LdapSortExpression Update( Expression expression ) {
			if( expression == this.Expression )
				return this;

			return new LdapSortExpression( expression, this.Direction );
		}

		#endregion
	}
}
