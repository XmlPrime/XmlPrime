using System;
using System.Diagnostics;

namespace XmlPrime.Contracts
{
	/// <summary>
	/// Indicates the condition parameter of the assertion method. 
	/// The method itself should be marked by <see cref="AssertionMethodAttribute"/> attribute.
	/// The mandatory argument of the attribute is the assertion type.
	/// </summary>
	/// <seealso cref="AssertionConditionType"/>
	[Conditional("ANNOTATION")]
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	internal sealed class AssertionConditionAttribute : Attribute
	{
		#region Private Fields

		private readonly AssertionConditionType _conditionType;

		#endregion

		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="AssertionConditionAttribute"/> class.
		/// </summary>
		/// <param name="conditionType">The type of condition.</param>
		public AssertionConditionAttribute(AssertionConditionType conditionType)
		{
			_conditionType = conditionType;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the condition type.
		/// </summary>
		/// <value>The condition type.</value>
		public AssertionConditionType ConditionType
		{
			get
			{
				return _conditionType;
			}
		}

		#endregion
	}
}