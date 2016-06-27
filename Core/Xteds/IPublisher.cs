using System;

using Aspire.Core.Messaging;

namespace Aspire.Core.xTEDS
{
	public delegate void SubscribedToHandler(IDataMessage message, Subscription subscription, bool cancel);

	public interface IPublisher
	{
		bool IsEvent { get; }
		bool IsProvider { get; }
		Publication Publication { get; set; }
		void RaiseSubscription(Subscription subscription, bool cancel);
		void RaiseUnpublished(Address subscriber);
		event SubscribedToHandler SubscribedTo;
	}
}
