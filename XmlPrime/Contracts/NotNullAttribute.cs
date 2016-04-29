using System;
using System.Diagnostics;

namespace XmlPrime.Contracts
{
	/// <summary>
	/// Indicates that the value of marked element could never be <see langword="null" />
	/// </summary>
	[Conditional("ANNOTATION")]
	[AttributeUsage(
		AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Delegate |
		AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	internal sealed class NotNullAttribute : Attribute
	{
	}
}