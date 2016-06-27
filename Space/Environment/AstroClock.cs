using System;
using System.ComponentModel;

using Aspire.Framework;
using Aspire.Primitives;
//using Aspire.Utilities;

namespace Aspire.Space
{
    public class AstroClock : Model
	{
		const string category = "AstroClock";
		const double refJulianEpochDate = Constant.JulianDateEpoch2000;
		// should be coming from ClockData.GregorianJulianDifference.DifferenceDays and a function of the JulianDate
		const int gregorianJulianDifferenceDays = -13;
		const double TAItoTT = 32.184;

		internal static DateTime julianRef;
		static System.Globalization.JulianCalendar calendar = new System.Globalization.JulianCalendar();

		Clock clock;

		double gmst, JD, julianEpochDate;
		double UTCtoTTday, UTCtoTTcentury, dUT1;
		int leapSeconds;

		AstroClock()
		{
			julianRef = new DateTime(2000, 1, 1, 12, 0, 0, 0, calendar);

			// NOTE: the julian 'calender' in the above line causes days to be counted 
			// as originally defined, rather than by the Gregorian calender.  The 
			// difference is in when leap years occur.

			julianRef = julianRef.AddDays(gregorianJulianDifferenceDays);
		}

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			clock = scenario.Clock;
			clock.LeapSecondsChanged += clock_LeapSecondsChanged;

			base.Discover(scenario);
		}

		public override void Initialize()
		{
			Execute();
		}

		public override void Execute()
		{
			var dateTime = clock.DateTime.AddDays(gregorianJulianDifferenceDays);
			JD = dateTime.Subtract(julianRef).TotalDays + Constant.JulianDateEpoch2000; // Need to do JD arithmetic in MJD

			// Julian Epoch date
			julianEpochDate = JD - refJulianEpochDate;
			// Jean Meeus, "Astronomical Algorithms", Willmann-Bell, Inc., 1991
			double Tu = julianEpochDate / Constant.JulianDayPerCentury;
			gmst = 280.46061837 + 360.98564736629 * julianEpochDate
				+ Tu * Tu * (0.000387933 - Tu / 38710000.0);
			gmst = Maths.Mod(gmst, 360.0);
			gmst *= Constant.SecPerDay / 360.0;

			base.Execute();
		}

		void clock_LeapSecondsChanged(object sender, EventArgs e)
		{
			leapSeconds = clock.LeapSeconds;
			UTCtoTTday = (leapSeconds + TAItoTT) * Constant.DayPerSec;
			UTCtoTTcentury = UTCtoTTday / Constant.DayPerCentury;

			dUT1 = ClockData.DeltaUT1(clock.DateTime);
		}

		#endregion

		#region Properties

		public Clock Clock { get { return clock; } }

		public double GreenwichHourAngle
		{
			get { return gmst * Constant.TwoPi / Constant.SecPerDay; }
		}

		/// <summary>
		/// Access UT1 - UTC.
		/// </summary>
		[Category(category)]
		public double DeltaUT1 { get { return dUT1; } }

		/// <summary>
		/// 
		/// </summary>
		[Category(category)]
		public double JulianCenturies { get { return julianEpochDate / Constant.JulianDayPerCentury; } }

		/// <summary>
		/// 
		/// </summary>
		[Category(category)]
		public double J2000Century { get { return (JD - Constant.ModifiedJulianTare - 51544.5) / Constant.JulianDayPerCentury; } }

		public static double J2000CenturyOf(double mjd)
		{
			return (mjd - 51544.5) / Constant.JulianDayPerCentury;
		}

		/// <summary>
		/// Access the time in days as a Julian date UT1.
		/// </summary>
		[Category(category)]
		public double JulianDateUT1 { get { return JD + dUT1 * Constant.DayPerSec; } }

		/// <summary>
		/// Access the time in days as a Julian date TAI.
		/// </summary>
		[Category(category)]
		public double JulianDateTAI { get { return JD + leapSeconds * Constant.DayPerSec; } }

		/// <summary>
		/// Access the time in days as a Julian date TT.
		/// </summary>
		[Category(category)]
		public double JulianDateTT { get { return JD + UTCtoTTday; } }

		/// <summary>
		/// Access the time in days as a Julian date UTC.
		/// </summary>
		[Category(category)]
		public double JulianDate { get { return JD; } }

		/// <summary>
		/// 
		/// </summary>
		[Category(category)]
		public double J2000CenturyTT { get { return J2000Century + UTCtoTTcentury; } }

		/// <summary>
		/// 
		/// </summary>
		public double ModifiedJulianDate
		{
			get { return JD - Constant.ModifiedJulianTare; } // Need to do JD arithmetic in MJD
		}

		#endregion

		#region Methods

		/// <summary>
		/// Convert a modified julian date to J2000 epoch julian centuries
		/// </summary>
		/// <param name="mjd"></param>
		/// <returns></returns>
		public static double J2000CenturyFromMjd(double mjd) { return (mjd - 51544.5) / Constant.JulianDayPerCentury; }

		#endregion
	}

	public static class AstroClockExtensions
	{
		public static double ToJulianDate(this DateTime date)
		{
			return date.Subtract(AstroClock.julianRef).TotalDays + Constant.JulianDateEpoch2000; // Need to do JD arithmetic in MJD
		}

		public static double ToModifiedJulianDate(this DateTime date)
		{
			return ToJulianDate(date) - Constant.ModifiedJulianTare;
		}

	}
}
