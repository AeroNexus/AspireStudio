using System;

namespace Aspire.Framework
{
	/// <summary>
	/// Something that can be published on the Blackboard
	/// </summary>
	public interface IPublishable
	{
		/// <summary>
		/// A fully rooted path to a published item
		/// </summary>
		string Path { get; set; }

		/// <summary>
		/// A simple moniker, used as te leaf node in a full path. May include '.'
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// The parent published item
		/// </summary>
		IPublishable Parent { get; set; }
	}
}
