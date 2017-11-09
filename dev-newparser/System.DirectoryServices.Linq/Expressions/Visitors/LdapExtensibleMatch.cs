using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace System.DirectoryServices.Linq.Expressions {
	public class LdapExtensibleMatch : Expression {
		private readonly Expression _attribute;
		private readonly bool _includeDistinguishedName;
		private readonly LdapMatchingRule _matchingRule;
		private readonly Expression _value;

		public LdapExtensibleMatch( Expression attribute, bool includeDistinguishedName, LdapMatchingRule matchingRule, Expression value ) {
			this._attribute = attribute;
			this._includeDistinguishedName = includeDistinguishedName;
			this._matchingRule = matchingRule;
			this._value = value;
		}

		public Expression Attribute { get { return _attribute; } }
		public bool IncludeDistinguishedName { get { return _includeDistinguishedName; } }
		public LdapMatchingRule MatchingRule { get { return _matchingRule; } }
		public Expression Value { get { return _value; } }

		#region Expression methods

		public override ExpressionType NodeType {
			get {
				return (ExpressionType)LdapExpressionType.Extensible;
			}
		}

		public override Type Type {
			get {
				return typeof( bool );
			}
		}

		public override string ToString() {
			return string.Format(
				"{0}:{1}{2}:={3}",
				Attribute,
				IncludeDistinguishedName ? "dn:" : string.Empty,
				MatchingRule,
				Value
			);
		}

		#endregion

		#region Update methods

		public LdapExtensibleMatch Update( Expression attribute, Expression value ) {
			if( attribute == this.Attribute && value == this.Value )
				return this;
			return new LdapExtensibleMatch( attribute, this.IncludeDistinguishedName, MatchingRule, value );
		}

		#endregion

	}
}
