using System;
using System.ComponentModel;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Core;
using Aspire.Core.Messaging;
using Aspire.Core.xTEDS;

namespace Aspire.CoreModels
{
	public class AspireClock : Application, IKnownMarshaler, ITimeFormatter
	{
		const string Category = "Clock";
		long frameCount = -1;
		Clock mClock;
		DateTime gpsEpoch = new DateTime(1980, 1, 6, 0, 0, 0);
		Scheduler mScheduler;
		internal uint gpsOffset;
		uint lastSeconds, seconds, subSeconds;
		int leapSecondsGpsEpoch;
		TimeSyncProtocol timeSync = null;

		public AspireClock() : base("{746D5D6D-231D-4958-9B68-2BD5A2C2B729}")
		{
			DontRegister = true;
			theClock = this;
			TimeDisplay.AddFormatter(this);
		}

		public static AspireClock The
		{
			get
			{
				if (theClock == null)
					theClock = new AspireClock();
				return theClock;
			}
		} static AspireClock theClock;

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			mClock = scenario.Clock;
			mClock.LeapSecondsChanged += mClock_LeapSecondsChanged;
			mClock.SecondsChanged += mClock_SecondsChanged;
			mScheduler = scenario.Executive.Scheduler;

			base.Discover(scenario);

			//timeSync = new TimeSyncProtocol(this);
			//timeSync.Clock = mClock;
			//TimeAtTone = TimeAtTone; // syncronize with timeSync;
			//AddProtocol(ProtocolId.Aspire, timeSync);
		}

		void mClock_LeapSecondsChanged(object sender, EventArgs e)
		{
			leapSecondsGpsEpoch = ClockData.GetLeapSeconds(gpsEpoch);
		}

		public override void Initialize()
		{
			TimeSpan ts = mClock.DateTime.Subtract(gpsEpoch);
			gpsOffset = (uint)(ts.TotalSeconds + GpsLeapSeconds);
			seconds = (uint)GpsSeconds;
			subSeconds = (uint)mClock.MicroSeconds;
		}

		public override void Execute()
		{
			CheckTime();
			if (TimeAtTone && seconds > lastSeconds && mControlProtocol != null)
			{
				// should send average us delay read in from hardware instead of 0 us
				mControlProtocol.SendTimeAtTheTone(BroadcastAddrFromAddr(mControlProtocol.LocalManagerAddress), seconds + 1, 0, (float)mClock.TimeRatio);
				tatsSent++;
			}
			lastSeconds = seconds;
		}

		#endregion

		public void CheckTime()
		{
			if (mScheduler != null && mScheduler.FrameCount != frameCount)
			{
				frameCount = mScheduler.FrameCount;
				seconds = (uint)GpsSeconds;
				subSeconds = (uint)mClock.MicroSeconds;
			}
		}

		/// <summary>
		/// Obtain the current time as seconds after the GPS epoch, Jan 6, 1980, [sec]
		/// </summary>
		[Category(Category)]
		[Blackboard(Description = "Time as seconds after the GPS epoch time of Jan 6, 1980 00:00:00", Units = "sec")]
		public uint GpsSeconds
		{
			get { return gpsOffset + mClock.Seconds; }
		}

		/// <summary>
		/// Gets the number of leap seconds after Jan 6, 1980
		/// </summary>
		[Category(Category)]
		public int GpsLeapSeconds
		{
			get { return mClock.LeapSeconds - leapSecondsGpsEpoch; }
		}

		uint ppssSent;
		[XmlIgnore]
		public uint PpssSent { get { return ppssSent; } set { ppssSent = value; } }

		[XmlAttribute,DefaultValue(false)]
		public bool SoftPps
		{
			get { return mSoftPps; }
			set { mSoftPps = value; if ( timeSync != null ) timeSync.Synchronize = !mSoftPps && !mTimeAtTone; }
		} bool mSoftPps;

		uint tatsSent;
		[XmlIgnore]
		public uint TaTsSent { get { return tatsSent; } set { tatsSent = value; } }

		[XmlAttribute,DefaultValue(false)]
		public bool TimeAtTone
		{
			get { return mTimeAtTone; }
			set { mTimeAtTone = value; if (timeSync != null) timeSync.Synchronize = !mSoftPps && !mTimeAtTone; }
		} bool mTimeAtTone;


		Address BroadcastAddrFromAddr(Address address)
		{
			Address bcast = address.Clone();
			long addr;
			int port = bcast.GetAddressPort(out addr);
			bcast.SetAddressPort(addr | 0xFF000000, port);
			return bcast;
		}

		void mClock_SecondsChanged(object sender, EventArgs e)
		{
			var p = mControlProtocol as NativeProtocol;
			if (SoftPps && p != null)
			{
				p.SendSoftPps(BroadcastAddrFromAddr(mControlProtocol.LocalManagerAddress));
				ppssSent++;
			}
		}

		#region IKnownMarshaler Members

		VariableMarshaler mTimeMarshaler, mSubSMarshaler;

		public uint Seconds { get { return seconds; } }
		public uint SubSeconds { get { return subSeconds; } }

		public VariableMarshaler KnownMarshaler(IVariable iVariable)
		{
			switch (iVariable.Name)
			{
				case "Time":
					if (mTimeMarshaler == null)
						mTimeMarshaler = new VariableMarshaler(iVariable, this, "seconds");
					return mTimeMarshaler;
				case "SubS":
					if (mSubSMarshaler == null)
						mSubSMarshaler = new VariableMarshaler(iVariable, this, "subSeconds");
					return mSubSMarshaler;
			}
			return null;
		}

		#endregion

		#region ITimeFormatter Members

		/// <summary>
		/// Selects one of many formats that the formatter knows
		/// </summary>
		public string Format { set {  } }
		/// <summary>
		/// Format the time as mission time
		/// </summary>
		/// <param name="clock"></param>
		/// <returns></returns>
		public string FormatTime(Clock clock)
		{
			return string.Format("{0}.{1:D3}", gpsOffset + clock.Seconds, clock.MilliSeconds);
		}

		/// <summary>
		/// Displayed name
		/// </summary>
		public string[] Names { get { return new string[] { "GPS" }; } }

		#endregion



		#region TimeSync protocol

		public class TimeSyncProtocol : ApplicationProtocol
		{
			uint toneGpsSeconds;
			bool synchronize, requestSync;
			AspireClock mAspireClock;

			public TimeSyncProtocol(AspireClock aspireClock)
				: base(aspireClock, aspireClock.Transport,
				new Core.Utilities.MarshaledString(aspireClock.XtedsText),aspireClock.CompUid)
			{
				mAspireClock = aspireClock;
			}

			protected override void OnTimeAtTheTone(Message message, UInt32 ToneSec, UInt32 ToneSubSec, float TimeRatio)
			{
				toneGpsSeconds = ToneSec;
			}

			internal Clock Clock
			{
				set { mClock = value; Synchronize = requestSync; }
			} Clock mClock;

			protected override void OnSoftPps(Message message)
			{
				if (synchronize)
				{
					uint gpsSeconds = mClock.Seconds;
					if (toneGpsSeconds > gpsSeconds + 1 || toneGpsSeconds < gpsSeconds - 1)
						mClock.JamSync(toneGpsSeconds - mAspireClock.gpsOffset);
				}
			}

			internal bool Synchronize
			{
				set
				{
					requestSync = value;
					synchronize = mClock != null && requestSync;
				}
			}
		}

		#endregion
	}
}
