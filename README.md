# [LinqToLdap](https://adlinq.codeplex.com/)
C# LINQ provider built on top of System.DirectoryServices.Protocols for querying and updating LDAP servers.

First thing to note is, LINQ 2 AD is built using the "oh so familiar" pattern LINQ to SQL and EntityFramework use, everything is queried using IQueryable<T> properties of a "Context". So, here is a sample context.

```
public class DirectoryContextMock : DirectoryContext
{
	public DirectoryContextMock() : base(string.Empty)
	{

	}

	private IEntrySet<User> _users;
	private IEntrySet<Group> _groups;

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
}

[DirectoryType("User", "OU=InternalUsers")]
public class User : UserEntryObject
{
	private Guid _id;
	private string _email;
	private string _userName;
	private string _firstName;
	private string _lastName;
	private DateTime? _whenChanged;

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

	[DirectoryProperty("whenchanged", true)]
	public DateTime? LastModifiedDate
	{
		get
		{
			return _whenChanged;
		}
		set
		{
			_whenChanged = value;
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

	[EntryCollectionProperty("member")]
	public EntryCollection<Group> Groups
	{
		get
		{
			return ((IEntryWithRelationships)this).RelationshipManager.GetEntryCollection<Group>("Groups");
		}
	}
}

[DirectoryType("group", "OU=Groups")]
public class Group : EntryObject
{
	[DirectoryProperty("samaccountname")]
	public string Name { get; set; }

	[EntryCollectionProperty("memberOf", MatchingRule = MatchingRuleType.InChain)]
	public EntryCollection<User> Users
	{
		get
		{
			return ((IEntryWithRelationships)this).RelationshipManager.GetEntryCollection<User>("Users");
		}
	}
}
```

The DirectoryTypeAttribute represents an ObjectClass in AD. The constructor of the attribute has 2 overloads. First, is the name of the ObjectClass and second, the optional Parent schema to add the class to.

The DirectoryPropertyAttribute represents an attribute to load from the parent ObjectClass.

Also, note that the User object inherits from UserEntryObject instead of EntryObject. This is optional, but it the UserEntryObject provides a .SetPassword(string password) method.
Now that we have our base context, lets do a basic query against AD.

```
[TestMethod]
public void FirstUserByUserNameTest()
{
	using (var context = new MockDirectoryContext())
	{
		var user = context.Users.FirstOrDefault(u => u.UserName == "sbaker");
		Assert.IsNotNull(user);
		Assert.AreEqual(user.FirstName, "Stephen");
	}
}
```

Now, that we have done a basic query, lets try some more complex queries.
```
[TestMethod]
public void WhereUserFirstNameSkip20Take10Test()
{
	using (var context = new MockDirectoryContext())
	{
		var users = context.Users.Where(u => u.FirstName.StartsWith("S"))
			.Skip(20)
			.Take(10)
			.OrderBy(u => u.LastName)
			.ToArray();

		Assert.IsTrue(users.Length == 10);
	}
}

[TestMethod]
public void LastUserFirstNameAndEmailIsNotNullTest()
{
	using (var context = new MockDirectoryContext())
	{
		var user = context.Users.LastOrDefault(u => u.FirstName == "Stephen" && u.Email != null);

		Assert.IsNotNull(user);
	}
}

[TestMethod]
public void WhereUserFirstNameIsStephenLastNameIsBakerOrSomeOtherTestSelectTest()
{
	using (var context = new MockDirectoryContext())
	{
		var users = context.Users.Where(u => u.FirstName == "Stephen" && (u.LastName == "SomeOtherLastName" || u.LastName == "Baker"))
			.OrderBy(u => u.LastName)
			.Select(u => new { Name = string.Concat(u.FirstName, " ", u.LastName) })
			.ToList();

		Assert.IsTrue(users.Count == 4);
		Assert.IsTrue(users.All(u => u.Name.StartsWith("Stephen")));
	}
}
```

You can also use LINQ syntax vs. Lambda.
```
[TestMethod]
public void FirstUserByIdTest()
{
	using (var context = new MockDirectoryContext())
	{
		var user = (from u in context.Users
			    where u.UserName == "sbaker"
			    select u).Single();

		Assert.IsNotNull(user);
		Assert.AreEqual(user.FirstName, "Stephen");
	}
}
```
You can also Add and Delete objects from AD.

```
[TestMethod]
public void AddAndDeleteNewUserTest()
{
	using (var context = new MockDirectoryContext())
	{
		var newUser = new User
		{
		    UserName = "sbaker",
		    FirstName = "Steve",
		    LastName = "Baker",
		    Email = "sbaker@logikbug.com"
		};

		context.AddObject(newUser);
		newUser.SetPassword("1234$#@!");
		context.SubmitChanges();

		var user = context.Users.Single(u => u.UserName == "sbaker");
		context.DeleteObject(user);
		context.SubmitChanges();
	}
}
```
