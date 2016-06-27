using System;
using System.ComponentModel;

using Aspire.Utilities;

namespace Aspire.Framework
{
	/// <summary>
	/// Schedule models to run
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class Scheduler
	{
		Scenario mScenario;
		long mFrameCount;

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="scenario"></param>
		public Scheduler(Scenario scenario)
		{
			mScenario = scenario;
		}

		/// <summary>
		/// Access the dispatch enable flag
		/// </summary>
		public bool Dispatch
		{
			get { return mDispatch; }
			set
			{
				mDispatch = value;
				mScenario.Clock.Dispatching = value;
			}
		} bool mDispatch;

		/// <summary>
		/// How many frames have occured after start
		/// </summary>
		public long FrameCount { get { return mFrameCount; } internal set { mFrameCount = value; } }

		/// <summary>
		/// Event that is raised when finished dispatching all models
		/// </summary>
		public event EventHandler FrameEnd;

		/// <summary>
		/// Reset internal state
		/// </summary>
		public void Initialize()
		{
			mFrameCount = 0;
		}

		/// <summary>
		/// Reset to 0 elapsedTime
		/// </summary>
		public void Reset(bool clear=false)
		{
			Timer.WallReset(clear);
		}

		/// <summary>
		/// Perform one frame on a clock tick
		/// </summary>
		/// <param name="wallSeconds"></param>
		public void Tick(double wallSeconds)
		{
			Timer.WallTick(wallSeconds);
			if (mDispatch)
			{
				mFrameCount++;
				mScenario.Execute();
				FrameEnd(this, EventArgs.Empty);
			}
		}

		internal void Tick()
		{
			mFrameCount++;
			mScenario.Execute();
			FrameEnd(this, EventArgs.Empty);
		}

		/// <summary>
		/// 
		/// </summary>
		public int TimerPeriod
		{
			get { return mTimerPeriod; }
			set
			{
				mTimerPeriod = value;
				if (TimerPeriodChanged != null)
					TimerPeriodChanged(null, EventArgs.Empty);
			}
		} int mTimerPeriod;
		/// <summary>
		/// 
		/// </summary>
		public event EventHandler TimerPeriodChanged;
	}
}
