using System;

namespace Aspire.Framework
{
	/// <summary>
	/// Interface describing a provider of late bound Blackboard Items
	/// </summary>
	public interface IAsyncItemProvider
	{
		/// <summary>
		/// Matching condition
		/// </summary>
		bool Matches(string path);
		/// <summary>
		/// A filtering prefix
		/// </summary>
		string Prefix { get; }
		/// <summary>
		/// Attemps to create and publish a Blackboard Item based on a failed subscription request
		/// </summary>
		/// <param name="path">Full path in the Blackboard</param>
		void Provide(string path);
		/// <summary>
		/// A more specific provider has become available, requiring the current provider
		/// to transfer all of its items that match the new provider
		/// </summary>
		/// <param name="provider"></param>
		void TransferItemsTo(IAsyncItemProvider provider);
		/// <summary>
		/// Verifies with the provider that the path will produce data
		/// </summary>
		/// <param name="path">Full path in the Blackboard</param>
		void Verify(string path);
	};
}
