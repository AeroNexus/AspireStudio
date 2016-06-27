using System;
using System.ComponentModel;
using System.Threading;

using Aspire.Utilities;

namespace Aspire.Framework
{
	class TimerList
	{
		internal TimerList(string name)
		{
			this.name = name;
		}
		internal string name;
		public override string ToString()
		{
			return name;
		}
		internal Timer head;
		/// <summary>
		/// Add a timer to the appropriate location in the expiration list
		/// </summary>
		/// <param name="timer"></param>
		/// <param name="referenceTime"></param>
		internal Timer Add(Timer timer, double referenceTime)
		{
			lock (this)
			{
				count++;
				timer.list = this;
				return timer.ReSchedule(ref head, referenceTime);
			}
		}

		/// <summary>
		/// Clear out all the entries
		/// </summary>
		internal void Clear()
		{
			lock (this)
			{
				Framework.Timer timer = head;
				if (timer != null)
					do
					{
						var next = timer.next;
						timer.Cancel();
						timer = next;
					} while (timer != null && timer != head);
			}
		}
		/// <summary>
		/// Get the count of timers awaiting expiration
		/// </summary>
		internal int Count
		{
			get { return count; }
			set { count = value; }
		} int count;

		internal void Dispatch(double referenceTime)
		{
			Framework.Timer timer, resched;

			lock (this)
			{
				while (head != null)
				{
					if (head.expirationTime > referenceTime)	// timers still active
						break;
					timer = head;
					if (timer == timer.next)					// is timer last one on Q ?
						head = null;
					else
					{
						head = timer.next;
						timer.next.prev = timer.prev;
						timer.prev.next = timer.next;
					}
					resched = timer.Dispatch();
					if (resched != null)
						resched.ReSchedule(ref head, referenceTime);
					else
					{
						count--;
						if (count == 0) head = null;
					}
				}
			}
		}
		internal int IntervalMs
		{
			get { return intervalMs; }
			set
			{
				if (value <= 0) return;
				if (intervalMs > 0 && value < intervalMs)
				{
					intervalMs = value;
					//if (Timer.wallTimer != null)
					//	Timer.wallTimer.Change(0, intervalMs);
				}
			}
		} int intervalMs = Timeout.Infinite;
		internal void SetInterval(int interval) { intervalMs = interval; }

		internal void Reschedule(double referenceTime, string where)
		{
			lock (this)
			{
				if (head == null) return;

				Timer temp = head;
				head = null;
				Timer timer = temp, nextTimer;

				do
				{
					//if ( timer.interval == 0 )
					//    timer.interval = timer.expirationTime;
					nextTimer = timer.next;
					timer.ReSchedule(ref head, referenceTime);
					timer = nextTimer;
				} while (timer != temp);
			}
		}
	}

	/// <summary>
	/// How often does the timer occur: once or periodically.
	/// </summary>
	public enum TimerOccurance
	{
		/// <summary>
		/// The timer occurs once after interval seconds.
		/// </summary>
		OneShot,
		/// <summary>
		/// The timer occurs periodically after interval seconds and thereafter
		/// every interval seconds.
		/// </summary>
		Periodic
	}

	/// <summary>
	/// Timer class delay dispatches delegates 
	/// </summary>
	public class Timer
	{
		static TimerList freeTimers = new TimerList("free timers");
		static TimerList wallTimers = new TimerList("Wall Timers");

		internal Timer next, prev;
		internal double expirationTime;
		Callback callback;
		TimerOccurance occurance;
		internal TimerList list;
		//		static int tagId = 1;
		//		int tag = tagId++;
		/// <summary>
		/// Method signature for the timer callbacks.
		/// </summary>
		public delegate void Callback();

		Timer(double interval, Callback callback)
		{
			this.interval = interval;
			this.callback = callback;
		}

		void AddToFree()
		{
			lock (freeTimers)
			{
				freeTimers.Count++;
				//				SimulationConsole.WriteLine("{0} {1} on free({2})", tag,interval,freeTimers.Count);

				next = freeTimers.head;
				freeTimers.head = this;
				prev = null;
			}
		}

		/// <summary>
		/// Cancel an existing Timer
		/// </summary>
		/// <returns>The next timer in the list</returns>
		public void Cancel()
		{
			if (prev == null)
				return; // already on free list
			lock (list)
			{
				//				SimulationConsole.WriteLine("Cancelling {0} {1}, prev {2}, prev.next {3} next {4} next.prev {5}",
				//					tag, interval, prev.tag, prev.next.tag, next.tag, next.prev.tag);
				if (prev == this)					// Only 1 entry
					list.head = null;
				else
				{									// Ungraft it from the list
					if (prev.next != this)
					{
						//Log.WriteLine("Timer.Cancel", Log.Severity.Info,
						//	"Trying to cancel timer not on {0}.", list.ToString());
						return;
					}
					else
					{
						next.prev = prev;
						prev.next = next;
						if (this == list.head)	// reset list.head only if we removed 1st entry
							list.head = next;
					}
				}
				list.Count--;						// prior to dispatching
				//interval = 0;
				AddToFree();
			}
		}

