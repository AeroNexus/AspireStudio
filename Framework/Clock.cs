using System;
using System.ComponentModel;
using System.Xml.Serialization;

using Aspire.Utilities;

namespace Aspire.Framework
{
	/// <summary>
	/// The fundamental timekeeper
	/// </summary>
	public class Clock : Model, ITimeFormatter
	{
		const string category = "Clock";

		DateTime dateTime, dateToLeap, initialDateTime, dtTare;
		TimeDisplay mTimeDisplay;
		internal static TimerList simTimers = new TimerList("Simulation Timers");
		Scheduler mScheduler;
		int leapSeconds;

		const double SecPerMicroSec = 0.000001;
		const int MicroSecPerSec = 1000000;

		int mMicroSeconds, mStepMicroSec;
		uint mSeconds;
		double mElapsedSeconds, mStepSize, mStartOfYear, mDaysPerYear, etTare;
		bool mInitialTimeIsNow = true;

		/// <summary>
		/// Default constructor
		/// </summary>
		public Clock()
		{
			current = this;
			initialDateTime = DateTime.Now;
			initialDateTime = initialDateTime.AddMilliseconds(1000 - initialDateTime.Millisecond);
			dateTime = initialDateTime;
			StepSize = 0.125;
			Name = "Clock";
			mTimeDisplay = new TimeDisplay(this);
			TimeDisplay.AddFormatter(this);
		}

		#region Model implementation

		/// <summary>
		/// Configure the time display formatters
		/// </summary>
		/// <param name="scenario"></param>
		public override void Discover(Scenario scenario)
		{
			mTimeDisplay.Discover();
			base.Discover(scenario);
			mScheduler = scenario.Executive.Scheduler;
		}

		/// <summary>
		/// Update time by one frame
		/// </summary>
		public override void Execute()
		{
			uint prevSeconds = mSeconds;
			mMicroSeconds = mMicroSeconds + mStepMicroSec;
			if (mMicroSeconds >= MicroSecPerSec)
			{
				uint dsec = (uint)(mMicroSeconds * SecPerMicroSec);
				mSeconds += dsec;
				mMicroSeconds %= MicroSecPerSec;
			}
			else if (mMicroSeconds < 0)
			{
				int dsec = (int)((mMicroSeconds - MicroSecPerSec) * SecPerMicroSec);
				mSeconds = (uint)(mSeconds + dsec);
				mMicroSeconds -= dsec * MicroSecPerSec;
			}
			mElapsedSeconds = mSeconds + mMicroSeconds * SecPerMicroSec;
			dateTime = initialDateTime.AddSeconds(mElapsedSeconds);

			base.Execute();

			simTimers.Dispatch(mElapsedSeconds);
			if (mSeconds != prevSeconds && SecondsChanged != null) SecondsChanged(this, EventArgs.Empty);

			if (realtime && mElapsedSeconds - etTare <= (DateTime.Now - dtTare).TotalSeconds - mStepSize)
			{
				mExtraTicks++;
				bool prevRealtime = realtime;
				realtime = false;
				mScheduler.Tick();
				realtime = prevRealtime;
			}

		}

		/// <summary>
		/// Reset internal state
		/// </summary>
		public override void Initialize()
		{
			mSeconds = 0;
			mMicroSeconds = 0;
			mElapsedSeconds = mSeconds + mMicroSeconds * SecPerMicroSec;
			dateTime = initialDateTime.AddSeconds(mElapsedSeconds);

			double secondOfYear = (((dateTime.DayOfYear - 1) * 24 + dateTime.Hour) * 60 + dateTime.Minute) * 60 + dateTime.Second + dateTime.Millisecond / 1000;
			mStartOfYear = mElapsedSeconds - secondOfYear;

			SetLeapInfo(dateTime);

			base.Initialize();

			mTimeDisplay.Initialize();
			simTimers.Reschedule(mElapsedSeconds, "Clock.Initialize");
		}

		/// <summary>
		/// Scenario is unloading, dispose
		/// </summary>
		public override void Unload()
		{
			base.Unload();
			TimeDisplay.Formatters.Clear();
		}

		#endregion

