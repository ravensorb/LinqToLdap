using System.DirectoryServices.Linq.Attributes;
using System.DirectoryServices.Linq.EntryObjects;
using System.Reflection;

namespace System.DirectoryServices.Linq
{
    public class ResultMapper : IResultMapper
    {
        #region Constructors

        public ResultMapper(DirectoryContext context)
        {
            Context = context;
        }

        #endregion

        #region Properties

        public DirectoryContext Context { get; set; }

        #endregion

        #region Methods

        public virtual T Map<T>(SearchResult result)
        {
            var value = Map(typeof(T), result);

            if (value != null)
            {
                return (T)value;
            }

            return default(T);
        }

        public virtual object Map(Type type, SearchResult result)
        {
            if (result != null)
            {
                var mappedObject = Activator.CreateInstance(type);

                foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    MapProperty(mappedObject, property, result);
                }

                if (typeof(EntryObject).IsAssignableFrom(type))
                {
                    var entryObject = (EntryObject)mappedObject;
                    entryObject.Context = Context;
					entryObject.ChangeState = ChangeState.None;
					entryObject.ADPath = result.Path;
					entryObject.Entry = result.GetDirectoryEntry();
					entryObject.SetParent(entryObject.Entry.Parent);
                    Context.ChangeTracker.TrackChanges(entryObject);
                }

                return mappedObject;
            }

            return null;
        }

        private static void MapProperty(object mappedObject, PropertyInfo property, SearchResult result)
        {
            string attributeName = GetAttributeName(property);

            if (property.CanWrite && result.Properties.Contains(attributeName))
            {
                var resultPropertyCollection = result.Properties[attributeName];

                if (resultPropertyCollection.Count == 1)
                {
                    object value = resultPropertyCollection[0];

                    value = GetResultValue(property.PropertyType, value);

                    property.SetValue(mappedObject, value, null);
                }
                else
                {
                    MapCollectionProperty(resultPropertyCollection, property);
                }
            }
        }

        private static object GetResultValue(Type propertyType, object value)
        {
            if (value != null)
            {
                var valueType = value.GetType();

                if (valueType != propertyType)
                {
                    if (valueType == typeof(byte[]) && propertyType == typeof(Guid))
                    {
                        return new Guid((byte[])value);
                    }

                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        return Activator.CreateInstance(propertyType, value);// new[] { value });
                    }

                    return Convert.ChangeType(value, propertyType);
                }
            }

            return value;
        }

        private static void MapCollectionProperty(ResultPropertyValueCollection resultPropertyCollection, PropertyInfo property)
        {
        }

        private static string GetAttributeName(MemberInfo info)
        {
            var attribute = info.GetAttribute<DirectoryPropertyAttribute>();

            if (attribute != null && !string.IsNullOrEmpty(attribute.Name))
            {
                return attribute.Name;
            }

            return info.Name;
        }

        #endregion
    }
}
