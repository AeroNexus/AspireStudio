using System;

using Aspire.Core.Messaging;
using Aspire.Core.xTEDS;

namespace Aspire.Core
{
	class AppMessage : IDataMessage, IPublisher
	{
		//SubscriptionCallback mSubscriptionCallback;

		//typedef void (*SubscriptionCallback)(object context, Subscription subscription, bool cancel);

		public AppMessage():this(false)
		{
		}

		public AppMessage(bool isEvent)
		{
			mIsEvent = isEvent;
		} bool mIsEvent;

		public int AllowableLeasePeriod
		{
			get { return Int32.MaxValue; }
			set { }
		}

		public bool IsEvent
		{
			get { return mIsEvent; }
		}

		public bool IsProvider
		{
			get { return false; }
		}

		public MessageId MessageId
		{
			get { return MessageId.Empty; }
		}

		public string Name
		{
			get { return "AppMessage"; }
		}

		public Address Provider
		{
			get { return Address.Empty; }
		}

		public string ProviderName
		{
			get { return string.Empty; }
		}

		public Publication Publication
		{
			get { return mPublication; }
			set { mPublication = value;}
		} Publication mPublication;

		public void RaiseSubscription(Subscription subscription, bool cancel)
		{
			if (SubscribedTo != null)
				SubscribedTo(this, subscription, cancel);
		}

		public void RaiseUnpublished(Address subscriber) { }

		public event SubscribedToHandler SubscribedTo;

		public bool Verified { get { return false; } set { } }

		public XtedsMessage XtedsMessage { get { return null; } }
	}
}
