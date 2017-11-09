using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public class LdapApproxExpression : Expression {
		private readonly Expression _left;
		private readonly Expression _right;

		public LdapApproxExpression( Expression left, Expression right ) {
			this._left = left;
			this._right = right;
		}

		public Expression Left { get { return this._left; } }
		public Expression Right { get { return this._right; } }

		#region Expression details

		public override ExpressionType NodeType {
			get {
				return (ExpressionType)LdapExpressionType.Approx;
			}
		}

		public override Type Type {
			get {
				return typeof( bool );
			}
		}

		public override string ToString() {
			return string.Format( "{0} approx {1}", Left, Right );
		}

		#endregion

		#region Update methods

		public LdapApproxExpression Update( Expression left, Expression right ) {
			if( left == this.Left && right == this.Right )
				return this;

			return new LdapApproxExpression( left, right );
		}

		#endregion
	}
}
