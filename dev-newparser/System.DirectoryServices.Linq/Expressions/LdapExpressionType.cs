using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	public enum LdapExpressionType{
		And = 3000,
		Or,
		//Not, -- no need for this, since the regular one will suffice
		Approx,
		Present,
		Substring,
		Extensible,
		Attribute,
		//
		Projection,
		DirectorySearcher,
		Sort,
	}
}
