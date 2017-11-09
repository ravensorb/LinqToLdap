using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.DirectoryServices.Linq
{
	internal static class TypeExtensions
	{
		public static T GetAttribute<T>(this ICustomAttributeProvider info) where T : Attribute
		{
			return info.GetAttributes<T>().FirstOrDefault<T>();
		}

		public static IEnumerable<T> GetAttributes<T>(this ICustomAttributeProvider info) where T : Attribute
		{
			return info.GetCustomAttributes(typeof(T), true).Cast<T>();
		}

		public static TAttribute AssertGetAttribute<TAttribute>(this ICustomAttributeProvider info) where TAttribute : Attribute
		{
			var attribute = info.GetAttribute<TAttribute>();

			if (attribute == null)
			{
				throw new InvalidOperationException(
					string.Format(
					"The type or property must have the Attribute: '{0}'.",
					typeof(TAttribute).FullName)
				);
			}

			return attribute;
		}
	}
}