		internal bool Dispatching
		{
			set
			{
				if (value)
				{
					etTare = mElapsedSeconds;
					dtTare = DateTime.Now;
				}
			}
		}

		/// <summary>
		/// Clear the simulation timer list when unloading
		/// </summary>
		internal void ClearTimers()
		{
			Initialize();
			simTimers.Clear();
		}

		/// <summary>
		/// The current Clock in the Active scenario
		/// </summary>
		public static Clock Current { get { return current; } } static Clock current;

		/// <summary>
		/// .Net DateTime equivalent
		/// </summary>
		[Category(category)]
		public DateTime DateTime { get { return dateTime; } }

		private void DayChanged()
		{
			var date = dateTime;

			if (date.Year == dateToLeap.Year && date.Month == dateToLeap.Month && date.Day == dateToLeap.Day)
				SetLeapInfo(date);
		}

		/// <summary>
		/// Day within a year
		/// </summary>
		[Category(category), Blackboard(Units = "days")]
		public double DayOfyear
		{
			// This is carrying fractional day. If it proves to carry just the integral day, this becomes much simpler.
			get
			{
				double dayOfYear = (mElapsedSeconds - mStartOfYear) / 86400;
				if (dayOfYear >= mDaysPerYear)
				{
					double secondOfYear = (((dateTime.DayOfYear - 1)*24 + dateTime.Hour) * 60 + dateTime.Minute) * 60 + dateTime.Second + dateTime.Millisecond/1000;
					mStartOfYear = mElapsedSeconds - secondOfYear;
					SetLeapInfo(dateTime);

					return (mElapsedSeconds - mStartOfYear) / 86400;
				}
				else
					return dayOfYear;
			}
		}

		/// <summary>
		/// Format used for the Time toolbar
		/// </summary>
		[Category(category), XmlAttribute("displayFormat")]
		public string DisplayFormat
		{
			get { return mTimeDisplay.Format; }
			set { mTimeDisplay.Format = value; }
		}

		/// <summary>
		/// Elapsed seconds. Approximation
		/// </summary>
		[Category(category), Blackboard(Units = "sec")]
		public double ElapsedSeconds { get { return mElapsedSeconds; } }

		/// <summary>
		/// How many extra ticks have been taken to keep time synced
		/// </summary>
		[Category(category), Blackboard,XmlIgnore]
		public int ExtraTicks { get { return mExtraTicks; } } int mExtraTicks;

		/// <summary>
		/// The initial time. If not specified for a scenario, Now is used
		/// </summary>
		[Category(category), XmlAttribute("initialTime"),DefaultValue("")]
		public string InitialTime
		{
			get
			{
				if (IsSaving && mInitialTimeIsNow) return string.Empty;
				return initialDateTime.ToString(TimeDisplay.UtcFormat); }
			set
			{
				dateTime = initialDateTime = TimeDisplay.Parse(value);
				mInitialTimeIsNow = false;
			}
		}

		/// <summary>
		/// Allow an external time source to sync us
		/// </summary>
		/// <param name="seconds"></param>
		public void JamSync(uint seconds)
		{
			mSeconds = seconds;
			mMicroSeconds = 0;// microSeconds;
		}

		/// <summary>
		/// Gets the number of leap seconds
		/// </summary>
		[Category(category)]
		public int LeapSeconds { get { return leapSeconds; } }

		/// <summary>
		/// Event raised when leap seconds changes. Occurs at midnight of the new month.
		/// </summary>
		public event EventHandler LeapSecondsChanged;

		/// <summary>
		/// Elapsed time micro-seconds within a second. Exact.
		/// </summary>
		[Category(category), Blackboard(Units = "microsec")]
		public int MicroSeconds { get { return mMicroSeconds; } }

		/// <summary>
		/// Elapsed time milli-seconds within a second. Exact.
		/// </summary>
		[Category(category), Blackboard(Units = "millisec")]
		public int MilliSeconds { get { return mMicroSeconds/1000; } }

		/// <summary>
		/// Correction required to synchronize to wall clock
		/// </summary>
		[Category(category), Blackboard(Units = "%"),XmlIgnore]
		public double RealtimeCorrection
		{
			get
			{
				rtCorr += mExtraTicks < 4 ? 0 : 0.04 * (mExtraTicks * 100 * mStepSize / (mElapsedSeconds - etTare) - rtCorr);
				return rtCorr;
			}
		} double rtCorr = 3.6;

