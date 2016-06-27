using System;

namespace Aspire.Framework
{
	/// <summary>
	/// InstancedAttribute used between Instance.Loader and loaded component's properties
	/// </summary>
	[AttributeUsage(
		 AttributeTargets.Field | AttributeTargets.Property)]
	public class InstancedAttribute : Attribute
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public InstancedAttribute() { }
	}
}
