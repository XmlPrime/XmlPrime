using System;
using System.Diagnostics;

namespace XmlPrime.Contracts
{
	/// <summary>
	/// Indicates that the function argument should be string literal and match one of the parameters of the caller 
	/// function.  For example, <see cref="ArgumentNullException"/> has such parameter.
	/// </summary>
	[Conditional("ANNOTATION")]
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	internal sealed class InvokerParameterNameAttribute : Attribute
	{
	}
}