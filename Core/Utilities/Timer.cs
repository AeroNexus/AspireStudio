using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Serialization;

namespace Aspire.Core.Utilities
{
	public class Timer
	{
		public enum Trigger {OneShot, Periodic }
		public delegate void TimeoutHandler();

		internal LinkedListNode<Timer> mNode;
		internal TimerList mList;
		internal SecTime mExpirationTime;
		SecTime mPeriod;
		static SecTime now;
		Trigger mTrigger;
		TimeoutHandler mHandler;
		bool mEnabled = true;
		//int tag;
		//static int tags=1;

		private Timer(SecTime period, Trigger trigger, TimeoutHandler handler)
		{
			mPeriod = period;
			mTrigger = trigger;
			mHandler = handler;
			mNode = null;
		//	tag = tags++;
		}

		private void AddToFree()
		{
			lock (TimerList.free)
			{
				TimerList.free.AddFirst(this);
				mNode = null;
				mEnabled = false;
			}
		}

		public void Cancel()
		{
			if ( mNode == null )
				return; // already on free list

			lock(mList)
			{
				if (mList.Count > 0)
				{
					if ( mNode != null )
						mList.mList.Remove(mNode);
					if (mList.Count == 0)						// prior to dispatching
					{
						mList.mPollPeriodUs = TimerList.emptyPollPeriodUs;
						if ( mList.IsThreaded )
							mList.mThread.Interrupt();
					}
				}
				AddToFree();
			}
		}

		public void Change(int periodSec, int periodUSec)
		{
			LinkedListNode<Timer> node=null;
			lock (mList)
			{
				if (mNode != null)
				{
					mList.mList.Remove(mNode);
					if (mNode.Next != null)
						node = mNode.Next;
					else if (mNode.Previous != null)
						node = mNode.Previous;
				}
				if ( node == null )
					node = mList.mList.First;
			}
			mPeriod.Set(periodSec, periodUSec);
			mEnabled = true;
			Clock.GetGps(ref now);
			
			Reschedule(ref node, now);
		}

		public void Change(SecTime period)
		{
			LinkedListNode<Timer> node;
			lock (mList)
			{
				if (mNode != null)
					mList.mList.Remove(this.mNode);
				node = mList.mList.First;
			}
			mPeriod = period;
			mEnabled = true;
			Clock.GetGps(ref now);

			Reschedule(ref node, now);
		}

		internal Timer Dispatch()
		{
			if (mTrigger == Trigger.Periodic)
			{
				mHandler();
				if (mTrigger == Trigger.OneShot)
				{
					AddToFree();
					return null;
				}
				else
					return this;
			}
			else
			{
				AddToFree();
				mHandler();
			}
			return null;
		}

		public bool Enable
		{
			get { return mEnabled; }
			set
			{
				if (mEnabled != value)
				{
					mEnabled = value;
					if (mEnabled)
					{
						var now = new SecTime();
						Clock.GetTime(ref now);
						LinkedListNode<Timer> node;
						lock (mList)
						{
							node = mList.mList.First;
							Reschedule(ref node, now);
						}
					}
					else
						Remove();
				}
			}
		}

		private static Timer Get(SecTime period, Trigger trigger, TimeoutHandler handler)
		{
			Timer timer;

			lock (TimerList.free)
			{
				if (TimerList.free.Count > 0)
				{
					timer = TimerList.free.First.Value;
					TimerList.free.RemoveFirst();

					timer.mPeriod = period;
					timer.mTrigger = trigger;
					timer.mHandler = handler;
					timer.mEnabled = true;
				}
				else
					timer = new Timer(period, trigger, handler);
			}

			//SecTime now = new SecTime();
			//Clock.GetGps(ref now);
			//MsgConsole.WriteLine("GetTimer {0}, {1} @ {2}",timer.mPeriod.ToString(),timer.tag,now.ToString());
			return timer;
		}

		void Remove()
		{
			if ( this.mNode == null )
				return; // already on free list

			if ( mNode.Previous == mNode )					// Only 1 entry
				mList.mList.RemoveFirst();
			else 
			{									// Ungraft it from the list
				if ( mNode.Previous.Next != mNode ) 
				{
					Logger.Log(1,"Timer.Cancel:Trying to cancel timer not on {{0}}", mList.ToString());
					return;
				}
				else 
				{
					mList.mList.Remove(this);
				}
			}
		}

		internal Timer Reschedule(ref LinkedListNode<Timer> head, SecTime referenceTime)
		{
			SecTime time = referenceTime + mPeriod;
			mExpirationTime = time;

			if (head == null)
			{
				mNode = mList.mList.AddFirst(this); // Make this the first timeout entry
				head = mNode;
			}
			else
			{
				lock (mList)
				{
					// 	Search for first timer that expires AFTER us
					int i = 0;	// counter to prevent an infinite loop.
					LinkedListNode<Timer> node = head;
					do
					{
						i++;
						if (node.Value.mExpirationTime > time) break;
						node = node.Next;
					} while (node != null && i < Int32.MaxValue - 1);
					//	Found the slot. Now graft the timer in place
					if (node != null)
						mNode = mList.mList.AddBefore(node, this);
					else
						mNode = mList.mList.AddLast(this);
					if (time < head.Value.mExpirationTime)
						head = this.mNode;
				}
			}

			return this;
		}

