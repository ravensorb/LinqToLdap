using System.ComponentModel;
using System.DirectoryServices.Linq.Attributes;

namespace System.DirectoryServices.Linq.EntryObjects
{
    public abstract class EntryObject : DisposableObject, IEntryWithRelationships, INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Fields

        private const int UF_DONT_EXPIRE_PASSWD = 0x10000;

        private IRelationshipManager _relationshipManager;

        #endregion

        #region Constructors



        #endregion

        #region Properties

        IRelationshipManager IEntryWithRelationships.RelationshipManager
        {
            get
            {
                if (_relationshipManager == null)
                {
                    _relationshipManager = GetRelationshipManager();
                }

                return _relationshipManager;
            }
        }

        internal Type ElementType
        {
            get
            {
                return GetType();
            }
        }

        internal string ADPath { get; set; }

        internal DirectoryEntry Entry { get; set; }

        internal ChangeState ChangeState { get; set; }

        internal DirectoryContext Context { get; set; }

		internal EntryObject Parent { get; set; }

		[DirectoryProperty("distinguishedName")]
		internal string InternalDn { get; set; }
        #endregion

        #region Methods
		private string GetFullDn(string cn, string schemaName)
		{
			if (!string.IsNullOrEmpty(schemaName) && schemaName.ToLower() == "organizationalunit")
			{
				return string.Concat("OU=", cn);
			}

			return string.Concat("CN=", cn);
		}

        internal string GetCnValue()
        {
            string givenName = null;
            string surName = null;
            string accountName = null;

            foreach (var property in ElementType.GetProperties())
            {
                var attr = property.GetAttribute<DirectoryPropertyAttribute>();

                if (attr == null)
                {
                    continue;
                }

                switch ((attr.Name ?? string.Empty).ToLower())
                {
					case "cn":
					{
						return Convert.ToString(property.GetValue(this, null));
					}
					case "name":
                        {
                            return Convert.ToString(property.GetValue(this, null));
                        }
                    case "samaccountname":
                        {
                            accountName = Convert.ToString(property.GetValue(this, null));
                            break;
                        }
                    case "givenname":
                        {
                            givenName = Convert.ToString(property.GetValue(this, null));
                            break;
                        }
                    case "sn":
                        {
                            surName = Convert.ToString(property.GetValue(this, null));
                            break;
                        }
                    default:
                        {
                            continue;
                        }
                }

                if (!string.IsNullOrEmpty(accountName))
                {
                    return accountName;
                }

                if (!string.IsNullOrEmpty(givenName) && !string.IsNullOrEmpty(surName))
                {
                    break;
                }
            }

            return string.Concat(givenName, " ", surName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Entry != null)
            {
                Entry.Dispose();
            }
        }

        protected virtual IRelationshipManager GetRelationshipManager()
        {
            return new RelationshipManager(this);
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null && !string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
		}

		public void SetParent(DirectoryEntry parent)
		{
			if (parent == null)
			{
				throw new ArgumentNullException("parent");
			}

			SetParent(new NamedEntryObject
			{
				Context = Context,
				ADPath = parent.Path,
				Entry = parent
			});
		}

		public void SetParent(EntryObject parent)
		{
			if (parent == null)
			{
				throw new ArgumentNullException("parent");
			}

			if (Parent == parent) 
			{
				return;
			}

			Parent = parent;
			//SetParent(GetCnValue(), parent);
		}

		public void AddToParent(string cn)
		{
			var schemaName = this.GetType().AssertGetAttribute<DirectoryTypeAttribute>();

			if (Entry != null)
			{
				Parent.Entry.Children.Remove(Entry);
				Parent.Entry.CommitChanges();
			}
			
			Entry = Parent.Entry.Children.Add(GetFullDn(cn, schemaName.Name), schemaName.Name);
		}

        public void AddToAttribute(string attribute, object value)
        {
            if (!string.IsNullOrEmpty(attribute))
            {
                Entry.Properties[attribute].Add(value);
                Context.ChangeTracker.SetEntryObjectChanged(this);
            }
        }

        public void AddToAttribute(string attribute, params object[] values)
        {
            if (!string.IsNullOrEmpty(attribute) && values.Length > 0)
            {
                Entry.Properties[attribute].AddRange(values);
                Context.ChangeTracker.SetEntryObjectChanged(this);
            }
        }

        public void SetAttribute(string attribute, object value)
        {
            if (!string.IsNullOrEmpty(attribute))
            {
                Entry.Properties[attribute].Value = value;
                Context.ChangeTracker.SetEntryObjectChanged(this);
            }
        }

        public void SetAttribute(string attribute, params object[] values)
        {
            if (!string.IsNullOrEmpty(attribute))
            {
                var property = Entry.Properties[attribute];

                if (property.Count > 0)
                {
                    property.Clear();
                }

                property.AddRange(values);
                Context.ChangeTracker.SetEntryObjectChanged(this);
            }
        }

        public T GetAttribute<T>(string attribute)
        {
            if (Entry.Properties.Contains(attribute))
            {
                return (T)Entry.Properties[attribute].Value;
            }

            return default(T);
        }

        public DateTime ParseDateTime(string attribute)
        {
            if (Entry.Properties.Contains(attribute))
            {
                object value = Entry.Properties[attribute].Value;

                if (value != null && value is long)
                {
                    return DateTime.FromFileTime((long)value);
                }
            }

            return DateTime.MinValue;
        }

        #endregion
    }
}