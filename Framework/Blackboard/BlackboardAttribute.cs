using System;

namespace Aspire.Framework
{
	/// <summary>
	/// Publish the field, property, event or rule on the Blackboard. Elements must be in a class that imlements IPublishable
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = false)]
	public class BlackboardAttribute : Attribute
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public BlackboardAttribute()
		{
			EntryName = string.Empty;
			Units = string.Empty;
		}

		/// <summary>
		/// Construct with the alternate name instead of te elements natural name. Can be separated.
		/// </summary>
		/// <param name="name">The altername name to publish with</param>
		public BlackboardAttribute(string name)
		{
			EntryName = name;
			Units = string.Empty;
		}

		/// <summary>
		/// Access the entry name for publishing
		/// </summary>
		public string EntryName { get; set; }

		/// <summary>
		/// Access the description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Access the read only flag
		/// </summary>
		public bool IsReadOnly { get; set; }

		/// <summary>
		/// Access the units label
		/// </summary>
		public string Units { get; set; }

	}
}
