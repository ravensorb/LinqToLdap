using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public static class LdapExpressionFactory {
		public static NaryExpression And( IEnumerable<Expression> clauses ) {
			return new NaryExpression( (ExpressionType)LdapExpressionType.And, typeof( bool ), clauses );
		}

		public static NaryExpression Or( IEnumerable<Expression> clauses ) {
			return new NaryExpression( (ExpressionType)LdapExpressionType.Or, typeof( bool ), clauses );
		}

		public static LdapApproxExpression Approx( Expression left, Expression right ) {
			return new LdapApproxExpression( left, right );
		}

		public static LdapAttributePresentExpression Present( Expression operand ) {
			return new LdapAttributePresentExpression( operand );
		}

		public static SubstringExpression Substring( bool wildcardAtStart, IEnumerable<Expression> parts, bool wildcardAtEnd ) {
			return new SubstringExpression( wildcardAtStart, parts, wildcardAtEnd );
		}
	}
}
