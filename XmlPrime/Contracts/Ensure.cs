using System;
using System.Globalization;

namespace XmlPrime.Contracts
{
	/// <summary>
	///   Assertion functions that are not optimized away in release builds.
	/// </summary>
	internal static class Ensure
	{
		#region Public Static Methods

		/// <summary>
		///   Ensures that an argument to a function is in a particular range, and throws an
		///   <see cref = "ArgumentOutOfRangeException" /> otherwise.
		/// </summary>
		/// <param name = "argument">The argument.</param>
		/// <param name = "minimumValue">The minimum value of the argument.</param>
		/// <param name = "maximumValue">The maximum value of the argument.</param>
		/// <param name = "argumentName">The name of the argument.</param>
		/// <exception cref = "ArgumentOutOfRangeException">
		///   <paramref name = "argument" /> is out of the specified range.
		/// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void ArgumentInRange(int argument,
		                                   int minimumValue,
		                                   int maximumValue,
		                                   [NotNull] [InvokerParameterName] string argumentName)
		{
			Assert.ArgumentNotNull(argumentName, "argumentName");

			if (argument < minimumValue ||
			    argument > maximumValue)
			{
				throw new ArgumentOutOfRangeException(argumentName,
				                                      string.Format(CultureInfo.CurrentCulture,
				                                                    "{0} was {1}, should be in the range [{2}, {3}].",
				                                                    argumentName,
				                                                    argument,
				                                                    minimumValue,
				                                                    maximumValue));
			}
		}

