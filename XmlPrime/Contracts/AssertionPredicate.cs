using System;
using XmlPrime.Contracts;

namespace XmlPrime.Contracts
{
	/// <summary>
	/// A predicate with a description for use in assertions.
	/// </summary>
	/// <typeparam name="T">The type of the argument being validated.</typeparam>
	internal class AssertionPredicate<T>
	{
		/// <summary>
		/// Gets the predicate.
		/// </summary>
		/// <value>The predicate.</value>
		public Predicate<T> Predicate
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a description of the predicate (where <c>{0}</c> will be replaced with the name of the argument).
		/// </summary>
		public string Description
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AssertionPredicate&lt;T&gt;"/> class with a default message.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		public AssertionPredicate([NotNull] Predicate<T> predicate)
		{
			Ensure.ArgumentNotNull(predicate, "predicate");
			
			Predicate = predicate;
			Description = "{0} must satisfy " + predicate;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AssertionPredicate&lt;T&gt;"/> class with a default message.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="description">The description.</param>
		public AssertionPredicate([NotNull] Predicate<T> predicate, [NotNull] string description)
		{
			Ensure.ArgumentNotNull(predicate, "predicate");
			Ensure.ArgumentNotNull(description, "description");
			
			Predicate = predicate;
			Description = description;
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="System.Predicate&lt;T&gt;"/> to <see cref="AssertionPredicate&lt;T&gt;"/>.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns>The result of the conversion.</returns>
		[NotNull]
		public static implicit operator AssertionPredicate<T>([NotNull] Predicate<T> predicate)
		{
			Assert.ArgumentNotNull(predicate, "predicate");

			return new AssertionPredicate<T>(predicate);	
		}


	}
}
