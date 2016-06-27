using System;
using System.Collections.Generic;
using System.ComponentModel;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;

namespace Aspire.Core.xTEDS
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class Subscription
	{
		public Address mAddress;
		IPublisher mPublisher;
		public enum OpState { Initial, Active, Dormant, Expired }
		OpState mState, mPreviousState;
		Timer mExpireTimer;
		int mLeasePeriod;
		byte mIth, mPriority;
		internal LinkedListNode<Subscription> mNode;

		public Subscription(Address address, byte ith, byte priority)
		{
			mAddress = address;
			mIth = ith > 0 ? ith : (byte)1;
			mPriority = priority;
			mPreviousState = OpState.Initial;
			mState = OpState.Dormant;
		}

		public void Activate(Publication publication, ref int leasePeriod)
		{
			int allowableLeasePeriod = publication.AllowableLeasePeriod;
			if (leasePeriod < 0) leasePeriod = 0;

			// Pick the lesser, but accomodate 0=forever
			if ( allowableLeasePeriod > 0 && ( leasePeriod == 0 || allowableLeasePeriod < leasePeriod ) )
				leasePeriod = allowableLeasePeriod;
			mLeasePeriod = leasePeriod;
			mPreviousState = mState;

			// Renewing:
			if ( mState == OpState.Active )
			{
				if ( mExpireTimer != null)
				{
					// Just push the timer out
					if (leasePeriod > 0)
					{
						//var msg = mPublisher.Publication.mDataMessage;
						//Log.WriteLine("ct {0} {1}",msg.ProviderName,msg.Name);
						mExpireTimer.Change(leasePeriod, 0);
					}
					else
					{
						// or not at all
						mExpireTimer.Cancel();
						mExpireTimer = null;
					}
				}
				return;
			}

			mPublisher = publication.mDataMessage as IPublisher;

			// First time. Might need a limited lease
			if ( leasePeriod > 0 )
				mExpireTimer = Timer.Set(leasePeriod, 0, Timer.Trigger.OneShot, new Timer.TimeoutHandler(OnLeaseExpire));
			else if ( mExpireTimer != null )
			{
				mExpireTimer.Cancel();
				mExpireTimer = null;
			}

			mState = OpState.Active;
		}

		public Address Address { get { return mAddress; } }

		public bool Cancel()
		{
			bool canceled = mState == OpState.Active;
			mState = OpState.Dormant;
			if ( mExpireTimer != null )
			{
				mExpireTimer.Cancel();
				mExpireTimer = null;
			}
			//mNode.List.Remove(mNode);
			return canceled;
		}

		public void OnLeaseExpire()
		{
			var list = mNode.List;
			if (list != null)
			{
				var subs = list as SubscriptionList;
				var rwl = subs.Lock;
				if ( rwl.AcquireWrite() )
				{
					mNode.Value.mExpireTimer = null;
					list.Remove(mNode);
					subs.mPublication.Remove(mAddress);
					rwl.ReleaseWrite();
				}

				if (list.Count == 0)
					mPublisher.RaiseUnpublished(mAddress);
			}
			mExpireTimer = null;
			MsgConsole.WriteLine("{4} {0}.{1}'s subscription to {2} expired after {3}s",
				(mPublisher as IDataMessage).XtedsMessage.ProviderName,
				(mPublisher as IDataMessage).XtedsMessage.Name, mAddress.ToString(), mLeasePeriod,
				DateTime.Now.ToLongTimeString());
		}

		public OpState PreviousState { get { return mPreviousState; } }

		public bool ShouldPublish(uint count)
		{
			return mState == OpState.Active && count%mIth==0;
		}

		public OpState State { get { return mState; } }
	}

	public class SafeReaderWriterLock
	{
		System.Threading.ReaderWriterLock mLock = new System.Threading.ReaderWriterLock();
		public int Timeout{set{mTimeout = value;}}
		int mTimeout;
		public bool AcquireRead()
		{
			try
			{
				mLock.AcquireReaderLock(mTimeout);
			}
			catch (ApplicationException ex)
			{
				if (ex.Message != null)
					return false;
				return false;
			}
			return true;
		}
		public bool AcquireWrite()
		{
			try
			{
				if (mLock.IsReaderLockHeld)
					mLock.UpgradeToWriterLock(mTimeout);
				else
					mLock.AcquireWriterLock(mTimeout);
			}
			catch (ApplicationException ex)
			{
				if (ex.Message != null)
					return false;
				return false;
			}
			return true;
		}
		public void ReleaseRead(){ mLock.ReleaseReaderLock(); }
		public void ReleaseWrite(){ mLock.ReleaseWriterLock(); }
		public bool UpgradeToWrite()
		{
			try
			{
				mLock.UpgradeToWriterLock(mTimeout);
			}
			catch (ApplicationException ex)
			{
				if (ex.Message != null)
					return false;
				return false;
			}
			return true;
		}
	}

	public class SubscriptionList : LinkedList<Subscription>
	{
		internal SafeReaderWriterLock Lock = new SafeReaderWriterLock();
		internal Publication mPublication;

		//Lock.Timeout = 100;

		public SubscriptionList(Publication pub)
		{
			mPublication = pub;
		}

		public Subscription this[int index]
		{
			get
			{
				if (index >= Count)
					return null;
				var node = First;
				for (int i=0; i<index; i++)
					node = node.Next;
				return node.Value;
			}
		}
	}

}
