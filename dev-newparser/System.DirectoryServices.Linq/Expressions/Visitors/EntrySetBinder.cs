using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq.Expressions {
	class EntrySetBinder {
		private static IEnumerable<MemberInfo> GetMappedMembers( Type type ) {
			return type.GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
				.Where( p => p.CanWrite )
				.Where( p => p.DeclaringType != typeof( EntryObjects.EntryObject ) );
		}

		private static Type GetMemberType( MemberInfo member ) {
			FieldInfo fi = member as FieldInfo;
			if( fi != null ) {
				return fi.FieldType;
			}
			PropertyInfo pi = (PropertyInfo)member;
			return pi.PropertyType;
		}

		internal static Expression GetEntrySetProjection(EntryObjects.EntrySet entrySet) {
			Type type = ((IQueryable)entrySet).ElementType;
			string filterType = type.AssertGetAttribute<System.DirectoryServices.Linq.Attributes.DirectoryTypeAttribute>().Name;

			List<MemberBinding> bindings = new List<MemberBinding>();
			List<LdapAttributeExpression> attributes = new List<LdapAttributeExpression>();

			foreach( MemberInfo memberInfo in GetMappedMembers( type ) ) {
				string attributeName = GetAttributeName( memberInfo );
				Type memberType = GetMemberType( memberInfo );

				//	if (property.CanWrite && result.Properties.Contains(attributeName))
				var attributeExpression = new LdapAttributeExpression( memberType, attributeName );
				bindings.Add( Expression.Bind( memberInfo, attributeExpression ) );
				attributes.Add( attributeExpression );
			}

			Expression projector = Expression.MemberInit( Expression.New( type ), bindings );

			Type resultType = ( typeof( IEnumerable<> ).MakeGenericType( type ) );

			// Set the filter to include the object class
			Expression filter = Expression.Equal(
				new LdapAttributeExpression( typeof( string ), "ObjectClass" ),
				Expression.Constant( filterType )
			);
			DirectoryEntry searchRoot = entrySet.RootEntry;
			SearchScope scope = SearchScope.Subtree;
			if( entrySet.Scope.HasValue )
				scope = entrySet.Scope.Value;

			return new ProjectionExpression(
				new DirectorySearcherExpression(
					resultType,
					searchRoot,
					attributes,
					filter,
					scope,
					null,
					null,
					null
				),
				projector,
				null
			);
		}

		internal static Expression GetEntryCollectionProjection(EntryObjects.EntryCollection2 entryCollection) {
			throw new NotImplementedException();
		}

		internal static string GetAttributeName( MemberInfo info ) {
			var attribute = info.GetAttribute<Attributes.DirectoryPropertyAttribute>();
			if( attribute != null && !string.IsNullOrEmpty( attribute.Name ) ) {
				return attribute.Name;
			}
			return info.Name;
		}
	}
}
