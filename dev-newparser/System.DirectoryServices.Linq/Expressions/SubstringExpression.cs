using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace System.DirectoryServices.Linq.Expressions {
	public class SubstringExpression : Expression {
		private readonly ReadOnlyCollection<Expression> _parts;
		private readonly bool _wildcardAtStart;
		private readonly bool _wildcardAtEnd;

		public SubstringExpression( bool wildcardAtStart, IEnumerable<Expression> parts, bool wildcardAtEnd ) {
			if( parts == null )
				throw new ArgumentNullException( "parts" );
			var readonlyParts = parts as ReadOnlyCollection<Expression>;
			if( readonlyParts == null )
				readonlyParts = new List<Expression>( parts ).AsReadOnly();
			this._parts = readonlyParts;
			this._wildcardAtStart = wildcardAtStart;
			this._wildcardAtEnd = wildcardAtEnd;
		}

		public ReadOnlyCollection<Expression> Parts { get { return _parts; } }
		public bool WildcardAtStart { get { return _wildcardAtStart; } }
		public bool WildcardAtEnd { get { return _wildcardAtEnd; } }

		public override ExpressionType NodeType {
			get {
				return (ExpressionType)LdapExpressionType.Substring;
			}
		}

		public override Type Type {
			get {
				// TODO: is this accurate? Does it matter?
				return typeof( string );
			}
		}

		public override string ToString() {
			return "(" + ( WildcardAtStart ? "*" : "" ) + string.Join( "*", Parts ) + ( WildcardAtEnd ? "*" : "" ) + ")";
		}

		public SubstringExpression Update( bool wildcardAtStart, IEnumerable<Expression> parts, bool wildcardAtEnd ) {
			if(
				wildcardAtStart == this.WildcardAtStart
				&& PartsAreEqual( parts )
				&& wildcardAtEnd == this.WildcardAtEnd
			) {
				return this;
			}
			return new SubstringExpression( wildcardAtStart, parts, wildcardAtEnd );
		}

		public SubstringExpression Update( IEnumerable<Expression> parts ) {
			return this.Update( this.WildcardAtStart, parts, this.WildcardAtEnd );
		}

		private bool PartsAreEqual( IEnumerable<Expression> parts ) {
			if( parts == this.Parts )
				return true;
			if( parts.Count() != this.Parts.Count )
				return false;

			return parts.Zip( this.Parts, ( x, y ) => x == y ).All( b => b );
		}

	}
}
