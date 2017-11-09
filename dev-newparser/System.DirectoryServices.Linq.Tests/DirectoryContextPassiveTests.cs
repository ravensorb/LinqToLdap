using System.Collections.Generic;
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
		public void GroupNamesTest() {
			// Arrange
			string userName = this.TestUser.UserName;
			string[] groupNames;

			// Act
			using( var context = new DirectoryContextMock() ) {
				var user = context.Users.First( u => u.UserName == userName );
				groupNames = user.DirectGroupNames.ToArray();
			}

			// Assert
			Assert.IsNotNull(groupNames);
			Assert.AreNotEqual(0, groupNames.Length);
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
		public void TrivialClauseTrue() {
			// Arrange
			int count;
			int unexpected = 0;

			// Act
			using( var context = new DirectoryContextMock() ) {
				count = context.Users.Where(u => true).Count();
			}

			// Assert
			Assert.AreNotEqual(unexpected, count);
		}

		[TestMethod]
		public void TrivialClauseFalse() {
			// Arrange
			int count;
			int expected = 0;

			// Act
			using( var context = new DirectoryContextMock() ) {
				count = context.Users.Where(u => false).Count();
			}

			// Assert
			Assert.AreEqual(expected, count);
		}

		[TestMethod]
		public void WhereLastUserFirstNameTest()
		{
			// Arrange
			string firstName = TestUser.FirstName;
			string notOrderedName;
			string orderedName;

			// Act
			using (var context = new DirectoryContextMock())
			{
				notOrderedName = context.Users.Last(u => u.FirstName == firstName).LastName;
				orderedName = context.Users.OrderBy(u => u.LastName).Last(u => u.FirstName == firstName).LastName;
			}

			// Assert
			Assert.AreNotEqual(notOrderedName, orderedName);
		}

		[TestMethod]
		public void OrderByDescendingTest() {
			// Arrange
			string firstName = TestUser.FirstName;
			string orderedName;
			string reverseOrderedName;

			// Act
			using( var context = new DirectoryContextMock() ) {
				orderedName = context.Users.OrderBy(u=>u.LastName).First( u => u.FirstName == firstName ).LastName;
				reverseOrderedName = context.Users.OrderByDescending( u => u.LastName ).First( u => u.FirstName == firstName ).LastName;
			}

			// Assert
			Assert.AreNotEqual( orderedName, reverseOrderedName );
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
		[ExpectedException( typeof( System.InvalidOperationException ) )]
		public void LastUserFirstNameTest() {
			// Arrange
			string firstName = "asdf";

			using( var context = new DirectoryContextMock() ) {
				var user = context.Users.Last( u => u.FirstName == firstName );

				Assert.IsNull( user );
			}
		}

		[TestMethod]
		[ExpectedException( typeof( System.InvalidOperationException ) )]
		public void SingleFirstNameFailsTest() {
			// Arrange
			string firstName = TestUser.FirstName;

			// Act
			using( var context = new DirectoryContextMock() ) {
				var user = context.Users.Single( u => u.FirstName == firstName );
			}

			// Assert
			Assert.Fail( "Should have thrown an exception." );
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
		public void SkipTest() {
			// Arrange
			string firstNameStart = TestUser.FirstName.Substring( 0, 1 );
			int skip = 1;
			IEnumerable<User> expected;
			IEnumerable<User> actual;

			// Act
			using( var context = new DirectoryContextMock() ) {
				var queryBase = context.Users.Where( u => u.FirstName.StartsWith( firstNameStart ) );
				int total = queryBase.Count();
				if( total <= skip )
					Assert.Inconclusive( "Not enough elements for this test." );

				expected = queryBase.ToArray().Skip( skip );
				actual = queryBase.Skip( skip ).ToArray();
			}

			// Assert
			SequenceAssert.AreEqual( expected.Select( u => u.LastName ), actual.Select( u => u.LastName ) );
		}

		[TestMethod]
		public void TakeTest() {
			// Arrange
			string firstNameStart = TestUser.FirstName.Substring( 0, 1 );
			int take = 2;
			IEnumerable<User> expected;
			IEnumerable<User> actual;

			// Act
			using( var context = new DirectoryContextMock() ) {
				var queryBase = context.Users.Where( u => u.FirstName.StartsWith( firstNameStart ) );
				int total = queryBase.Count();
				if( total <= take )
					Assert.Inconclusive( "Not enough elements for this test." );

				expected = queryBase.ToArray().Take( take );
				actual = queryBase.Take( take ).ToArray();
			}

			// Assert
			SequenceAssert.AreEqual( expected.Select( u => u.LastName ), actual.Select( u => u.LastName ) );
		}

		[TestMethod]
		public void SkipTakeTest() {
			// Arrange
			string firstNameStart = TestUser.FirstName.Substring( 0, 1 );
			int skip = 1;
			int take = 2;
			IEnumerable<User> expected;
			IEnumerable<User> actual;

			// Act
			using( var context = new DirectoryContextMock() ) {
				var queryBase = context.Users.Where( u => u.FirstName.StartsWith( firstNameStart ) );
				int total = queryBase.Count();
				if( total <= skip + take )
					Assert.Inconclusive( "Not enough elements for this test." );

				expected = queryBase.ToArray().Skip( skip ).Take( take );
				actual = queryBase.Skip( skip ).Take( take ).ToArray();
			}

			// Assert
			SequenceAssert.AreEqual( expected.Select( u => u.LastName ), actual.Select( u => u.LastName ) );
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

		private static class SequenceAssert {
			public static void AreEqual<T>( IEnumerable<T> expected, IEnumerable<T> actual ) {
				AreEqualInt<T>( expected, actual );
			}

			private static void AreEqualInt<T>( IEnumerable<T> expected, IEnumerable<T> actual ) {
				var expectedEnumerator = expected.GetEnumerator();
				var actualEnumerator = actual.GetEnumerator();
				int count = 0;
				while( true ) {
					bool expectedNext = expectedEnumerator.MoveNext();
					bool actualNext = actualEnumerator.MoveNext();
					if( expectedNext != actualNext ) {
						// Get the full count.
						int expectedCount = count;
						int actualCount = count;
						if( expectedNext ) {
							expectedCount++;
							while( expectedEnumerator.MoveNext() ) { expectedCount++; }
						}
						if( actualNext ) {
							actualCount++;
							while( actualEnumerator.MoveNext() ) { actualCount++; }
						}
						throw new AssertFailedException( string.Format( "Sizes do not match: expected {0}; received {1}", expectedCount, actualCount ) );
					}
					if( !actualNext ) {
						// Since expectedNext == actualNext == false, so the enumerations are done.
						break;
					}
					count++;
					Assert.AreEqual( expectedEnumerator.Current, actualEnumerator.Current );
				}
			}
		}

	}
}
