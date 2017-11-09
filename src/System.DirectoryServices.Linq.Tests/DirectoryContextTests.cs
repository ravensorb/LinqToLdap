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

		[TestMethod]
		public void AddAndDeleteNewUserSubmitChangesTest()
		{
			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var single = new User
				{
					UserName = "sbaker",
					FirstName = "Steve",
					LastName = "Baker",
					Email = "sbaker@homeserver.local"
				};

				var ou = context.OrganizationUnits.First(o => o.Name == "TestOU");
				single.SetParent(ou);
				context.AddObject(single);
				single.SetPassword("Wh@7Wh@7");
				context.SubmitChanges();

				var single1 = context.Users.Single(u => u.UserName == "sbaker");
				context.DeleteObject(single1);
				context.SubmitChanges();
				var single2 = context.Users.SingleOrDefault(u => u.UserName == "sbaker");
				Assert.IsNull(single2);
			}
		}

		[TestMethod]
		public void AddAndDeleteNewOuSubmitChangesTest()
		{
			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = new OU {Name = "ChildOU"};
				ou.SetParent(context.OrganizationUnits.First(u => u.Name == "TestOU"));
				context.AddObject(ou);
				context.SubmitChanges();

				var childOu = context.OrganizationUnits.First(u => u.Name == "ChildOU");
				context.DeleteObject(childOu);
				context.SubmitChanges();
			}
		}

		[TestMethod]
		public void AddAndDeleteNewGroupSubmitChangesTest()
		{
			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var newGroup = new Group {Name = "TestGroup"};
				newGroup.SetParent(context.OrganizationUnits.First(u => u.Name == "TestOU"));
				context.AddObject(newGroup);
				context.SubmitChanges();

				var group = context.Groups.First(u => u.Name == "TestGroup");
				context.DeleteObject(group);
				context.SubmitChanges();
			}
		}

		[TestMethod]
		public void MultipleOuSelectTest()
		{
			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == "TestOU");
				var result = ou.Ous.Where(u => u.Name != null).ToArray();
				Assert.AreEqual(result.Length, 2);
			}
		}

		[TestMethod]
		public void FirstOuSelectTest()
		{
			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == "TestOU");
				var result = ou.Ous.First(u => u.Name != null);
				Assert.AreEqual(result.Name, "Test1");
			}
		}

		[TestMethod]
		public void LastOuSelectTest()
		{
			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == "TestOU");
				var result = ou.Ous.Last(u => u.Name != null);
				Assert.AreEqual(result.Name, "Test2");
			}
		}

		[TestMethod]
		public void MultipleUserSelectTest()
		{
			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == "TestOU");
				var user = ou.Users.Where(u => u.FirstName != null).ToArray();
				Assert.AreEqual(user.Length, 3);
			}
		}

		[TestMethod]
		public void FirstUserSelectTest()
		{
			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == "TestOU");
				var user = ou.Users.First(u => u.FirstName != null);
				Assert.AreEqual(user.FirstName, "Test1");
			}
		}

		[TestMethod]
		public void LastUserSelectTest()
		{
			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == "TestOU");
				var user = ou.Users.Last(u => u.FirstName != null);
				Assert.AreEqual(user.FirstName, "TestUser");
			}
		}

		[TestMethod]
		public void MultipleGroupSelectTest()
		{
			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == "TestOU");
				var group = ou.Groups.Where(u => u.Name != null).ToArray();
				Assert.AreEqual(group.Length, 2);
			}
		}

		[TestMethod]
		public void FirstGroupSelectTest()
		{
			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == "TestOU");
				var group = ou.Groups.First(u => u.Name != null);
				Assert.AreEqual(group.Name, "Test1Group");
			}
		}

		[TestMethod]
		public void LastGroupSelectTest()
		{
			using (var context = new DirectoryContextMock(ConnectionString, Username, Password))
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == "TestOU");
				var group = ou.Groups.Last(u => u.Name != null);
				Assert.AreEqual(group.Name, "Test2Group");
			}
		}

	}
}
