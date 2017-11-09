using System;
using System.Collections.Generic;
using System.DirectoryServices.Linq.EntryObjects;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Linq {
	/// <summary>
	/// A class used to initialise <see cref="EntrySet"/>s in <see cref="DirectoryContext"/>s.
	/// </summary>
	/// <remarks>
	/// This class is inspired by the <c>DbSetDiscoveryService</c> class in the ASP.NET Entity Framework.
	/// Credit where it's due. :)
	/// </remarks>
	public static class EntrySetInitalizer
	{
		/// <summary>
		/// The generic DirectoryContext.CreateEntrySet method, used to create entry sets.
		/// </summary>
		private static readonly MethodInfo _createEntrySetMethodInfo =
			typeof(DirectoryContext).GetMethod("CreateEntrySet", Type.EmptyTypes);


		/// <summary>
		/// Find and initlize <see cref="EntrySet"/> properties in the given <see cref="DirectoryContext"/>.
		/// </summary>
		/// <param name="context">The context to initilise.</param>
		public static void DiscoverAndInitiliseEntrySets(DirectoryContext context) {
			var initDelegates = new List<Action<DirectoryContext>>();

			const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			var properties = context.GetType().GetProperties( bindingFlags ).Where(
				p => p.GetIndexParameters().Length == 0
				&& p.DeclaringType != typeof(DirectoryContext)
			);

			foreach(var propertyInfo in properties) {
				Type setType;
				if (!CanBeCreated(propertyInfo.PropertyType, out setType))
					continue;

				var setter = propertyInfo.GetSetMethod(nonPublic: false);
				if (setter != null) {
					// Basically, create the following lambda:
					// context => context.[EntrySetProperty] = context.CreateEntrySet<[setType]>()

					var contextParam = Expression.Parameter(typeof(DirectoryContext), "context");
					var newExpression = Expression.Call(contextParam, _createEntrySetMethodInfo.MakeGenericMethod(setType));
					var setExpression = Expression.Call(
						Expression.Convert(contextParam, context.GetType()),
						setter,
						newExpression
					);
					initDelegates.Add(
						Expression.Lambda<Action<DirectoryContext>>(setExpression, contextParam).Compile()
					);
				}
			}

			foreach(var action in initDelegates) {
				action.Invoke(context);
			}
		}


		/// <summary>
		/// Check whether the created type (<see cref="EntrySet"/>) can be assigned to the given type.
		/// </summary>
		/// <param name="type">The type of the property to assign to.</param>
		/// <param name="setType">The generic type parameter to this <see cref="EntrySet"/>.</param>
		/// <returns>True if the property type can be assigned to; false otherwise.</returns>
		private static bool CanBeCreated(Type type, out Type setType) {
			if (!IsEntrySet(type, out setType))
				return false;
			return type.IsAssignableFrom(typeof(EntrySet<>).MakeGenericType(setType));
		}


		/// <summary>
		/// Check whether the given type is an <see cref="IEntrySet"/>, and find its generic type parameter.
		/// </summary>
		/// <param name="type">The type to check.</param>
		/// <param name="setType">The generic type parameter to this <see cref="IEntrySet"/>.</param>
		/// <returns>True if the type is an <see cref="IEntrySet"/> type; false otherwise.</returns>
		private static bool IsEntrySet(Type type, out Type setType)
		{
			if (type.IsGenericType) {
				if (typeof(IEntrySet<>).IsAssignableFrom(type.GetGenericTypeDefinition())) {
					setType = type.GetGenericArguments()[0];
					return true;
				} else {
					setType = null;
					return false;
				}
			} else {
				try {
					Type interfaceImp = type.GetInterface(typeof(IEntrySet<>).FullName);
					if (interfaceImp != null && interfaceImp.IsGenericType) {
						setType = interfaceImp.GetGenericArguments()[0];
						return true;
					} else {
						setType = null;
						return false;
					}
				} catch (AmbiguousMatchException) {
					setType = null;
					return false;
				}
			}
		}

	}
}

