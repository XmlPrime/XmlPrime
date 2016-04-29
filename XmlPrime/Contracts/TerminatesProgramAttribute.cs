using System;
using System.Diagnostics;

namespace XmlPrime.Contracts
{
	/// <summary>
	/// Indicates that the value of marked element could never be <c>null</c>.
	/// </summary>
	[Conditional("ANNOTATION")]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	internal sealed class TerminatesProgramAttribute : Attribute
	{
	}
}