		public static Timer Set(int periodSec, int periodUSec, Trigger trigger, TimeoutHandler handler, bool enabled = true)
		{
			SecTime period = new SecTime(periodSec, periodUSec);
			Clock.GetTime(ref now);
			var timer = Get(period, trigger, handler);
			if (!enabled)
			{
				timer.mEnabled = false;
				timer.mList = TimerList.active;
				return timer;
			}
			else
				return TimerList.active.Add( timer, now );
		}
		public static Timer Set(SecTime period, Trigger trigger, TimeoutHandler handler, bool enabled = true)
		{
			Clock.GetTime(ref now);
			var timer = Get(period, trigger, handler);
			if (!enabled)
			{
				timer.mEnabled = false;
				timer.mList = TimerList.active;
				return timer;
			}
			else
				return TimerList.active.Add(timer, now);
		}
	}

	class TimerList
	{
		internal LinkedList<Timer> mList = new LinkedList<Timer>();
		string mName;
		internal Thread mThread;
		internal int mPollPeriodUs;
		SecTime mThreshold;

		internal const int emptyPollPeriodUs = 100000;
		const int TimerResolutionNanoseconds = 120000; // Win32

		internal static TimerList active = new TimerList("active timers");
		public static TimerList The { get { return active; } }
		internal static LinkedList<Timer> free = new LinkedList<Timer>();

		public TimerList(string name, bool isThreaded=false)
		{
			mName = name;
			mPollPeriodUs = emptyPollPeriodUs;
			mThreshold.Set(0,TimerResolutionNanoseconds/1000);
			IsThreaded = isThreaded;
		}

		public Timer Add(Timer timer, SecTime referenceTime)
		{
			//if (timer.mNode == null) return timer; // Disables timers for now
			lock (this)
			{
				LinkedListNode<Timer> head = mList.First;
				timer.mList = this;
				timer = timer.Reschedule(ref head, referenceTime);
				int newPollPeriod;

				if (timer == head.Value)
				{
					SecTime diff = timer.mExpirationTime - referenceTime;
					if (diff > 0)
					{
						newPollPeriod = diff.ToMicroSeconds;
						if (newPollPeriod > 1000000) newPollPeriod = 1000000;
					}
					else
					{
						newPollPeriod = 0;
						//    putchar('@');
					}
					if (mThread == null && IsThreaded)
					{
						mThread = new Thread(new ThreadStart(TimerThread));
						mThread.IsBackground = true;
						mThread.Name = "Aspire timers";
						mThread.Start();
					}
					else if (newPollPeriod != mPollPeriodUs)
					{
						mPollPeriodUs = newPollPeriod;
						if ( IsThreaded ) mThread.Interrupt();
					}
				}
			}
			return timer;
		}

		public void Clear()
		{
			lock (this)
			{
				LinkedListNode<Timer> node = mList.First;
				if (node != null)
				{
					do
					{
						node.Value.Cancel();
						node = node.Next;
					} while (node != mList.First);
				}
			}
		}

		public int Count{ get{return mList.Count;} }

		public void Dispatch(SecTime referenceTime)
		{
			SecTime diff;
			Timer resched;
			int newPollPeriod = mPollPeriodUs;

			lock(this)
			{
				LinkedListNode<Timer> head = mList.First, node = head;
				while ( head != null )
				{
					if (head.Value.mExpirationTime > referenceTime + mThreshold) // timers still active
					{
						diff = head.Value.mExpirationTime - referenceTime;
						if (diff > 0)
						{
							newPollPeriod = diff.ToMicroSeconds;
							if (newPollPeriod > 1000000) newPollPeriod = 1000000;
						}
						else
						{
							newPollPeriod = 0;
							//putchar('@');
						}
						break;
					}
					node = head;
					if (node == node.Next)	// is timer last one on Q ?
						head = null;
					else
					{
						head = node.Next;
						mList.Remove(node);
					}
					resched = node.Value.Dispatch();
					if ( resched != null)
						resched.Reschedule( ref head, referenceTime );
				}
				if ( mList.Count == 0 )
					newPollPeriod = emptyPollPeriodUs;
			}
			if (newPollPeriod != mPollPeriodUs)
			{
				mPollPeriodUs = newPollPeriod;
				lock(this)
					if ( IsThreaded ) mThread.Interrupt();
			}
		}

		[XmlIgnore]
		public bool IsThreaded { get; set; }

		[MTAThread]
		void TimerThread()
		{
			bool forever = true;
			SecTime now = new SecTime();
			// start a thread that dispatches timers. microseconds is the time until the next timer
			while(forever)
			{
				Clock.GetGps(ref now);
				Dispatch(now);
				try
				{
					Thread.Sleep(mPollPeriodUs/1000);
				}
				catch (ThreadInterruptedException)
				{
				}
			}
		}

		internal void Remove(Timer timer)
		{
			lock(this)
				mList.Remove(timer.mNode);
		}

		public override string ToString(){return mName;}

		public SecTime UntilEarliestDeadline(SecTime referenceTime)
		{
			if (mList.Count == 0)
				return SecTime.Infinite;

			return mList.First.Value.mExpirationTime - referenceTime;
		}
	};

}
