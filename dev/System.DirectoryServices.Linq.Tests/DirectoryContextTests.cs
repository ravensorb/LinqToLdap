using System.DirectoryServices.Linq.Tests.Mocks;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.DirectoryServices.Linq.Tests
{
	/// <summary>
	/// Summary description for DirectoryContextTests
	/// </summary>
	[TestClass]
	public class DirectoryContextTests
	{
		private const string Username = "username";
		private const string Password = "password";
		private const string ConnectionString = "LDAP://homeserver.local/DC=homeserver,DC=local";
		private string ouName = "TestOu";

		private DirectoryEntry _rootEntry;
		private DirectoryEntry _ouEntry;

		#region Setup and teardown methods

		/// <summary>
		/// Run before each test, create a new OU for testing purposes.
		/// </summary>
		[TestInitialize]
		public void TestInit() {
			if( _rootEntry == null )
				_rootEntry = new DirectoryEntry( ConnectionString, Username, Password );
			if( _ouEntry != null )
				throw new InvalidOperationException( "OuEntry is already set!" );
			_ouEntry = CreateRootOu( _rootEntry, ouName );
		}

		/// <summary>
		/// Run after each test, delete the test OU and all contained within it.
		/// </summary>
		[TestCleanup]
		public void TestCleanup() {
			_ouEntry.RefreshCache();
			RecursiveRemove( _ouEntry );
			_rootEntry.RefreshCache();
			_rootEntry.Children.Remove( _ouEntry );
		}

		/// <summary>
		/// Create the OU used for testing.
		/// </summary>
		/// <param name="rootEntry">The parent of the test OU to create.</param>
		/// <param name="ouName">The name of the OU to create.</param>
		/// <returns>A <see cref="DirectoryEntry"/> object corresponding to the new OU.</returns>
		private static DirectoryEntry CreateRootOu( DirectoryEntry rootEntry, string ouName ) {
			rootEntry.RefreshCache();
			var ouEntry = rootEntry.Children.Add( "OU=" + ouName, "OrganizationalUnit" );
			ouEntry.CommitChanges();
			return ouEntry;
		}

		/// <summary>
		/// Recursively remove all the desecendents of a given <see cref="DirectoryEntry"/>.
		/// </summary>
		/// <param name="entry">The entry to remove the descendents of.</param>
		private static void RecursiveRemove( DirectoryEntry entry ) {
			foreach( DirectoryEntry child in entry.Children ) {
				RecursiveRemove( child );
				entry.Children.Remove( child );
			}
		}

		/// <summary>
		/// Create a user in the test OU (without exercising the code under test).
		/// </summary>
		/// <param name="username">The username of the new user. Required.</param>
		/// <param name="firstName">The first name.</param>
		/// <param name="lastName">The last name.</param>
		private void CreateTestUser( string username, string firstName, string lastName ) {
			CreateTestUser( username, firstName, lastName, null );
		}

		/// <summary>
		/// Create a user in the test OU (without exercising the code under test).
		/// </summary>
		/// <param name="username">The username of the new user. Cannot be null.</param>
		/// <param name="firstName">The first name.</param>
		/// <param name="lastName">The last name.</param>
		/// <param name="email">The email address.</param>
		private void CreateTestUser( string username, string firstName, string lastName, string email ) {
			if( username == null )
				throw new ArgumentNullException( "username" );
			var userEntry = _ouEntry.Children.Add( "CN=" + username, "User" );
			userEntry.Properties["samAccountName"].Add( username );
			if( firstName != null )
				userEntry.Properties["givenName"].Add( firstName );
			if( lastName != null )
				userEntry.Properties["sn"].Add( lastName );
			if( email != null )
				userEntry.Properties["mail"].Add( email );
			userEntry.CommitChanges();
		}

		/// <summary>
		/// Create a group in the test OU (without exercising the code under test).
		/// </summary>
		/// <param name="groupName">The name of the new group. Cannot be null.</param>
		private void CreateTestGroup( string groupName ) {
			if( groupName == null )
				throw new ArgumentNullException( "groupName" );
			var groupEntry = _ouEntry.Children.Add( "CN=" + groupName, "Group" );
			groupEntry.Properties["samAccountName"].Add( groupName );
			groupEntry.CommitChanges();
		}

		/// <summary>
		/// Create a sub-OU in the test OU (without exercising the code under test).
		/// </summary>
		/// <param name="ouName">The name of the new OU. Cannot be null.</param>
		private void CreateTestOu( string ouName ) {
			if( ouName == null )
				throw new ArgumentNullException( "ouName" );
			var ouEntry = _ouEntry.Children.Add( "OU=" + ouName, "OrganizationalUnit" );
			ouEntry.CommitChanges();
		}

		#endregion

		[TestMethod]
		public void AddAndDeleteNewUserSubmitChangesTest()
		{
			var single = new User {
				UserName = "sbaker",
				FirstName = "Steve",
				LastName = "Baker",
				Email = "sbaker@homeserver.local"
			};
			string password = "Wh@7Wh@7";
			string username = single.UserName;

			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.First(o => o.Name == ouName);
				single.SetParent(ou);
				context.AddObject(single);
				single.SetPassword(password);
				context.SubmitChanges();

				var single1 = context.Users.Single(u => u.UserName == username);
				context.DeleteObject(single1);
				context.SubmitChanges();
				var single2 = context.Users.SingleOrDefault(u => u.UserName == username);
				Assert.IsNull(single2);
			}
		}

		[TestMethod]
		public void AddAndDeleteNewOuSubmitChangesTest()
		{
			string newOuName = "ChildOU";
			
			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = new OU {Name = newOuName};
				ou.SetParent(context.OrganizationUnits.First(u => u.Name == ouName));
				context.AddObject(ou);
				context.SubmitChanges();

				var childOu = context.OrganizationUnits.First(u => u.Name == newOuName);
				context.DeleteObject(childOu);
				context.SubmitChanges();
			}
		}

		[TestMethod]
		public void AddAndDeleteNewGroupSubmitChangesTest()
		{
			string newGroupName = "TestGroup";

			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var newGroup = new Group {Name = newGroupName};
				newGroup.SetParent(context.OrganizationUnits.First(u => u.Name == ouName));
				context.AddObject(newGroup);
				context.SubmitChanges();

				var group = context.Groups.First(u => u.Name == newGroupName);
				context.DeleteObject(group);
				context.SubmitChanges();
			}
		}

		[TestMethod]
		public void MultipleOuSelectTest()
		{
			int expected = 2;
			CreateTestOu("Test1");
			CreateTestOu("Test2");

			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == ouName);
				var result = ou.Ous.Where(u => u.Name != null).ToArray();
				Assert.AreEqual(expected, result.Length);
			}
		}

		[TestMethod]
		public void FirstOuSelectTest()
		{
			string expected = "Test1";
			CreateTestOu("Test1");
			CreateTestOu("Test2");

			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == ouName);
				var result = ou.Ous.First(u => u.Name != null);
				Assert.AreEqual(expected, result.Name);
			}
		}

		[TestMethod]
		public void LastOuSelectTest()
		{
			string expected = "Test2";
			CreateTestOu("Test1");
			CreateTestOu("Test2");

			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == ouName);
				var result = ou.Ous.Last(u => u.Name != null);
				Assert.AreEqual(expected, result.Name);
			}
		}

		[TestMethod]
		public void MultipleUserSelectTest()
		{
			int expected = 3;
			CreateTestUser("TestUser1", "Test1", null);
			CreateTestUser("TestUser2", "Test2", null);
			CreateTestUser("TestUser3", "Test3", null);

			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == ouName);
				var user = ou.Users.Where(u => u.FirstName != null).ToArray();
				Assert.AreEqual(expected, user.Length);
			}
		}

		[TestMethod]
		public void FirstUserSelectTest()
		{
			string expected = "Test1";
			CreateTestUser("TestUser1", "Test1", null);
			CreateTestUser("TestUser2", "Test2", null);
			CreateTestUser("TestUser3", "Test3", null);

			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == ouName);
				var user = ou.Users.First(u => u.FirstName != null);
				Assert.AreEqual(expected, user.FirstName);
			}
		}

		[TestMethod]
		public void LastUserSelectTest()
		{
			string expected = "Test3";
			CreateTestUser("TestUser1", "Test1", null);
			CreateTestUser("TestUser2", "Test2", null);
			CreateTestUser("TestUser3", "Test3", null);

			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == ouName);
				var user = ou.Users.Last(u => u.FirstName != null);
				Assert.AreEqual(expected, user.FirstName);
			}
		}

		[TestMethod]
		public void MultipleGroupSelectTest()
		{
			int expected = 2;
			CreateTestGroup("Test1Group");
			CreateTestGroup("Test2Group");

			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == ouName);
				var group = ou.Groups.Where(u => u.Name != null).ToArray();
				Assert.AreEqual(expected, group.Length);
			}
		}

		[TestMethod]
		public void FirstGroupSelectTest()
		{
			string expected = "Test1Group";
			CreateTestGroup("Test1Group");
			CreateTestGroup("Test2Group");

			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == ouName);
				var group = ou.Groups.First(u => u.Name != null);
				Assert.AreEqual(expected, group.Name);
			}
		}

		[TestMethod]
		public void LastGroupSelectTest()
		{
			string expected = "Test2Group";
			CreateTestGroup("Test1Group");
			CreateTestGroup("Test2Group");

			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == ouName);
				var group = ou.Groups.Last(u => u.Name != null);
				Assert.AreEqual(expected, group.Name);
			}
		}

	}
}