		/// <summary>
		/// Dispatch a timer
		/// </summary>
		/// <returns></returns>
		internal Timer Dispatch()
		{
			//			SimulationConsole.WriteLine("Disp {0} h {1} h.p {2} h.p.n {3} h.n{4} h.n.p {5}",
			//				tag, list.head.tag, list.head.prev.tag,
			//				list.head.prev.next.tag, list.head.next.tag, list.head.next.prev.tag);
			if (occurance == TimerOccurance.Periodic)
			{
				callback();
				return occurance == TimerOccurance.OneShot ? null : this;
			}
			else
			{
				list.Count--;
				AddToFree();
				callback();
			}
			return null;
		}

		static Timer GetTimer(double interval, Callback callback)
		{
			Timer timer;
			lock (freeTimers)
			{
				if (freeTimers.Count > 0)
				{
					freeTimers.Count--;
					timer = freeTimers.head;
					//					SimulationConsole.WriteLine("{0} from free({1})", timer.tag,freeTimers.Count);
					freeTimers.head = timer.next;
					if (freeTimers.Count == 0)
						freeTimers.head = null;


					timer.interval = interval;
					timer.callback = callback;
				}
				else
					timer = new Timer(interval, callback);
			}
			return timer;
		}

		/// <summary>
		/// Inhibit the wall timer
		/// </summary>
		public static bool InhibitWallTimer
		{
			get { return inhibitWallTimer; }
			set
			{
				//if (value)
				//	wallTimer.Change(0, Timeout.Infinite);
				//else
				//	wallTimer.Change(0, wallTimers.IntervalMs);
				inhibitWallTimer = value;
			}
		} static bool inhibitWallTimer;

		/// <summary>
		/// Reschedule the list of timers starting at head, comparing them to referenceTime
		/// </summary>
		/// <param name="head"></param>
		/// <param name="referenceTime"></param>
		/// <returns></returns>
		internal Framework.Timer ReSchedule(ref Timer head, double referenceTime)
		{
			double time = referenceTime + interval;
			Framework.Timer timer;

			expirationTime = time;

			if (head == null)
			{				/* Make this the first timeout entry */
				head = this;
				next = this;
				prev = this;
			}
			else
			{
				// 	Search for first timeout that expires AFTER us
				timer = head;
				int i = 0;	// let's make a counter to prevent an infinite loop.
				do
				{
					i++;
					if (timer.expirationTime > time) break;
					timer = timer.next;
				} while (timer != head && i < int.MaxValue - 1);
				//	Found the slot. Now graft the timeout in place
				this.next = timer;
				this.prev = timer.prev;
				timer.prev.next = this;
				timer.prev = this;
				//	Reset head only if timeout added to front
				if (time < head.expirationTime)
					head = this;
			}

			//			SimulationConsole.WriteLine("resched {0} at {1} in {2} ",
			//					tag,referenceTime,interval);
			//			timer = head;
			//			do 
			//			{
			//				SimulationConsole.WriteLine("{0} {1}",timer.tag,timer.expirationTime );
			//				timer = timer.next;
			//			} while ( timer != head );

			return this;
		}
		/// <summary>
		/// Access the timer's interval
		/// </summary>
		[Description("The time delay until the timer expires and calls its callback")]
		public double Interval
		{
			get { return interval; }
			set
			{
				interval = value;
				list.IntervalMs = (int)(value * 1000);
			}
		}
		internal double interval;

		/// <summary>
		/// Create a new timer based on simulation elapsed time. Will only create a one-shot timer that expires at the specified time.
		/// </summary>
		/// <param name="elapsedTime">Elapsed simulation time at which the timer expires.</param>
		/// <param name="callback">Callback delegate called when the timer expires.</param>
		/// <returns></returns>
		public static Framework.Timer SetSimulationTimerAtAbsoluteTime(double elapsedTime, Callback callback)
		{
			return CreateTimer(TimerOccurance.OneShot, Math.Abs(elapsedTime), callback, 0);
		}

