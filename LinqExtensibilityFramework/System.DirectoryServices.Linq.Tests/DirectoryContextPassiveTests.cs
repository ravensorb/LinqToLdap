using System.DirectoryServices.Linq.Tests.Mocks;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.DirectoryServices.Linq.Tests
{
	/// <summary>
	/// Summary description for DirectoryContextTests
	/// </summary>
	[TestClass]
	public class DirectoryContextPassiveTests
	{
		#region Test data

		/// <summary>
		/// A test user that exists in the directory.
		/// </summary>
		private User TestUser
		{
			get
			{
				return new User
				{
					FirstName = "Stephen",
					LastName = "Baker",
					UserName = "sbaker",
					Email = "stephen.baker@homeserver.local"
				};
			}
		}

		string commonName = "Stephen Baker";
		string ouName = "ExternalUsers";
		string groupName = "gbl-biztalk_developers";
		string otherFirstName = "Steve";

		#endregion

		[TestMethod]
		public void DefaultConstructorTest()
		{
			// Arrange
			string firstNameStart = TestUser.FirstName.Substring(0, 2).ToLowerInvariant();

			using (var context = new DirectoryContextMock())
			{
                var users = context.Users.Where(u => u.FirstName.StartsWith(firstNameStart)).Skip(10).Take(10);
                var count = users.Count();

				Assert.IsNotNull(context.ConnectionString);
			}
		}

        [TestMethod]
        public void GetCountWithQueryTest()
        {
			// Arrange
			string firstNameStart = TestUser.FirstName.Substring(0, 2).ToLowerInvariant();

            using (var context = new DirectoryContextMock())
            {
                var userCount1 = context.Users.Where(u => u.FirstName.StartsWith(firstNameStart)).Select(u => new { u.FirstName }).Count();
                var userCount2 = context.Users.Count(u => u.FirstName.StartsWith(firstNameStart));

				Assert.AreEqual(userCount1, userCount2);
            }
        }

		[TestMethod]
		public void GetQueryableTypeTest()
		{
			// Arrange
			string userName = TestUser.UserName;
			string groupName = this.groupName;

			using (var context = new DirectoryContextMock())
			{
				var user = context.Users.First(u => u.UserName == userName);
				var userGroups = user.Groups.ToArray();
				var group = context.Groups.First(u => u.Name == groupName);
				var groupUsers = group.Users.ToArray();

				Assert.IsTrue(userGroups.Length > 0);
				Assert.IsTrue(groupUsers.Length > 0);
			}
		}

		/// <summary>
		/// This is a technical check to see that creating <c>Query</c>s in a non-standard way still works.
		/// </summary>
		[TestMethod]
		public void CheckQueryProviderActivatorMethods()
		{
			// Arrange
			IQueryable query1;
			IQueryable query2;
			System.Linq.Expressions.Expression expression = System.Linq.Expressions.Expression.Constant("test");
			Type expectedType = typeof(System.DirectoryServices.Linq.EntryObjects.EntryQuery<string>);

			// Act
			using (var context = new DirectoryContextMock())
			{
				IQueryProvider provider;
				provider = context.Users.Provider;
				query1 = provider.CreateQuery(expression);

				provider = ((IQueryable) context.OrganizationUnits.FirstOrDefault().Ous).Provider;
				query2 = provider.CreateQuery(expression);
			}

			// Assert
			Assert.AreEqual(expectedType, query1.GetType());
			Assert.AreEqual(expectedType, query2.GetType());
		}

		[TestMethod]
		public void WhereUserFirstNameIsStephenTest()
		{
			// Arrange
			string firstName = TestUser.FirstName;
			string lastNameTest = "Test";
			string lastName = TestUser.LastName;

			using (var context = new DirectoryContextMock())
			{
				var users = context.Users.Where(u => u.FirstName == firstName && (u.LastName == lastNameTest || u.LastName == lastName))
					.OrderBy(u => u.LastName)
					.Select(u => new { Name = string.Concat(u.FirstName, " ", u.LastName) })
					.ToList();

				Assert.IsTrue(users.Count >= 1);
				Assert.IsTrue(users.All(u => u.Name.StartsWith(firstName)));
			}
		}

		[TestMethod]
		public void NotEnumeratingTheResultsDoesntExecuteTest()
		{
			// Arrange
			string partialFirstName1 = TestUser.FirstName.Substring(1, TestUser.FirstName.Length - 2);
			string partialFirstName2 = this.otherFirstName.Substring(1, this.otherFirstName.Length - 2);

			using (var context = new DirectoryContextMock())
			{
				var usersNamedStephenQuery = context.Users.Where(u => u.FirstName.Contains(partialFirstName1) || u.FirstName.Contains(partialFirstName2));
			}

			// TODO: test this somehow
		}

		[TestMethod]
		public void FirstUserByEmailTest()
		{
			// Arrange
			string email = TestUser.Email;
			string firstName = TestUser.FirstName;

			using (var context = new DirectoryContextMock())
			{
				var single = context.Users.First(u => u.Email == email);

				Assert.IsNotNull(single);
				Assert.AreEqual(single.FirstName, firstName);
			}
		}

		[TestMethod]
		public void FirstUserByIdTest()
		{
			// Arrange
			string userName = TestUser.UserName;

			using (var context = new DirectoryContextMock())
			{
				var single = (from u in context.Users
							  where u.UserName == userName
							  select u).Single();

				Assert.IsNotNull(single);
				Assert.AreEqual(single.UserName.ToLower(), userName);
			}
		}

		[TestMethod]
		public void SingleUserByFirstNameAndLastNameFailsTest()
		{
			// Arrange
			string firstName = TestUser.FirstName;

			using (var context = new DirectoryContextMock())
			{
				try
				{
					var single = (from u in context.Users
								  where u.FirstName == firstName
								  select u).Single();
				}
				catch
				{
					// Passed Test.
					return;
				}

				// TODO: only makes sense if we already know the query will return more than one result
				Assert.Fail("Returned more then one result and didn't throw an exception.");
			}
		}

		[TestMethod]
		public void WhereUserFirstNameTest()
		{
			// Arrange
			string userName = TestUser.FirstName;

			using (var context = new DirectoryContextMock())
			{
				var all = context.Users.Where(u => userName == u.FirstName);

				Assert.IsNotNull(all);
				Assert.IsTrue(all.Count() > 0);
			}
		}

		[TestMethod]
		public void WhereGetUsersByAnonymousObjectTest()
		{
			// Arrange
			string commonName = this.commonName;

			using (var context = new DirectoryContextMock())
			{
				var test = new { Cn = string.Empty };
				var all = context.Users.Where(u => test.Cn == commonName).ToArray();

				Assert.IsNotNull(all);
				Assert.IsTrue(all.Length > 0);
			}
		}

		[TestMethod]
		public void WhereLastUserFirstNameTest()
		{
			// Arrange
			string firstName = TestUser.FirstName;

			using (var context = new DirectoryContextMock())
			{
				var user = context.Users.OrderBy(u => u.LastName).Last(u => u.FirstName == firstName);

				Assert.IsNotNull(user);
			}
		}

		[TestMethod]
		public void LastOrDefaultUserFirstNameTest()
		{
			// Arrange
			string firstName = "asdf";

			using (var context = new DirectoryContextMock())
			{
				var user = context.Users.LastOrDefault(u => u.FirstName == firstName);

				Assert.IsNull(user);
			}
		}

		[TestMethod]
		public void LastUserFirstNameIsNotEmptyTest()
		{
			// Arrange
			string firstName = TestUser.FirstName;
			string email = null;

			using (var context = new DirectoryContextMock())
			{
				var user = context.Users.LastOrDefault(u => u.FirstName == firstName && u.Email == email);

				Assert.IsNotNull(user);
			}
		}

		[TestMethod]
		public void WhereUserFirstNameSkip1Take10Test()
		{
			string firstNameStart = TestUser.FirstName.Substring(0, 2);

			using (var context = new DirectoryContextMock())
			{
				var all = context.Users.Where(u => u.FirstName.StartsWith(firstNameStart))
					.Skip(90)
					.Take(10)
					.OrderBy(u => u.LastName)
					.ToArray();

				Assert.IsNotNull(all);
				Assert.IsTrue(all.Length == 10);
			}
		}

		[TestMethod]
		public void WhereFirstNameContainsTest()
		{
			string firstNameFragment1 = TestUser.FirstName.Substring(1, TestUser.FirstName.Length - 2);
			string firstNameFragment2 = this.otherFirstName.Substring(1, this.otherFirstName.Length - 2);

			using (var context = new DirectoryContextMock())
			{
				// Takes a while...figure out why..
				var usersFirstNameMethodQuery = context.Users.Where(u => u.FirstName.Contains(firstNameFragment1) || u.FirstName.Contains(firstNameFragment2));
				Assert.IsTrue(usersFirstNameMethodQuery.ToArray().Length > 0);
			}
		}

		[TestMethod]
		public void WhereFirstNameStartsWithAndEndsWithTest()
		{
			string firstNameStart = TestUser.FirstName.Substring(0, 2);
			string firstNameEnd = TestUser.FirstName.Substring(TestUser.FirstName.Length - 2);

			using (var context = new DirectoryContextMock())
			{
				// Takes a while...figure out why..
				var usersFirstNameMethodQuery = context.Users.Where(u => u.FirstName.StartsWith(firstNameStart) || u.FirstName.EndsWith(firstNameEnd)).ToList();
				Assert.IsTrue(usersFirstNameMethodQuery.Count > 0);
			}
		}

		[TestMethod]
		public void OneLevelTest()
		{
			// Test that the OneLevel option works with subqueries.
			// Relies on there being at least one user in a sub-OU.

			// Arrange
			string ouName = this.ouName;
			int unexpected;
			int result;

			// Act
			using (var context = new DirectoryContextMock())
			{
				var ou = context.OrganizationUnits.Single(u => u.Name == ouName);
				result = ou.DirectUsers.Where( u => true ).Count();
				unexpected = ou.Users.Count();
			}

			// Assert
			Assert.AreNotEqual(unexpected, result);
		}

		/// <summary>
		/// Check that string comparison with empty strings produces the expected results.
		/// </summary>
		[TestMethod]
		public void EmptyStringTests() {
			// Arrange
			int expected;
			int startingWith;
			int endingWith;
			int containing;
			int equal;

			// Act
			using( var context = new DirectoryContextMock() ) {
				expected = context.Users.Count();
				startingWith = context.Users.Where(u=>u.UserName.StartsWith(string.Empty)).Count();
				endingWith = context.Users.Where(u=>u.UserName.EndsWith(string.Empty)).Count();
				containing = context.Users.Where(u=>u.UserName.Contains(string.Empty)).Count();
				equal = context.Users.Where(u=>u.UserName == string.Empty).Count();
			}

			// Assert
			Assert.AreEqual(expected, startingWith);
			Assert.AreEqual(expected, endingWith);
			Assert.AreEqual(expected, containing);
			Assert.AreEqual(0, equal);
		}
	}
}