		/// <summary>
		/// Elapsed seconds. Exact.
		/// </summary>
		[Category(category), Blackboard(Units = "sec")]
		public uint Seconds { get { return mSeconds; } }

		/// <summary>
		/// Fractional day as seconds.
		/// </summary>
		[Category(category), Blackboard(Units = "sec")]
		public float SecondsSinceMidnight { get { return (dateTime.Hour * 60 + dateTime.Minute) * 60 + dateTime.Second; } }

		/// <summary>
		/// Event raised when seconds changes. Occurs at the top of the second
		/// </summary>
		public event EventHandler SecondsChanged;

		private void SetLeapInfo(DateTime dateTime)
		{
			if (DateTime.IsLeapYear(dateTime.Year))
				mDaysPerYear = 366;
			else
				mDaysPerYear = 365;
			dateToLeap = ClockData.GetLeapDate(dateTime);
			leapSeconds = 34; ClockData.GetLeapSeconds(dateTime);
			if (LeapSecondsChanged != null)
				LeapSecondsChanged(this, EventArgs.Empty);
		}

		/// <summary>
		/// Amount of time in a major frame. Approximation
		/// </summary>
		[Category(category), XmlAttribute("stepSize"),DefaultValue(0.125)]
		public double StepSize
		{
			get { return mStepSize; }
			set
			{
				mStepSize = value;
				mStepMicroSec = (int)(mStepSize / SecPerMicroSec);
				if ( mScheduler != null )
					mScheduler.TimerPeriod = mStepMicroSec;
			}
		}

		/// <summary>
		/// Number of milliseconds in a major frame. Approximation
		/// </summary>
		[Category(category)]
		public int StepSizeMilliSeconds { get { return mStepMicroSec / 1000; } }

		/// <summary>
		/// Fractional seconds
		/// </summary>
		[Category(category), Blackboard(Units = "sec")]
		public double Subseconds
		{
			get
			{
				return mMicroSeconds * 0.000001;
			}
		}

		/// <summary>
		/// The TimeDisplay to show on the Time toolbar
		/// </summary>
		[Category(category), Browsable(false)]
		public TimeDisplay TimeDisplay { get { return mTimeDisplay; } }

		/// <summary>
		/// Ratio of simulated time to wall clock time. Command and filtered actual value
		/// </summary>
		[Category(category), XmlIgnore]
		public double TimeRatio
		{
			get { return mTimeRatio; }
			set { mTimeRatioCmd = value; realtime = (value == 1.0); mTimeRatio = mTimeRatioCmd; }
		} double mTimeRatio = 1.0, mTimeRatioCmd;
		bool realtime = true;

		#region ITimeFormatter Members

		bool formatUtc;

		/// <summary>
		/// Selects one of many formats that the formatter knows
		/// </summary>
		public string Format { set { formatUtc = value == "UTC"; } }
		/// <summary>
		/// Format the time as mission time
		/// </summary>
		/// <param name="clock"></param>
		/// <returns></returns>
		public string FormatTime(Clock clock)
		{
			if ( formatUtc )
				return clock.DateTime.ToString(TimeDisplay.UtcFormat);
			else
				return string.Format("{0:F3}", clock.ElapsedSeconds);
		}

		/// <summary>
		/// Displayed name
		/// </summary>
		[Browsable(false)]
		public string[] Names { get { return new string[] { "ElapsedSeconds", "UTC" }; } }

		#endregion

	}

	/// <summary>
	/// Leap seconds and Georgian calendar adjustments
	/// </summary>
	public class ClockData
	{
		static ClockData clockData = new ClockData();

