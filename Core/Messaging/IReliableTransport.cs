
namespace Aspire.Core.Messaging
{
	interface IReliableTransport
	{
		bool SupportsReliableDelivery { get; }
		bool SupportsBestEffortDelivery { get; }
		bool SupportsBroadcast { get; }
		bool SupportsMulticast { get; }
	}
}
