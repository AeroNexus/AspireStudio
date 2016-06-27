using System;
using System.ComponentModel;

using Aspire.Framework;

namespace Aspire.Primitives
{
	public static class Constant
	{
		#region Prefixes

		public const double Kilo = 1000.0;

		#endregion

		#region Math

		public const double HalfPi = Math.PI / 2;
		public const double HalfSqrt2 = Sqrt2 / 2;
		public const double Pi = Math.PI;
		public const double Sqrt2 = 1.4142135623730950488016887242097;
		public const double TwoPi = 2 * Pi;

		#endregion

		#region Time

		/// <summary>Time conversion constant</summary>
		//[Blackboard("Time.DayPerCentury", Units = "day/century")]
		public const double DayPerCentury = DayPerYear * 100;

		/// <summary>Time conversion constant</summary>
		//[Blackboard("Time.DayPerSec", Units = "day/year")]
		public const double DayPerSec = 1 / SecPerDay;

		/// <summary>Time conversion constant</summary>
		//[Blackboard("Time.DayPerYear", Units = "day/year")]
		public const double DayPerYear = 365.2425;

		/// <summary>Time constant</summary>
		//[Blackboard("Time.JulianDateEpoch2000", Units = "days", Description = "Julian day of 2000-Jan-1 12:00:00")]
		public const double JulianDateEpoch2000 = 2451545;

		/// <summary>Time conversion constant</summary>
		//[Blackboard("Time.JulianDayPerCentury", Units = "day/century")]
		public const double JulianDayPerCentury = 36525;

		/// <summary>Time conversion constant</summary>
		//[Blackboard("Time.MinPerHour", Units = "s/hr")]
		public const double MinPerHour = 60;

		/// <summary>Time conversion constant</summary>
		//[Blackboard("Time.MinPerSec", Units = "s/hr")]
		public const double MinPerSec = 1 / SecPerMinute;

		/// <summary>JD = MJD + tare</summary>
		//[Blackboard("Time.MinPerSec", Units = "s/hr")]
		public const double ModifiedJulianTare = 2400000.5;

		/// <summary>Time conversion constant</summary>
		//[Blackboard("Time.SecPerDay", Units = "s/day")]
		public const double SecPerDay = SecPerHour * 24;

		/// <summary>Time conversion constant</summary>
		//[Blackboard("Time.SecPerHour", Units = "s/hr")]
		public const double SecPerHour = SecPerMinute * MinPerHour;

		/// <summary>Time conversion constant</summary>
		//[Blackboard("Time.SecPerMinute", Units = "s/min")]
		public const double SecPerMinute = 60;

		#endregion

		#region Distance

		public const double MperAU = 149597870691;
		/// <summary>Length conversion constant</summary>
		[Description("Meters per Astronomical unit (mean earth-sun distance), [m]")]

		#endregion

		#region Angular Displacement

		public const double DegPerRad = 180 / Pi;
		public const double RadPerDeg = Pi / 180;

		#endregion

		#region Mass

		/// <summary>Universal gravitational constant[m3/kg/s2]
		/// "IUPAC Gold Book
		/// </summary>
		//[Blackboard("Mass.Gravitational", Units = "m*m*m/kg/s/s")]
		public const double Gravitational = 6.6725985e-11;

		#endregion

		#region Earth
		/// <summary>
		/// Earth flattening constant
		/// f = (a-b)/a
		/// a = Earth's semi-major axis, or the radius of the equator
		/// b = Earth's semi-minor axis, or the radius at the poles
		/// WGS-84 defines a and f, while b is derived
		/// </summary>
		//[Blackboard("Earth.Flattening")]
		public const double EarthFlattening = 1 / 298.257223563;

		/// <summary>
		/// First Earth Eccentricity Squared
		/// e2 = 2f - f^2
		/// </summary>
		//[Blackboard("Earth.Eccentricity2")]
		public const double EarthEccentricity2 = EarthFlattening * (2 - EarthFlattening);

		/// <summary>
		/// Earth equatorial radius [m]
		/// Semi-major Earth axis
		/// WGS-84
		/// </summary>
		public const double EarthEquatorialRadius = 6378137;

		/// <summary>Earth gravitational constant (m*Gm) [m3/s2]</summary>
		//[Blackboard("Earth.GravityConstant", Units = "m^3/s^2")]
		public const double EarthGravityConstant = 3.986004418e14;

		/// <summary>Earth mass [kg]</summary>
		//[Blackboard("Earth.Mass", Units = "kg")]
		public const double EarthMass = 5.973691385747246724672467e24;

		/// <summary>
		/// Earth polar radius [m]
		/// Semi-minor Earth axis, derived from semi-major axis and flattening factor constants
		/// </summary>
		//[Blackboard("Earth.PolarRadius", Units = "m")]
		public const double EarthPolarRadius = EarthRadius * (1.0 - EarthFlattening);

		/// <summary>
		/// Earth equatorial radius [m]
		/// Semi-major Earth axis
		/// WGS-84
		/// </summary>
		//[Blackboard("Earth.Radius", Units = "m")]
		public const double EarthRadius = EarthEquatorialRadius;

		/// <summary>Earth rotational rate [rad/s]</summary>
		//[Blackboard("Earth.Rate", Units = "rad/sec")]
		public const double EarthRate = TwoPi / (EarthRotationPeriod * 3600);

		/// <summary>Earth rotational period [hr]</summary>
		//[Blackboard("Earth.RotationPeriod", Units = "hr")]
		public const double EarthRotationPeriod = 23.9344719178;

		#endregion

		#region Sun

		/// <summary>Sun constant</summary>
		//[Blackboard("Sun.Mass", Units = "kg")]
		public const double SunMass = 1.98892e30;

		/// <summary>Sun constant</summary>
		//[Blackboard("Sun.Radius", Units = "m")]
		public const double SunRadius = 695990000;

		/// <summary>Sun constant</summary>
		//[Blackboard("Sun.SolarConstant", Units = "W/m^2")]
		public const double SolarConstant = 1358;

		#endregion

		#region Moon

		/// <summary>Moon's gravitational constant (mass*G) [m3/s2]</summary>
		//[Blackboard("Moon.GravityConstant", Units = "m*m*m/sec/sec")]
		public const double MoonGravityConstant = Gravitational * MoonMass;

		/// <summary>Moon's mass. [kg]</summary>
		//[Blackboard("Moon.Mass", Units = "kg")]
		public const double MoonMass = 7.3477e22;

		/// <summary>Moon's mean rotational rate [rad/s]</summary>
		//[Blackboard("Moon.Rate", Units = "rad/sec")]
		public const double MoonRate = TwoPi / (MoonRotationPeriodDays * 86400);

		/// <summary>Moon's equatorial radius [m]</summary>
		//[Blackboard("Moon.Radius", Units = "m")]
		public const double MoonRadius = 1738140;

		/// <summary>Moon's mean rotational period [days]</summary>
		//[Blackboard("Moon.RotationPeriodDays", Units = "days")]
		public const double MoonRotationPeriodDays = 27.321661;
		// CAUTION: above value is from a non-authoritative site, but much
		//          better than previous gross approximation of 27.3
		// NOTE: this is the mean rate because the rate is assuredly non-constant

		#endregion


	}
}
