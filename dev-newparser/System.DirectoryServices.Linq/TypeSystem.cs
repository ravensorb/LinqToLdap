﻿using System.Collections.Generic;
using System.Reflection;

namespace System.DirectoryServices.Linq {

	// Code from Matt Warren (http://blogs.msdn.com/mattwar/pages/linq-links.aspx)

	/// <summary>
	/// Type related helper methods
	/// </summary>
	internal static class TypeSystem {
		private static Type FindIEnumerable( Type seqType ) {
			if( seqType == null || seqType == typeof( string ) )
				return null;
			if( seqType.IsArray )
				return typeof( IEnumerable<> ).MakeGenericType( seqType.GetElementType() );
			if( seqType.IsGenericType ) {
				foreach( Type arg in seqType.GetGenericArguments() ) {
					Type ienum = typeof( IEnumerable<> ).MakeGenericType( arg );
					if( ienum.IsAssignableFrom( seqType ) ) {
						return ienum;
					}
				}
			}
			Type[] ifaces = seqType.GetInterfaces();
			if( ifaces != null && ifaces.Length > 0 ) {
				foreach( Type iface in ifaces ) {
					Type ienum = FindIEnumerable( iface );
					if( ienum != null ) return ienum;
				}
			}
			if( seqType.BaseType != null && seqType.BaseType != typeof( object ) ) {
				return FindIEnumerable( seqType.BaseType );
			}
			return null;
		}
		internal static Type GetSequenceType( Type elementType ) {
			return typeof( IEnumerable<> ).MakeGenericType( elementType );
		}
		internal static Type GetElementType( Type seqType ) {
			Type ienum = FindIEnumerable( seqType );
			if( ienum == null ) return seqType;
			return ienum.GetGenericArguments()[0];
		}
		internal static bool IsNullableType( Type type ) {
			return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Nullable<> );
		}
		internal static bool IsNullAssignable( Type type ) {
			return !type.IsValueType || IsNullableType( type );
		}
		internal static Type GetNonNullableType( Type type ) {
			if( IsNullableType( type ) ) {
				return type.GetGenericArguments()[0];
			}
			return type;
		}
		internal static Type GetMemberType( MemberInfo mi ) {
			FieldInfo fi = mi as FieldInfo;
			if( fi != null ) return fi.FieldType;
			PropertyInfo pi = mi as PropertyInfo;
			if( pi != null ) return pi.PropertyType;
			EventInfo ei = mi as EventInfo;
			if( ei != null ) return ei.EventHandlerType;
			return null;
		}
	}
}