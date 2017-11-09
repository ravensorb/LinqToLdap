using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.Linq.Attributes;
using System.DirectoryServices.Linq.EntryObjects;
using System.Reflection;

namespace System.DirectoryServices.Linq.ChangeTracking
{
    public class ChangeTracker : IChangeTracker
    {
        #region Constructors

        public ChangeTracker(DirectoryContext context)
        {
            Context = context;
            Changes = new Dictionary<EntryObject, List<string>>();
        }

        #endregion

        #region Properties

        public DirectoryContext Context { get; set; }

        public IDictionary<EntryObject, List<string>> Changes { get; set; }

        #endregion

        #region Methods

        private static DirectoryPropertyAttribute GetAttribute(MemberInfo info)
        {
            return info.GetAttribute<DirectoryPropertyAttribute>();
        }

        private static object GetPropertyValue(EntryObject entry, PropertyInfo property)
        {
            var value = property.GetValue(entry, null);

            if (value is Guid)
            {
                return ((Guid)value).ToByteArray();
            }

            return value;
        }

        private static void UpdateEntry(EntryObject entry, IEnumerable<string> properties)
        {
            var entryType = entry.GetType();

            foreach (var name in properties)
            {
                var property = entryType.GetProperty(name);
				var attribute = GetAttribute(property);

				if (attribute == null || attribute.IsReadOnly)
				{
					continue;
				}

				var value = GetPropertyValue(entry, property);

                if (value != null)
                {
                    entry.Entry.Properties[attribute.Name].Value = value;
                }
                else
				{
					entry.Entry.Properties[attribute.Name].Clear();
                }
            }
        }

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var entryObject = sender as EntryObject;

			if (entryObject != null)
			{
				var property = entryObject.GetType().GetProperty(e.PropertyName);

				if (property == null) { throw new InvalidOperationException(string.Concat("Property must be defined: ", e.PropertyName)); }

				var attribute = GetAttribute(property);

				if (attribute != null && !attribute.IsReadOnly)
				{
					var properties = GetPropertyList(entryObject);

					if (!properties.Contains(e.PropertyName))
					{
						properties.Add(e.PropertyName);

						if (entryObject.ChangeState != ChangeState.Insert && entryObject.ChangeState != ChangeState.Delete)
						{
							entryObject.ChangeState = ChangeState.Update;
						}
					}
				}
			}
		}

        private List<string> GetAllPropertyNames(EntryObject entryObject)
        {
            var type = entryObject.GetType();
            var propertyNames = new List<string>();

            foreach (var item in type.GetProperties())
            {
                var attribute = GetAttribute(item);

                if (attribute == null || string.IsNullOrEmpty(attribute.Name) || attribute.IsReadOnly)
                {
                    continue;
                }

                propertyNames.Add(item.Name);
            }

            return propertyNames;
        }

        private List<string> GetPropertyList(EntryObject entryObject)
        {
            if (!Changes.ContainsKey(entryObject))
            {
                Changes.Add(entryObject, new List<string>());
            }

            return Changes[entryObject];
        }

        public void SetEntryObjectChanged(EntryObject entryObject)
        {
            if (!Changes.ContainsKey(entryObject))
            {
				entryObject.ChangeState = ChangeState.Update;
                Changes.Add(entryObject, new List<string>());
            }
        }

        public void TrackChanges<T>(T entryObject) where T : EntryObject
        {
            if (!Equals(entryObject, default(T)))
            {
                entryObject.PropertyChanged += OnPropertyChanged;
            }
        }

        public void AddObject<T>(T entryObject) where T : EntryObject
        {
            if (!Equals(entryObject, default(T)))
            {
                TrackChanges(entryObject);
                var properties = GetPropertyList(entryObject);
                properties.AddRange(GetAllPropertyNames(entryObject));

				if (entryObject.Entry != null)
				{
					entryObject.Entry.CommitChanges();
				}

	            entryObject.ChangeState = ChangeState.Insert;
            }
        }

        public void DeleteObject<T>(T entryObject) where T : EntryObject
        {
            var properties = GetPropertyList(entryObject);

            if (properties.Count > 0)
            {
                properties.Clear();
            }
        }

        public virtual void SubmitChanges()
        {
            foreach (var item in Changes)
            {
                var entry = item.Key;

                if (entry.ChangeState == ChangeState.Insert)
                {
                    entry.Entry.RefreshCache();
                    entry.ChangeState = ChangeState.Update;
                }

                if (entry.ChangeState == ChangeState.Update)
                {
                    UpdateEntry(entry, item.Value);
                    entry.Entry.CommitChanges();
                }
                else if (entry.ChangeState == ChangeState.Delete)
                {
                    entry.Entry.DeleteTree();
                }
            }

            Changes.Clear();
        }

        #endregion
    }
}