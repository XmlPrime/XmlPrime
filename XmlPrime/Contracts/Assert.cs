using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace XmlPrime.Contracts
{
	/// <summary>
	/// Assertion functions.
	/// </summary>
	internal static class Assert
	{
		#region Internal Static Methods

		/// <summary>
		/// Asserts that an argument satisfies a predicate.
		/// </summary>
		/// <typeparam name="T">The type of the argument being tested</typeparam>
		/// <param name="argument">The argument.</param>
		/// <param name="predicate">The predicate.</param>
		/// <param name="argumentName">The name of the argument.</param>
		[Conditional("DEBUG")]
		internal static void ArgumentSatisfies<T>([NotNull] T argument,
		                                          [NotNull] AssertionPredicate<T> predicate,
		                                          [NotNull] [InvokerParameterName] string argumentName)
		{
			ArgumentNotNull(argumentName, "argumentName");
			ArgumentNotNull(predicate, "predicate");

			Debug.Assert(predicate.Predicate(argument),
			             string.Format(CultureInfo.CurrentCulture, predicate.Description, argumentName));
		}

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Asserts that a value is equal to an expected value.
		/// </summary>
		/// <param name="actual">The actual value.</param>
		/// <param name="expected">The expected value.</param>
		/// <param name="argumentName">The name of the argument.</param>
		[Conditional("DEBUG")]
		public static void AreEqual<T>(T actual, T expected, [NotNull] string argumentName)
		{
			ArgumentNotNull(argumentName, "argumentName");

			Debug.Assert(actual.Equals(expected), argumentName + " should be " + expected + " but was " + actual + ".");
		}

		/// <summary>
		/// Asserts that two references are identical.
		/// </summary>
		/// <param name="actual">The actual value.</param>
		/// <param name="expected">The expected value.</param>
		/// <param name="actualName">The name of the actual value.</param>
		/// <param name="expectedName">The name of the expected value.</param>
		[Conditional("DEBUG")]
		public static void AreSame<T>([CanBeNull] T actual, [CanBeNull] T expected, [NotNull] string actualName, [NotNull] string expectedName) where T : class
		{
			ArgumentNotNull(actualName, "actualName");
			ArgumentNotNull(expectedName, "expectedName");
			
			Debug.Assert(ReferenceEquals(actual, expected), actualName + " and " + expectedName + " should be the same instance.");
		}

		/// <summary>
		/// Asserts that a value is not equal to an expected value.
		/// </summary>
		/// <param name="actual">The actual value.</param>
		/// <param name="expected">The expected value.</param>
		/// <param name="argumentName">The name of the argument.</param>
		[Conditional("DEBUG")]
		public static void AreNotEqual<T>(T actual, T expected, [NotNull] string argumentName)
		{
			ArgumentNotNull(argumentName, "argumentName");

			Debug.Assert(!actual.Equals(expected), argumentName + " should not be " + expected + ".");
		}

		/// <summary>
		/// Asserts that an argument to a function is in a particular range.
		/// </summary>
		/// <param name="argument">The argument.</param>
		/// <param name="minimumValue">The minimum value of the argument.</param>
		/// <param name="maximumValue">The maximum value of the argument.</param>
		/// <param name="argumentName">The name of the argument.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="argument"/> is out of the specified range.
		/// </exception>isp
		[Conditional("DEBUG")]
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public static void ArgumentInRange(long argument,
		                                   long minimumValue,
		                                   long maximumValue,
		                                   [NotNull] [InvokerParameterName] string argumentName)
		{
			ArgumentNotNull(argumentName, "argumentName");

			var message = string.Format(CultureInfo.CurrentCulture,
			                            "{0} was {1}, should be in the range [{2}, {3}].",
			                            argumentName,
			                            argument,
			                            minimumValue,
			                            maximumValue);

			Debug.Assert(argument >= minimumValue && argument <= maximumValue, message);
		}

		/// <summary>
		/// Asserts that an argument to a function is in a particular range.
		/// </summary>
		/// <param name="argument">The argument.</param>
		/// <param name="minimumValue">The minimum value of the argument.</param>
		/// <param name="maximumValue">The maximum value of the argument.</param>
		/// <param name="argumentName">The name of the argument.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="argument"/> is out of the specified range.
		/// </exception>
		[Conditional("DEBUG")]
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public static void ArgumentInRange(decimal argument,
		                                   decimal minimumValue,
		                                   decimal maximumValue,
		                                   [NotNull] [InvokerParameterName] string argumentName)
		{
			ArgumentNotNull(argumentName, "argumentName");

			var message = string.Format(CultureInfo.CurrentCulture,
			                            "{0} was {1}, should be in the range [{2}, {3}].",
			                            argumentName,
			                            argument,
			                            minimumValue,
			                            maximumValue);

			Debug.Assert(argument >= minimumValue && argument <= maximumValue, message);
		}

		/// <summary>
		/// Asserts that an argument to a function is not <see langword="null" />
		/// </summary>
		/// <param name="argument">The argument.</param>
		/// <param name="argumentName">The name of the argument.</param>
		/// <param name="expectedLength">The expected length of the array.</param>
		/// <remarks>
		/// This should be called for every argument marked with <see cref="NotNullAttribute" /> in private and internal
		/// methods.
		/// </remarks>
		[Conditional("DEBUG")]
		public static void ArgumentLengthEquals([CanBeNull] Array argument,
		                                        int expectedLength,
		                                        [NotNull] [InvokerParameterName] string argumentName)
		{
			ArgumentNotNull(argumentName, "argumentName");
			ArgumentNotNull(argument, argumentName);

			Debug.Assert(argument.Length == expectedLength,
			             argumentName + " must have length " + expectedLength + " (was " + argument.Length + ")");
		}

		/// <summary>
		/// Asserts that an argument to a function is non-zero, and throws an <see cref="ArgumentOutOfRangeException"/> 
		/// otherwise.
		/// </summary>
		/// <param name="argument">The argument.</param>
		/// <param name="argumentName">The name of the argument.</param>
		[Conditional("DEBUG")]
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public static void ArgumentNonzero(long argument, [NotNull] [InvokerParameterName] string argumentName)
		{
			ArgumentNotNull(argumentName, "argumentName");

			var message = argumentName + " cannot be 0.";
			Debug.Assert(argument != 0, message);
		}

		/// <summary>
		/// Asserts that a string value is not null or empty.
		/// </summary>
		/// <param name="argument">The value.</param>
		/// <param name="argumentName">The name of the value.</param>
		[Conditional("DEBUG")]
		[AssertionMethod]
		public static void ArgumentNotEmpty(
			[CanBeNull] [AssertionCondition(AssertionConditionType.IsNotNull)] string argument,
			[NotNull] [InvokerParameterName] string argumentName)
		{
			ArgumentNotNull(argumentName, "argumentName");

			Debug.Assert(!string.IsNullOrEmpty(argument), argumentName + " must not be empty.");
		}

		/// <summary>
		/// Asserts that a string value is not null or empty.
		/// </summary>
		/// <param name="argument">The value.</param>
		/// <param name="argumentName">The name of the value.</param>
		[Conditional("DEBUG")]
		[AssertionMethod]
		public static void ArgumentNotEmpty<T>(
			[NotNull] [AssertionCondition(AssertionConditionType.IsNotNull)] ICollection<T> argument,
			[NotNull] [InvokerParameterName] string argumentName)
		{
			Ensure.ArgumentNotNull(argument, "argument");
			ArgumentNotNull(argumentName, "argumentName");

			Debug.Assert(argument.Count != 0, argumentName + " must not be empty.");
		}

		/// <summary>
		/// Asserts that an argument to a function is not null.
		/// </summary>
		/// <param name="argument">The argument.</param>
		/// <param name="argumentName">The name of the argument.</param>
		/// <remarks>
		/// This should be called for every argument marked with <see cref="NotNullAttribute" /> in private and internal
		/// methods.
		/// </remarks>
		[Conditional("DEBUG")]
		[AssertionMethod]
		public static void ArgumentNotNull([CanBeNull] [AssertionCondition(AssertionConditionType.IsNotNull)] object argument,
		                                   [NotNull] [InvokerParameterName] string argumentName)
		{
			Debug.Assert(argument != null, argumentName + " must not be null.");
		}

		/// <summary>
		/// Fails with the specified message.
		/// </summary>
		/// <param name="message">The message.</param>
		[Conditional("DEBUG")]
		[TerminatesProgram]
		public static void Fail([NotNull] string message)
		{
			ArgumentNotNull(message, "message");

			Debug.Fail(message);
		}

		/// <summary>
		/// Fails with the specified message.
		/// </summary>
		[Conditional("DEBUG")]
		[TerminatesProgram]
		public static void Fail()
		{
			Fail("Something went wrong");
		}

		/// <summary>
		/// Ensures that an indexer has the value specified by <paramref name="expectedIndex" />,
		/// and throws an <see cref="IndexOutOfRangeException"/> otherwise.
		/// </summary>
		/// <param name="index">The actual index value.</param>
		/// <param name="expectedIndex">The expected value of the index.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is not equal to the expected value.
		/// </exception>
		[Conditional("DEBUG")]
		public static void IndexEquals(long index, long expectedIndex)
		{
			if (index != expectedIndex)
				Fail(string.Format(CultureInfo.CurrentCulture, "index was {0}, should be {1}.", index, expectedIndex));
		}

		/// <summary>
		/// Ensures that an indexer is in the range 0 to <paramref name="maximumValue" />. 
		/// and throws an <see cref="IndexOutOfRangeException"/> otherwise.
		/// </summary>
		/// <param name="index">The actual index value.</param>
		/// <param name="maximumValue">The maximum permitted value of the index.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of the specified range.
		/// </exception>
		[Conditional("DEBUG")]
		public static void IndexInRange(int index, int maximumValue)
		{
			if (index >= 0 &&
			    index <= maximumValue)
				return;

			var msg = (maximumValue == -1)
			          	? "index {0} was out of range."
			          	: "index was {0}, should be in the range [0, {1}].";

			Fail(string.Format(CultureInfo.CurrentCulture, msg, index, maximumValue));
		}

		/// <summary>
		/// Ensures that an indexer is in the range 0 to <paramref name="maximumValue" />. 
		/// and throws an <see cref="IndexOutOfRangeException"/> otherwise.
		/// </summary>
		/// <param name="index">The actual index value.</param>
		/// <param name="maximumValue">The maximum permitted value of the index.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of the specified range.
		/// </exception>
		[Conditional("DEBUG")]
		public static void IndexInRange(long index, long maximumValue)
		{
			if (index >= 0 &&
			    index <= maximumValue)
				return;

			var msg = (maximumValue == -1)
			          	? "index {0} was out of range."
			          	: "index was {0}, should be in the range [0, {1}].";

			Fail(string.Format(CultureInfo.CurrentCulture, msg, index, maximumValue));
		}

		/// <summary>
		/// Asserts that a particular predicate is <see langword="false"/>.
		/// </summary>
		/// <param name="argument">The predicate to check.</param>
		/// <param name="argumentName">The name of the predicate.</param>
		[Conditional("DEBUG")]
		[AssertionMethod]
		public static void IsFalse([CanBeNull] [AssertionCondition((AssertionConditionType.IsFalse))] bool argument,
		                           [NotNull] string argumentName)
		{
			ArgumentNotNull(argumentName, "argumentName");
			
			Debug.Assert(argument == false, argumentName + " should be false.");
		}

		/// <summary>
		/// Asserts that a particular predicate is <see langword="true"/>.
		/// </summary>
		/// <param name="argument">The predicate to check.</param>
		/// <param name="argumentName">The name of the predicate.</param>
		[Conditional("DEBUG")]
		[AssertionMethod]
		public static void IsTrue([CanBeNull] [AssertionCondition((AssertionConditionType.IsTrue))] bool argument,
								   [NotNull] string argumentName)
		{
			ArgumentNotNull(argumentName, "argumentName");

			Debug.Assert(argument, argumentName + " should be true.");
		}


		/// <summary>
		/// Asserts that a string value is not null or empty.
		/// </summary>
		/// <param name="argument">The value.</param>
		/// <param name="argumentName">The name of the value.</param>
		[Conditional("DEBUG")]
		[AssertionMethod]
		public static void IsNotEmpty([CanBeNull] [AssertionCondition(AssertionConditionType.IsNotNull)] string argument,
		                              [NotNull] string argumentName)
		{
			ArgumentNotNull(argumentName, "argumentName");

			Debug.Assert(!string.IsNullOrEmpty(argument), argumentName + " must not be empty.");
		}

		/// <summary>
		/// Asserts that a value is not null.
		/// </summary>
		/// <param name="argument">The value.</param>
		/// <param name="argumentName">The name of the value.</param>
		[Conditional("DEBUG")]
		[AssertionMethod]
		public static void IsNotNull([CanBeNull] [AssertionCondition(AssertionConditionType.IsNotNull)] object argument,
		                             [NotNull] string argumentName)
		{
			ArgumentNotNull(argumentName, "argumentName");

			Debug.Assert(argument != null, argumentName + " must not be null.");
		}

		/// <summary>
		/// Asserts that a value is null.
		/// </summary>
		/// <param name="argument">The value.</param>
		/// <param name="argumentName">The name of the value.</param>
		[Conditional("DEBUG")]
		[AssertionMethod]
		public static void IsNull([CanBeNull] [AssertionCondition(AssertionConditionType.IsNull)] object argument,
									 [NotNull] string argumentName)
		{
			ArgumentNotNull(argumentName, "argumentName");

			Debug.Assert(argument == null, argumentName + " must not be null.");
		}

		/// <summary>
		/// Asserts that an argument satisfies a predicate.
		/// </summary>
		/// <param name="condition">The condition to assert.</param>
		/// <param name="description">The description of the condition.</param>
		[Conditional("DEBUG")]
		public static void Satisfies([NotNull] bool condition, [NotNull] string description)
		{
			ArgumentNotNull(description, "description");

			Debug.Assert(condition, description);
		}

		#endregion
	}
}