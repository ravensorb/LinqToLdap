using System;
using System.DirectoryServices.Linq.Tests.Mocks;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.DirectoryServices.Linq.Tests
{
	/// <summary>
	/// Summary description for SpeedTests
	/// </summary>
	[TestClass]
	public class SpeedTests
	{
		public SpeedTests()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		[TestMethod]
		public void GetUserById()
		{
			using (var context = new DirectoryContextMock())
			{
				var userId = new Guid("5825d585-8f56-4c8d-a1e6-ece79c974cc2");

				var user = (from u in context.CreateEntrySet<User>()
							where u.Id == userId
							select u).FirstOrDefault();
			}
		}

		[TestMethod]
		public void GetUserByEmployeeNumber()
		{
			using (var context = new DirectoryContextMock())
			{
				var userId = new Guid("5825d585-8f56-4c8d-a1e6-ece79c974cc2");
				var user = context.Users.FirstOrDefault(u => u.Id == userId);
			}
		}
	}
}
