using System;
using System.Diagnostics;

namespace XmlPrime.Contracts
{
	/// <summary>
	/// Indicates that the value of marked element could sometimes be <see langword="null"/>, so a check for 
	/// <see langword="null" /> is necessary before its usage.
	/// </summary>
	[Conditional("ANNOTATION")]
	[AttributeUsage(
		AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Delegate |
		AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	internal sealed class CanBeNullAttribute : Attribute
	{
	}
}