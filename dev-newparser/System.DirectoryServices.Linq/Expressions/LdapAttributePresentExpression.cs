using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public class LdapAttributePresentExpression : Expression {
		private readonly Expression _operand;

		public LdapAttributePresentExpression( Expression operand ) {
			_operand = operand;
		}

		public Expression Operand {
			get { return _operand; }
		}

		#region Expression details

		public override ExpressionType NodeType {
			get {
				return (ExpressionType)LdapExpressionType.Present;
			}
		}

		public override Type Type {
			get {
				return typeof( bool );
			}
		}

		public override string ToString() {
			return string.Format( "{0} present", Operand );
		}

		#endregion

		internal LdapAttributePresentExpression Update( Expression operand ) {
			if( operand == this.Operand )
				return this;

			return new LdapAttributePresentExpression( operand );
		}
	}
}