		/// <summary>
		///   Ensures that an argument to a function is in the range 
		///   <c>[<paramref name = "minimumValue" />,<paramref name = "maximumValue" />)</c> and throws an
		///   <see cref = "ArgumentOutOfRangeException" /> otherwise.
		/// </summary>
		/// <param name = "argument">The argument.</param>
		/// <param name = "minimumValue">The minimum value of the argument.</param>
		/// <param name = "maximumValue">The maximum value of the argument.</param>
		/// <param name = "argumentName">The name of the argument.</param>
		/// <exception cref = "ArgumentOutOfRangeException">
		///   <paramref name = "argument" /> is out of the specified range.
		/// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void ArgumentInRange(decimal argument,
		                                   decimal minimumValue,
		                                   decimal maximumValue,
		                                   [NotNull] [InvokerParameterName] string argumentName)
		{
			Assert.ArgumentNotNull(argumentName, "argumentName");

			if (argument < minimumValue ||
			    argument >= maximumValue)
			{
				throw new ArgumentOutOfRangeException(argumentName,
				                                      string.Format(CultureInfo.CurrentCulture,
				                                                    "{0} was {1}, should be in the range [{2}, {3}).",
				                                                    argumentName,
				                                                    argument,
				                                                    minimumValue,
				                                                    maximumValue));
			}
		}

		/// <summary>
		///   Ensures that an argument to a function is in the range 
		///   <c>[<paramref name = "minimumValue" />,<paramref name = "maximumValue" />)</c> and throws an
		///   <see cref = "ArgumentOutOfRangeException" /> otherwise.
		/// </summary>
		/// <param name = "argument">The argument.</param>
		/// <param name = "minimumValue">The minimum value of the argument.</param>
		/// <param name = "maximumValue">The maximum value of the argument.</param>
		/// <param name = "argumentName">The name of the argument.</param>
		/// <exception cref = "ArgumentOutOfRangeException">
		///   <paramref name = "argument" /> is out of the specified range.
		/// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void ArgumentInRange(long argument,
		                                   long minimumValue,
		                                   long maximumValue,
		                                   [NotNull] [InvokerParameterName] string argumentName)
		{
			Assert.ArgumentNotNull(argumentName, "argumentName");

			if (argument < minimumValue ||
			    argument >= maximumValue)
			{
				throw new ArgumentOutOfRangeException(argumentName,
				                                      string.Format(CultureInfo.CurrentCulture,
				                                                    "{0} was {1}, should be in the range [{2}, {3}).",
				                                                    argumentName,
				                                                    argument,
				                                                    minimumValue,
				                                                    maximumValue));
			}
		}

		/// <summary>
		/// Ensures that an argument is a subtype of a particular type.
		/// </summary>
		/// <typeparam name="T">The type to test agains.</typeparam>
		/// <param name="argument">The argument.</param>
		/// <param name="argumentName">The name of the argument.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="argument"/> is an instance of <typeparamref name="T"/>.
		/// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void ArgumentIsSubtypeOf<T>([NotNull] object argument, [NotNull] string argumentName)
		{
			Assert.ArgumentNotNull(argument, "argument");
			Assert.ArgumentNotNull(argumentName, "argumentName");

			if (!(argument is T))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
				                                          "{0} should be an instance of {1}.",
				                                          argumentName,
				                                          typeof(T).Name));
			}
		}

		/// <summary>
		///   Ensures that an argument to a function is non-zero, and throws an <see cref = "ArgumentOutOfRangeException" /> 
		///   otherwise.
		/// </summary>
		/// <param name = "argument">The argument.</param>
		/// <param name = "argumentName">The name of the argument.</param>
		/// <exception cref = "ArgumentOutOfRangeException"><paramref name = "argument" /> is <c>0</c>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void ArgumentNonzero(long argument, [NotNull] [InvokerParameterName] string argumentName)
		{
			Assert.ArgumentNotNull(argumentName, "argumentName");

			if (argument == 0)
				throw new ArgumentOutOfRangeException(argumentName, argumentName + " cannot be 0.");
		}

		/// <summary>
		///   Ensures that an argument to a function is not <see langword = "null" /> and throws a 
		///   <see cref = "ArgumentNullException" /> otherwise.
		/// </summary>
		/// <param name = "argument">The argument.</param>
		/// <param name = "argumentName">The name of the argument.</param>
		/// <remarks>
		///   This should be called for every argument marked with <see cref = "NotNullAttribute" /> in public methods.
		/// </remarks>
		/// <exception cref = "ArgumentNullException"><paramref name = "argument" /> is <see langword = "null" />.</exception>
		/// <seealso cref = "Assert.ArgumentNotNull" />
		[AssertionMethod]
		public static void ArgumentNotNull([CanBeNull] [AssertionCondition(AssertionConditionType.IsNotNull)] object argument,
		                                   [NotNull] [InvokerParameterName] string argumentName)
		{
			Assert.ArgumentNotNull(argumentName, "argumentName");

			if (argument == null)
				throw new ArgumentNullException(argumentName);
		}

		/// <summary>
		///   Ensures that arguments to a function satisfy a condition, and throws an <see cref = "ArgumentException" />
		///   otherwise.
		/// </summary>
		/// <param name = "condition">The condition.</param>
		/// <param name = "message">The message.</param>
		/// <exception cref = "ArgumentException"><paramref name = "condition" /> is false.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void ArgumentsValid(bool condition, [NotNull] string message)
		{
			Assert.ArgumentNotNull(message, "message");

			if (!condition)
				throw new ArgumentException(message);
		}

		/// <summary>
		///   Ensures that all entries of an argument to a function are not <see langword = "null" /> and throws a 
		///   <see cref = "ArgumentException" /> otherwise.
		/// </summary>
		/// <param name = "argument">The argument.</param>
		/// <param name = "argumentName">The name of the argument.</param>
		/// <exception cref = "ArgumentException">An entry in <paramref name = "argument" /> is <see langword = "null" />.</exception>
		/// <seealso cref = "Assert.ArgumentNotNull" />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), AssertionMethod]
		public static void EntriesNotNull([NotNull] [AssertionCondition(AssertionConditionType.IsNotNull)] Array argument,
		                                  [NotNull] [InvokerParameterName] string argumentName)
		{
			Assert.ArgumentNotNull(argument, "argument");
			Assert.ArgumentNotNull(argumentName, "argumentName");

			var count = argument.Length;
			for (var i = 0; i < count; i++)
			{
				if (argument.GetValue(i) == null)
				{
					throw new ArgumentException(
						string.Format(CultureInfo.CurrentCulture, "{0} should not contain any null entries", argumentName), argumentName);
				}
			}
		}

		/// <summary>
		///   Ensures that an indexer has the value specified by <paramref name = "expectedIndex" />,
		///   and throws an <see cref = "IndexOutOfRangeException" /> otherwise.
		/// </summary>
		/// <param name = "index">The actual index value.</param>
		/// <param name = "expectedIndex">The expected value of the index.</param>
		/// <exception cref = "ArgumentOutOfRangeException">
		///   <paramref name = "index" /> is not equal to the expected value.
		/// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void IndexEquals(int index, int expectedIndex)
		{
			if (index != expectedIndex)
			{
				throw new ArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture,
				                                                    "index was {0}, should be {1}.",
				                                                    index,
				                                                    expectedIndex));
			}
		}

		/// <summary>
		///   Ensures that an indexer has the value specified by <paramref name = "expectedIndex" />,
		///   and throws an <see cref = "IndexOutOfRangeException" /> otherwise.
		/// </summary>
		/// <param name = "index">The actual index value.</param>
		/// <param name = "expectedIndex">The expected value of the index.</param>
		/// <exception cref = "ArgumentOutOfRangeException">
		///   <paramref name = "index" /> is not equal to the expected value.
		/// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void IndexEquals(long index, long expectedIndex)
		{
			if (index != expectedIndex)
			{
				throw new ArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture,
				                                                    "index was {0}, should be {1}.",
				                                                    index,
				                                                    expectedIndex));
			}
		}

		/// <summary>
		///   Ensures that an indexer is in the range 0 to <paramref name = "maximumValue" />. 
		///   and throws an <see cref = "IndexOutOfRangeException" /> otherwise.
		/// </summary>
		/// <param name = "index">The actual index value.</param>
		/// <param name = "maximumValue">The maximum permitted value of the index.</param>
		/// <exception cref = "ArgumentOutOfRangeException">
		///   <paramref name = "index" /> is out of the specified range.
		/// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void IndexInRange(int index, int maximumValue)
		{
			if (index < 0 ||
			    index > maximumValue)
			{
				var msg = (maximumValue == -1)
				          	? "index {0} was out of range."
				          	: "index was {0}, should be in the range [0, {1}].";

				throw new ArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture, msg, index, maximumValue));
			}
		}

		/// <summary>
		///   Ensures that an indexer is in the range 0 to <paramref name = "maximumValue" />. 
		///   and throws an <see cref = "IndexOutOfRangeException" /> otherwise.
		/// </summary>
		/// <param name = "index">The actual index value.</param>
		/// <param name = "maximumValue">The maximum permitted value of the index.</param>
		/// <exception cref = "ArgumentOutOfRangeException">
		///   <paramref name = "index" /> is out of the specified range.
		/// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void IndexInRange(long index, long maximumValue)
		{
			if (index < 0 ||
			    index > maximumValue)
			{
				var msg = (maximumValue == -1)
				          	? "index {0} was out of range."
				          	: "index was {0}, should be in the range [0, {1}].";

				throw new ArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture, msg, index, maximumValue));
			}
		}

		/// <summary>
		///   Ensures that the specified condition is true.
		/// </summary>
		/// <param name = "condition">The condition to ensure.</param>
		/// <exception cref = "InvalidOperationException"><paramref name = "condition" /> is false.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), AssertionMethod]
		public static void IsTrue([AssertionCondition(AssertionConditionType.IsTrue)] bool condition)
		{
			if (!condition)
				throw new InvalidOperationException();
		}

		#endregion
	}
}