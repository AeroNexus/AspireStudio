//using System;
using System.ComponentModel;
//using System.Xml.Serialization;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;

namespace Aspire.Core
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class Publication
	{
		internal IDataMessage mDataMessage;
		ControlProtocol mControlProtocol;
		SubscriptionList mSubscriptions;
		Timer mRenewTimer;
		uint mPublishCount;
		Address mClient;
		AddressList mAddressList;
		SafeReaderWriterLock mRwLock;
		byte mIth = 1;

		public bool resumeCanceled;

		public Publication(IPublisher publisher)
		{
			mDataMessage = publisher as IDataMessage;
		}

		public Publication(IPublisher publisher, Address client)
		{
			mClient = client;
			mDataMessage = publisher as IDataMessage;
		}

		public Subscription AddIfRequired(Address source, byte Ith, byte Priority, ref int leasePeriod)
		{
			Subscription newSub;
			if (mSubscriptions == null)
			{
				mSubscriptions = new SubscriptionList(this);
				mRwLock = mSubscriptions.Lock;
				mAddressList = source.CreateList();
				newSub = new Subscription(source.Clone(),Ith, Priority);
				if ( mRwLock.AcquireWrite() )
				{
					newSub.mNode = mSubscriptions.AddLast(newSub);
					mAddressList.Add(newSub.Address);
					mRwLock.ReleaseWrite();
				}
			}
			else
			{
				if (mRwLock.AcquireRead())
				{
					foreach (var sub in mSubscriptions)
						if (source.EqualsNetwork(sub.mAddress))
						{
							sub.Activate(this, ref leasePeriod);
							mRwLock.ReleaseRead();
							return sub;
						}
					newSub = new Subscription(source.Clone(), Ith, Priority);
					if (mRwLock.UpgradeToWrite())
					{
						newSub.mNode = mSubscriptions.AddLast(newSub);
						mAddressList.Add(newSub.Address);
						mRwLock.ReleaseWrite();
					}
				}
				else
					newSub = null;
			}
			if ( newSub !=null )
				newSub.Activate(this,ref leasePeriod);
			return newSub;
		}

		public int AllowableLeasePeriod
		{
			get { return mDataMessage.AllowableLeasePeriod; }
			set { mDataMessage.AllowableLeasePeriod = value; }
		}

		public bool AutomaticLeaseRenewal
		{
			get { return mAutomaticLeaseRenewal; }
			set { mAutomaticLeaseRenewal = value; }
		} bool mAutomaticLeaseRenewal=true;

		public void AutomaticallyRenewLease(int grantedLeasePeriod, ControlProtocol controlProtocol)
		{
			mControlProtocol = controlProtocol;
			if (!mAutomaticLeaseRenewal) return;
			
			int renewPeriod = grantedLeasePeriod-1;
			if ( renewPeriod < 1 ) renewPeriod = 1;

			if ( mRenewTimer != null)
				mRenewTimer.Change(renewPeriod,0);
			else
				mRenewTimer = Timer.Set(renewPeriod, 0, Timer.Trigger.OneShot, new Timer.TimeoutHandler(OnLeaseRenew));
		}

		public int Cancel(Address source, out Subscription subscription)
		{
			int count = 0;
			bool canceled=false;
			subscription = null;
			if ( mRwLock.AcquireWrite() )
			{
				foreach (var sub in mSubscriptions)
					if (sub.Address.Equals(source))
					{
						canceled = sub.Cancel();
						subscription = sub;
						mAddressList.Remove(source);
					}
					else
						count++;
				mRwLock.ReleaseWrite();
			}
			return canceled?count:count+1;
		}

		public void CancelLeaseRenewal()
		{
			if (mRenewTimer != null)
			{
				mRenewTimer.Cancel();
				mRenewTimer = null;
			}
		}

		public void ChangeSubscription(int Ith, int priority, int leasePeriod)
		{
			if ( mIth == Ith ) return;
			mIth = (byte)Ith;
			//mPriority = priority;		// we'll get to these later
			//mLeasePeriod = period;
			if (mControlProtocol != null)
				mControlProtocol.SendSubscriptionRequest(mDataMessage.Provider,
					mDataMessage.MessageId,mIth,(byte)priority,(byte)leasePeriod);
		}

		public void ClearSubscriptions()
		{
			if ( mRwLock.AcquireWrite() )
			{
				foreach (var sub in mSubscriptions)
					sub.Cancel();
				mSubscriptions.Clear();

				mAddressList.Clear();
				mRwLock.ReleaseWrite();
			}
		}

		public int Dialog { get; set; }

		public int Ith { set { mIth = (byte)value; } }

		public MessageId MessageId { get { return mDataMessage.MessageId; } }

		public string MessageName { get { return mDataMessage.Name; } }

		void OnLeaseRenew()
		{
			mRenewTimer = null; // since it has expired

			//Log.WriteLine("{2} Renewing lease on {0} from {1}",mDataMessage.Name,mDataMessage.Provider,
			//    Clock.Elapsed);
			if (mControlProtocol != null)
				mControlProtocol.SendSubscriptionRequest(mDataMessage.Provider,
					mDataMessage.MessageId,mIth,0,0,  mClient);
			else
				MsgConsole.WriteLine("null control protocol while trying to renew lease to {0} @{1}",
					mDataMessage.Name,mDataMessage.Provider.ToString() );
		}

		public Address Provider { get { return mDataMessage.Provider; } }

		public void Publish(IDataMessage message, XtedsProtocol xtedsProtocol)
		{
			// For now, aspire 2 removed SendMultiple functionality
			mPublishCount++;

			foreach (var sub in mSubscriptions)
			{
				if (sub.State == Subscription.OpState.Active &&
					sub.ShouldPublish(mPublishCount) )
					xtedsProtocol.Send(message.XtedsMessage, sub.Address);
			}

			// This is more efficient when SendMultiple is fully suported

			//if (mSubscriptions.Count == 0) return;
			//mPublishCount++;
			//if (mSubscriptions.Count == 1)
			//{
			//	var sub = mSubscriptions.First.Value;
			//	if ( sub.ShouldPublish(mPublishCount) )
			//		xtedsProtocol.Send(message.XtedsMessage, sub.Address, 0);
			//}
			//else
			//{
			//	if (mRwLock.AcquireRead())
			//	{
			//		int i=0;
			//		foreach (var sub in mSubscriptions)
			//			mAddressList[i++].enabled = sub.ShouldPublish(mPublishCount);
			//		mRwLock.ReleaseRead();
			//	}
			//	xtedsProtocol.SendMultiple(message, mAddressList);
			//}
		}

		public void Remove(Address address)
		{
			mAddressList.Remove(address);
		}

		public SubscriptionList Subscriptions
		{
			get { return mSubscriptions; }
		}
	}
}
