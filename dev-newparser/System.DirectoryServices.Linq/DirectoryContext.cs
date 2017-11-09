using System.DirectoryServices.Linq.Attributes;
using System.DirectoryServices.Linq.ChangeTracking;
using System.DirectoryServices.Linq.EntryObjects;
using System.Linq;

namespace System.DirectoryServices.Linq
{
    public class DirectoryContext : DisposableObject
    {
        #region Fields

        private IChangeTracker _changeTracker;
        private DirectoryQueryProvider _provider;

        #endregion

        #region Constructors

        public DirectoryContext()
            : this(GetLdapConnectionString())
        {
        }

        public DirectoryContext(string connectionString)
            : this(new DirectoryEntry(connectionString))
        {
            ConnectionString = connectionString;
        }

        public DirectoryContext(string connectionString, string userName, string password)
            : this(new DirectoryEntry(connectionString, userName, password))
        {
            ConnectionString = connectionString;
        }

        public DirectoryContext(DirectoryEntry domainEntry)
        {
			RootEntry = domainEntry;
            domainEntry.AuthenticationType = AuthenticationTypes.Secure;
            EntrySetInitalizer.DiscoverAndInitiliseEntrySets(this);
        }

        #endregion

        #region Properties

		public DirectoryEntry RootEntry { get; private set; }

        public IChangeTracker ChangeTracker
        {
            get
            {
                if (_changeTracker == null)
                {
                    _changeTracker = GetChangeTracker();
                }

                return _changeTracker;
            }
        }

        public IQueryProvider QueryProvider
        {
            get
            {
                if (_provider == null)
                {
                    _provider = new DirectoryQueryProvider(this);
                }

                return _provider;
            }
        }

        public string ConnectionString { get; set; }

        #endregion

        #region Methods

        private DirectoryEntry GetParentDirectoryEntry(DirectoryTypeAttribute directoryType)
		{
			try
			{
				return RootEntry.Children.Find(directoryType.SchemaName);
			}
			catch (Exception)
			{
				return RootEntry;
			}
        }

        public static string GetLdapConnectionString()
        {
            using (var adRoot = new DirectoryEntry("LDAP://RootDSE"))
            {
                var dnc = Convert.ToString(adRoot.Properties["defaultNamingContext"][0]);
                var server = Convert.ToString(adRoot.Properties["dnsHostName"][0]);
                return string.Format("LDAP://{0}/{1}", server, dnc);
            }
        }

        protected override void Dispose(bool disposing)
        {
			if (disposing && RootEntry != null)
			{
				RootEntry.Dispose();
			}
        }

        protected virtual IChangeTracker GetChangeTracker()
        {
            return new ChangeTracker(this);
        }

        public IEntrySet<T> CreateEntrySet<T>() where T : class
        {
            return new EntrySet<T>(this);
        }

		public void AddObject<T>(T entry) where T : class
		{
			var entryObject = entry as EntryObject;

			if (entryObject != null)
			{
				AddObject(entryObject.GetCnValue(), entry);
			}
		}

		public void AddObject<T>(string cnName, T entry) where T : class
		{
			if (!string.IsNullOrEmpty(cnName) && entry != null)
			{
				var entryObject = entry as EntryObject;

				if (entryObject != null)
				{
					if (entryObject.Parent == null)
					{
						var schemaName = typeof (T).AssertGetAttribute<DirectoryTypeAttribute>();

						entryObject.SetParent(new NamedEntryObject
						{
							Entry = GetParentDirectoryEntry(schemaName)
						});
					}

					entryObject.Context = this;
					entryObject.AddToParent(cnName);
					ChangeTracker.AddObject(entryObject);
				}

				//TODO: Handle non-EntryObject types. - Stephen Baker
			}
		}

		public void DeleteObject<T>(T entry) where T : class
		{
			var entryObject = entry as EntryObject;

			if (entryObject != null)
			{
				entryObject.ChangeState = ChangeState.Delete;
				ChangeTracker.DeleteObject(entryObject);
			}

			//TODO: Handle non-EntryObject types. - Stephen Baker
		}

        public void SubmitChanges()
        {
            //EventLog.WriteEntry("Application", "SubmitChanges begin", //EventLogEntryType.Error);
            ChangeTracker.SubmitChanges();
            //EventLog.WriteEntry("Application", "SubmitChanges end", //EventLogEntryType.Error);
        }

        #endregion
    }
}