		LeapSecond[] leapSeconds = new LeapSecond[] {
		new LeapSecond(10,"1969/12/31",+0.0040385),
		new LeapSecond(1,"1972/06/30",-0.6349935),
		new LeapSecond(1,"1972/12/31",-0.1865687),
		new LeapSecond(1,"1973/12/31",-0.2978324),
		new LeapSecond(1,"1974/12/31",-0.2891688),
		new LeapSecond(1,"1975/12/31",-0.2717695),
		new LeapSecond(1,"1976/12/31",-0.3334846),
		new LeapSecond(1,"1977/12/31",-0.3474531),
		new LeapSecond(1,"1978/12/31",-0.3984455),
		new LeapSecond(1,"1979/12/31",-0.3526975),
		new LeapSecond(1,"1981/06/30",-0.6277815),
		new LeapSecond(1,"1982/06/30",-0.3891197),
		new LeapSecond(1,"1983/06/30",-0.2482162),
		new LeapSecond(1,"1985/06/30",-0.4507721),
		new LeapSecond(1,"1987/12/31",-0.6342585),
		new LeapSecond(1,"1989/12/31",-0.6694208),
		new LeapSecond(1,"1990/12/31",-0.3794463),
		new LeapSecond(1,"1992/06/30",-0.5557123),
		new LeapSecond(1,"1993/06/30",-0.3993301),
		new LeapSecond(1,"1994/06/30",-0.2160316),
		new LeapSecond(1,"1995/12/31",-0.4426184),
		new LeapSecond(1,"1997/06/30",-0.4721535),
		new LeapSecond(1,"1998/12/31",-0.2823394),
		new LeapSecond(1,"2005/12/31",-0.6611333),
		new LeapSecond(1,"2008/12/31",-0.5918700),
		new LeapSecond(1,"2012/06/30",-0.5868238),
// estimated as of 2012/11/11
		new LeapSecond(1,"2015/06/30",-0.5674752),
		};
		
		/// <summary>
		/// Calculate UT1 - UTC for this date
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public static double DeltaUT1(DateTime dateTime)
		{
			double dUT1 = 0;

			if (dateTime < clockData.leapSeconds[clockData.leapSeconds.Length - 1].date)
			{
				for (int i = clockData.leapSeconds.Length - 1; i > 0; i--)
				{
					var ls0 = clockData.leapSeconds[i - 1];
					var ls1 = clockData.leapSeconds[i];
					if (ls0.date < dateTime && dateTime <= ls1.date)
					{
						double prevSlope = 0;
						if (i > 1)
						{
							var ls00 = clockData.leapSeconds[i - 2];
							prevSlope = (ls00.dUT1 - (ls00.dUT1 + 1)) / (ls0.date - ls00.date).TotalDays; //[s/day]
						}
						double dUT1_0 = ls0.dUT1 + prevSlope + 1;
						double slope = (ls1.dUT1 - dUT1_0) / (ls1.date - ls0.date).TotalDays;
						dUT1 = dUT1_0 + slope * (dateTime - ls0.date).TotalDays;
						break;
					}
				}
			}
			return dUT1;
		}

		/// <summary>
		/// Find a leap date immediately following a starting data
		/// </summary>
		/// <param name="startDate"></param>
		/// <returns></returns>
		public static DateTime GetLeapDate(DateTime startDate)
		{
			if (IsValid)
			{
				foreach (var ls in clockData.leapSeconds)
				{
					if (ls.date.CompareTo(startDate) > 0)
						return ls.date;
				}
			}
			return DateTime.MaxValue;
		}

		/// <summary>
		/// Get the accumulated leap seconds following a starting date
		/// </summary>
		/// <param name="startDate"></param>
		/// <returns></returns>
		public static int GetLeapSeconds(DateTime startDate)
		{
			int accumulatedSeconds = 0;
			if (IsValid)
			{
				foreach (var ls in clockData.leapSeconds)
				{
					if (ls.date.CompareTo(startDate) > 0)
						return accumulatedSeconds;
					accumulatedSeconds += ls.value;
				}
			}
			return accumulatedSeconds;
		}

		/// <summary>
		/// Used when loaded from a file, indicationd a successful parse
		/// </summary>
		public static bool IsValid { get { return true; } }

		/// <summary>
		/// Leap second entry
		/// </summary>
		class LeapSecond
		{
			internal int value;
			internal DateTime date;
			internal double dUT1;
			internal LeapSecond(int value, string date,double dUT1)
			{
				this.value = value;
				this.date = DateTime.Parse(date); // End of month
				this.date += new TimeSpan(1, 0, 0, 0);
				this.dUT1 = dUT1;
			}
		}
	}
}
