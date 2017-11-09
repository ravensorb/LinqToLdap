using System;
using System.Collections.Generic;
using System.DirectoryServices.Linq.Expressions;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.DirectoryServices.Linq.Tests {
	[TestClass]
	public class SubstringExpressionTests {
		private SubstringExpression CreateSubstring( IEnumerable<string> parts, bool wildcardAtStart, bool wildcardAtEnd ) {
			return LdapExpressionFactory.Substring( wildcardAtStart, parts.Select( s => Expression.Constant( s ) ), wildcardAtEnd );
		}

		private SubstringExpression CreateSubstring( string part, bool wildcardAtStart, bool wildcardAtEnd ) {
			return CreateSubstring( new string[] { part }, wildcardAtStart, wildcardAtEnd );
		}

		[TestMethod]
		public void single_string_with_no_wildcard_becomes_constant() {
			// Arrange
			string value = "X";
			var substring = CreateSubstring( value, false, false );
			Expression result;

			// Act
			result = SubstringEmptyClauseRemover.Rewrite( substring );

			// Assert
			Assert.IsTrue( result.IsConstant( value ) );
		}

		[TestMethod]
		public void empty_string_with_no_wildcard_doesnt_become_constant() {
			// Arrange
			string value = string.Empty;
			var substring = CreateSubstring( value, false, false );
			Expression result;

			// Act
			result = SubstringEmptyClauseRemover.Rewrite( substring );

			// Assert
			Assert.IsFalse( result.IsConstant( value ) );
		}

		[TestMethod]
		public void simple_substring_is_unchanged_1() {
			// Arrange
			string value = "X";
			var substring = CreateSubstring( value, true, true );
			Expression result;

			// Act
			result = SubstringEmptyClauseRemover.Rewrite( substring );

			// Assert
			Assert.IsInstanceOfType( result, typeof( SubstringExpression ) );
			Assert.AreEqual( substring, (SubstringExpression)result );
		}

		[TestMethod]
		public void simple_substring_is_unchanged_2() {
			// Arrange
			string value1 = "X";
			string value2 = "Y";
			string value3 = "Z";
			var substring = CreateSubstring( new string[] { value1, value2, value3 }, true, true );
			Expression result;

			// Act
			result = SubstringEmptyClauseRemover.Rewrite( substring );

			// Assert
			Assert.IsInstanceOfType( result, typeof( SubstringExpression ) );
			//Assert.AreEqual(substring.WildcardAtStart, result.
			Assert.AreEqual( substring, (SubstringExpression)result );
		}

		[TestMethod]
		public void empty_string_at_start_is_removed() {
			// Arrange
			string value = "X";
			var substring = CreateSubstring( new string[] { string.Empty, value }, true, true );
			Expression result;
			SubstringExpression substringResult;

			// Act
			result = SubstringEmptyClauseRemover.Rewrite( substring );

			// Assert
			Assert.IsInstanceOfType( result, typeof( SubstringExpression ) );
			substringResult = (SubstringExpression)result;
			Assert.AreEqual( 1, substringResult.Parts.Count );
			Assert.IsTrue( substringResult.Parts[0].IsConstant( value ) );
			Assert.AreEqual( true, substringResult.WildcardAtStart );
		}

		[TestMethod]
		public void empty_string_at_end_is_removed() {
			// Arrange
			string value = "X";
			var substring = CreateSubstring( new string[] { value, string.Empty }, true, true );
			Expression result;
			SubstringExpression substringResult;

			// Act
			result = SubstringEmptyClauseRemover.Rewrite( substring );

			// Assert
			Assert.IsInstanceOfType( result, typeof( SubstringExpression ) );
			substringResult = (SubstringExpression)result;
			Assert.AreEqual( 1, substringResult.Parts.Count );
			Assert.IsTrue( substringResult.Parts[0].IsConstant( value ) );
			Assert.AreEqual( true, substringResult.WildcardAtEnd );
		}

		[TestMethod]
		public void empty_string_in_middle_is_removed() {
			// Arrange
			string value1 = "X";
			string value2 = "Y";
			var substring = CreateSubstring( new string[] { value1, string.Empty, value2 }, true, true );
			Expression result;
			SubstringExpression substringResult;

			// Act
			result = SubstringEmptyClauseRemover.Rewrite( substring );

			// Assert
			Assert.IsInstanceOfType( result, typeof( SubstringExpression ) );
			substringResult = (SubstringExpression)result;
			Assert.AreEqual( 2, substringResult.Parts.Count );
			Assert.IsTrue( substringResult.Parts[0].IsConstant( value1 ) );
			Assert.IsTrue( substringResult.Parts[1].IsConstant( value2 ) );
		}

		[TestMethod]
		public void null_strings_treated_the_same() {
			// Arrange
			string value1 = "X";
			string value2 = "Y";
			var substring = CreateSubstring( new string[] { value1, null, value2 }, true, true );
			Expression result;
			SubstringExpression substringResult;

			// Act
			result = SubstringEmptyClauseRemover.Rewrite( substring );

			// Assert
			Assert.IsInstanceOfType( result, typeof( SubstringExpression ) );
			substringResult = (SubstringExpression)result;
			Assert.AreEqual( 2, substringResult.Parts.Count );
			Assert.IsTrue( substringResult.Parts[0].IsConstant( value1 ) );
			Assert.IsTrue( substringResult.Parts[1].IsConstant( value2 ) );
		}

		[TestMethod]
		public void empty_strings_in_middle_are_removed() {
			// Arrange
			string[] strings = { "X", string.Empty, "Y", string.Empty, string.Empty, "Z" };
			var substring = CreateSubstring( strings, true, true );
			Expression result;
			SubstringExpression substringResult;

			// Act
			result = SubstringEmptyClauseRemover.Rewrite( substring );

			// Assert
			Assert.IsInstanceOfType( result, typeof( SubstringExpression ) );
			substringResult = (SubstringExpression)result;
			Assert.AreEqual( 3, substringResult.Parts.Count );
			Assert.IsTrue( substringResult.Parts[0].IsConstant( "X" ) );
			Assert.IsTrue( substringResult.Parts[1].IsConstant( "Y" ) );
			Assert.IsTrue( substringResult.Parts[2].IsConstant( "Z" ) );
		}

		[TestMethod]
		public void empty_string_at_start_is_removed_and_start_wildcard_is_set() {
			// Arrange
			string value = "X";
			var substring = CreateSubstring( new string[] { string.Empty, value }, false, true );
			Expression result;
			SubstringExpression substringResult;

			// Act
			result = SubstringEmptyClauseRemover.Rewrite( substring );

			// Assert
			Assert.IsInstanceOfType( result, typeof( SubstringExpression ) );
			substringResult = (SubstringExpression)result;
			Assert.AreEqual( 1, substringResult.Parts.Count );
			Assert.IsTrue( substringResult.Parts[0].IsConstant( value ) );
			Assert.AreEqual( true, substringResult.WildcardAtStart );
		}

		[TestMethod]
		public void empty_string_at_end_is_removed_and_end_wildcard_is_set() {
			// Arrange
			string value = "X";
			var substring = CreateSubstring( new string[] { value, string.Empty }, true, false );
			Expression result;
			SubstringExpression substringResult;

			// Act
			result = SubstringEmptyClauseRemover.Rewrite( substring );

			// Assert
			Assert.IsInstanceOfType( result, typeof( SubstringExpression ) );
			substringResult = (SubstringExpression)result;
			Assert.AreEqual( 1, substringResult.Parts.Count );
			Assert.IsTrue( substringResult.Parts[0].IsConstant( value ) );
			Assert.AreEqual( true, substringResult.WildcardAtEnd );
		}

	}
}
