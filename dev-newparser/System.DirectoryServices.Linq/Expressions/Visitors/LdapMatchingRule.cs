using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.DirectoryServices.Linq.Expressions {
	public class LdapMatchingRule {
		private readonly string _name;
		private readonly string _objectId;
		private readonly string _syntax;

		private LdapMatchingRule( string objectId )
			: this( objectId, null, null ) {
		}

		private LdapMatchingRule( string objectId, string name )
			: this( objectId, name, null ) {
		}

		private LdapMatchingRule( string objectId, string name, string syntaxOId ) {
			this._name = name;
			this._objectId = objectId;
			this._syntax = syntaxOId;
		}

		public string FilterExpression {
			get {
				if( _name != null )
					return _name;
				if( _objectId != null )
					return _objectId;
				return _syntax;
			}
		}

		public override string ToString() {
			return this.FilterExpression;
		}

		public static readonly LdapMatchingRule Object_Identifier = new LdapMatchingRule( "2.5.13.0", "objectIdentifierMatch", "1.3.6.1.4.1.1466.115.121.1.38" );
		public static readonly LdapMatchingRule Distinguished_Name = new LdapMatchingRule( "2.5.13.1", "distinguishedNameMatch", "1.3.6.1.4.1.1466.115.121.1.12" );
		public static readonly LdapMatchingRule Case_Ignore = new LdapMatchingRule( "2.5.13.2", "caseIgnoreMatch", "1.3.6.1.4.1.1466.115.121.1.15" );
		public static readonly LdapMatchingRule Numeric_String = new LdapMatchingRule( "2.5.13.8", "numericStringMatch", "1.3.6.1.4.1.1466.115.121.1.36" );
		public static readonly LdapMatchingRule Case_Ignore_List = new LdapMatchingRule( "2.5.13.11", "caseIgnoreListMatch", "1.3.6.1.4.1.1466.115.121.1.41" );
		public static readonly LdapMatchingRule Integer = new LdapMatchingRule( "2.5.13.14", "integerMatch", "1.3.6.1.4.1.1466.115.121.1.27" );
		public static readonly LdapMatchingRule Bit_String = new LdapMatchingRule( "2.5.13.16", "bitStringMatch", "1.3.6.1.4.1.1466.115.121.1.6" );
		public static readonly LdapMatchingRule Telephone_Number = new LdapMatchingRule( "2.5.13.20", "telephoneNumberMatch", "1.3.6.1.4.1.1466.115.121.1.50" );
		public static readonly LdapMatchingRule Presentation_Address = new LdapMatchingRule( "2.5.13.22", "presentationAddressMatch", "1.3.6.1.4.1.1466.115.121.1.43" );
		public static readonly LdapMatchingRule Unique_Member = new LdapMatchingRule( "2.5.13.23", "uniqueMemberMatch", "1.3.6.1.4.1.1466.115.121.1.34" );
		public static readonly LdapMatchingRule Protocol_Information = new LdapMatchingRule( "2.5.13.24", "protocolInformationMatch", "1.3.6.1.4.1.1466.115.121.1.42" );
		public static readonly LdapMatchingRule Generalized_Time = new LdapMatchingRule( "2.5.13.27", "generalizedTimeMatch", "1.3.6.1.4.1.1466.115.121.1.24" );
		public static readonly LdapMatchingRule Case_Exact_IA5 = new LdapMatchingRule( "1.3.6.1.4.1.1466.109.114.1", "caseExactIA5Match", "1.3.6.1.4.1.1466.115.121.1.26" );
		public static readonly LdapMatchingRule Case_Ignore_IA5 = new LdapMatchingRule( "1.3.6.1.4.1.1466.109.114.2", "caseIgnoreIA5Match", "1.3.6.1.4.1.1466.115.121.1.26" );
		//
		public static readonly LdapMatchingRule Bitwise_And = new LdapMatchingRule( "1.2.840.113556.1.4.803" );
		public static readonly LdapMatchingRule Bitwise_Or = new LdapMatchingRule( "1.2.840.113556.1.4.804" );
		public static readonly LdapMatchingRule In_Chain = new LdapMatchingRule( "1.2.840.113556.1.4.1941" );
	}
}