		/// <summary>
		/// Create a new timer based on simulation elapsed time
		/// </summary>
		/// <param name="interval">Time interval [seconds] to wait prior until the timer expires.
		///  Positive value is periodic, negative is a one shot.</param>
		/// <param name="callback">Callback delegate called when the timer expires.</param>
		/// <returns>The Timer so it can be cancelled.</returns>
		public static Framework.Timer SetSimulationTimer(double interval, Callback callback)
		{
			if (interval > 0)
				return CreateTimer(TimerOccurance.Periodic, interval, callback, Clock.Current.ElapsedSeconds);
			else
				return CreateTimer(TimerOccurance.OneShot, Math.Abs(interval), callback, Clock.Current.ElapsedSeconds);
		}

		/// <summary>
		/// Create a new timer based on simulation elapsed time
		/// </summary>
		/// <param name="occurance"><see cref="TimerOccurance"/> How often the timer occurs.</param>
		/// <param name="interval">Time interval [seconds] to wait prior until the timer expires.
		///  Positive value is periodic, negative is a one shot.</param>
		/// <param name="callback">Callback delegate called when the timer expires.</param>
		/// <returns>The Timer so it can be cancelled.</returns>
		public static Framework.Timer SetSimulationTimer(TimerOccurance occurance, double interval, Callback callback)
		{
			return CreateTimer(occurance, Math.Abs(interval), callback, Clock.Current.ElapsedSeconds);
		}

		private static Framework.Timer CreateTimer(TimerOccurance occurance, double interval, Callback callback, double startTime)
		{
			Framework.Timer timer = Clock.simTimers.Add(GetTimer(interval, callback), startTime);
			timer.occurance = occurance;
			//if ( occurance == TimerOccurance.OneShot )
			//    timer.interval = 0;
			//			SimulationConsole.WriteLine("Setting {0} {1} p {2} p.n {3} n {4} n.p {5}",
			//				timer.tag, interval, timer.prev.tag, timer.prev.next.tag,timer.next.tag, timer.next.prev.tag);
			return timer;
		}

		/// <summary>
		/// Create a new timer based on wall clock time
		/// </summary>
		/// <param name="interval">Time interval [seconds] to wait prior until the timer expires.
		///  Positive value is periodic, negative is a one shot.</param>
		/// <param name="callback">Callback delegate called when the timer expires.</param>
		/// <returns>The Timer so it can be cancelled.</returns>
		public static Framework.Timer SetWallTimer(double interval, Callback callback)
		{
			if (interval > 0)
				return CreateWallTimer(TimerOccurance.Periodic, interval, callback);
			else
				return CreateWallTimer(TimerOccurance.OneShot, Math.Abs(interval), callback);
		}

		/// <summary>
		/// Create a new timer based on wall clock time
		/// </summary>
		/// <param name="occurance"><see cref="TimerOccurance"/> How often the timer occurs.</param>
		/// <param name="interval">Time interval [seconds] to wait prior until the timer expires.
		///  Positive value is periodic, negative is a one shot.</param>
		/// <param name="callback">Callback delegate called when the timer expires.</param>
		/// <returns>The Timer so it can be cancelled.</returns>
		public static Framework.Timer SetWallTimer(TimerOccurance occurance, double interval, Callback callback)
		{
			return CreateWallTimer(occurance, Math.Abs(interval), callback);
		}

		private static Timer CreateWallTimer(TimerOccurance occurance, double interval, Callback callback)
		{
			int intervalMs = (int)(interval * 1000);
			if (wallTimers.IntervalMs == Timeout.Infinite)
			{
				wallTimers.SetInterval(intervalMs);
				//wallTimer = new System.Threading.Timer(new TimerCallback(WallTick), null, 0, intervalMs);
				//Executive.Exit += new EventHandler(Exiting);
			}
			Timer timer = GetTimer(interval, callback);
			timer.occurance = occurance;
			wallTimers.Add(timer, mWallSeconds);
			//if ( occurance == TimerOccurance.OneShot )
			//    timer.interval = 0;
			return timer;
		}

		//internal static System.Threading.Timer wallTimer;
		/// <summary>
		/// Dispatches the wall clock timers
		/// </summary>
		/// <param name="wallSeconds"></param>
		public static void WallTick(double wallSeconds)
		{
			wallTimers.Dispatch(wallSeconds);
			mWallSeconds = wallSeconds;
		} static double mWallSeconds;

		/// <summary>
		/// Reset or clear the wall clock timers
		/// </summary>
		/// <param name="clear"></param>
		internal static void WallReset(bool clear=false)
		{
			if ( clear )
				wallTimers.Clear();
			else
				wallTimers.Reschedule(0,"Reset");
		}

		/// <summary>
		/// Clean up the wall timers when exiting
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		static void Exiting(object sender, EventArgs e)
		{
			//wallTimer.Dispose();
		}
	}
}
