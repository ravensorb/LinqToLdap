using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public class LdapAttributeExpression : Expression {
		private readonly string _name;
		private readonly Type _type;

		public LdapAttributeExpression( Type type, string name ) {
			if( type == null )
				throw new ArgumentNullException( "type" );
			if( name == null )
				throw new ArgumentNullException( "name" );
			this._type = type;
			this._name = name;
		}

		public string Name {
			get { return _name; }
		}

		#region Expression details

		public override ExpressionType NodeType {
			get {
				return (ExpressionType)LdapExpressionType.Attribute;
			}
		}

		public override Type Type {
			get {
				return _type;
			}
		}

		public override string ToString() {
			return string.Format( "Attribute {0}", Name );
		}

		#endregion
	}
}
