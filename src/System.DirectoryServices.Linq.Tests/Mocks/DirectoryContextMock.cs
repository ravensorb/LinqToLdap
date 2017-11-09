using System.DirectoryServices.Linq.Attributes;
using System.DirectoryServices.Linq.EntryObjects;

namespace System.DirectoryServices.Linq.Tests.Mocks
{
	public class DirectoryContextMock : DirectoryContext
	{
		public DirectoryContextMock() : base()
		{
		}

		public DirectoryContextMock(string connectionString, string username, string password) : base(connectionString, username, password)
		{
		}

		private IEntrySet<User> _users;
		private IEntrySet<Group> _groups;
		private IEntrySet<OU> _ous;

		public IEntrySet<User> Users
		{
			get
			{
				if (_users == null)
				{
					_users = CreateEntrySet<User>();
				}

				return _users;
			}
		}

		public IEntrySet<Group> Groups
		{
			get
			{
				if (_groups == null)
				{
					_groups = CreateEntrySet<Group>();
				}

				return _groups;
			}
		}

		public IEntrySet<OU> OrganizationUnits
		{
			get
			{
				if (_ous == null)
				{
					_ous = CreateEntrySet<OU>();
				}

				return _ous;
			}
		}
	}

	[DirectoryType("User", "OU=ExternalUsers")]
	public class User : UserEntryObject
	{
		private Guid _id;
		private string _employeeNumber;
		private string _email;
		private string _userName;
		private string _firstName;
		private string _lastName;

		[DirectoryProperty("objectguid", true)]
		public Guid Id
		{
			get
			{
				return _id;
			}
			set
			{
				if (_id != value)
				{
					_id = value;
					NotifyPropertyChanged("Id");
				}
			}
		}

		[DirectoryProperty("EmployeeNumber", true)]
		public string EmployeeNumber
		{
			get
			{
				return _employeeNumber;
			}
			set
			{
				_employeeNumber = value;
			}
		}

		[DirectoryProperty("samaccountname")]
		public string UserName
		{
			get
			{
				return _userName;
			}
			set
			{
				if (_userName != value)
				{
					_userName = value;
					NotifyPropertyChanged("UserName");
				}
			}
		}

		[DirectoryProperty("givenName")]
		public string FirstName
		{
			get
			{
				return _firstName;
			}
			set
			{
				if (_firstName != value)
				{
					_firstName = value;
					NotifyPropertyChanged("FirstName");
				}
			}
		}

		[DirectoryProperty("sn")]
		public string LastName
		{
			get
			{
				return _lastName;
			}
			set
			{
				if (_lastName != value)
				{
					_lastName = value;
					NotifyPropertyChanged("LastName");
				}
			}
		}

		[DirectoryProperty("mail")]
		public string Email
		{
			get
			{
				return _email;
			}
			set
			{
				if (_email != value)
				{
					_email = value;
					NotifyPropertyChanged("Email");
				}
			}
		}

		[EntryCollectionProperty("member", MatchingRule = MatchingRuleType.InChain)]
		public EntryCollection<Group> Groups
		{
			get
			{
				return ((IEntryWithRelationships)this).RelationshipManager.GetEntryCollection<Group>("Groups");
			}
		}
	}

	[DirectoryType("group", "OU=ExternalUsers")]
	public class Group : GroupEntryObject
	{
		private string _name;

		[DirectoryProperty("samaccountname")]
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (_name != value)
				{
					_name = value;
					NotifyPropertyChanged("Name");
				}
			}
		}

		[EntryCollectionProperty("memberOf", MatchingRule = MatchingRuleType.InChain)]
		public EntryCollection<User> Users
		{
			get
			{
				return ((IEntryWithRelationships)this).RelationshipManager.GetEntryCollection<User>("Users");
			}
		}
	}

	[DirectoryType("organizationalunit")]
	public class OU : EntryObject
	{
		[DirectoryProperty("name", true)]
		public string Name { get; set; }

		[EntryCollectionProperty(MatchingRule = MatchingRuleType.Children, Scope = SearchScope.OneLevel)]
		public EntrySetCollection<OU> Ous
		{
			get
			{
				return ((IEntryWithRelationships)this).RelationshipManager.GetEntrySetCollection<OU>("Ous");
			}
		}

		[EntryCollectionProperty(MatchingRule = MatchingRuleType.Children)]
		public EntrySetCollection<User> Users
		{
			get
			{
				return ((IEntryWithRelationships)this).RelationshipManager.GetEntrySetCollection<User>("Users");
			}
		}

		[EntryCollectionProperty(MatchingRule = MatchingRuleType.Children)]
		public EntrySetCollection<Group> Groups
		{
			get
			{
				return ((IEntryWithRelationships)this).RelationshipManager.GetEntrySetCollection<Group>("Groups");
			}
		}
	}
}