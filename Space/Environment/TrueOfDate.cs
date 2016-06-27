//using System;

using Aspire.Primitives;

namespace Aspire.Space
{

	class TrueOfDate : IReferenceFrameModel
	{
		internal const double eclipticAngle0 = 23.4392911111111;

		public IReferenceFrameModel Clone()
		{
			var clone = new TrueOfDate();
			return clone;
		}

		AstroClock clock;
		public AstroClock Clock { set { clock = value; } }

		/// <summary>
		/// Mean obliquity of the ecliptic, Meeus, p147, 22.2, Astronimical Almanac 1984 
		/// </summary>
		/// <param name="TT"></param>
		/// <returns></returns>
		public double EclipticAngle(double TT)
		{
			return eclipticAngle0 - TT * (0.013004167 + TT * (1.6389e-7 - 5.0361e-7 * TT));
		}
		/// <summary>
		/// Mean obliquity of the ecliptic, Meeus, p147, 22.2, Astronimical Almanac 1984 
		/// </summary>
		public double EclipticAngleDeg
		{
			get
			{
				return EclipticAngle(clock.J2000CenturyTT);
			}
		}
		/// <summary>
		/// Calculate the inertial => ECEF transform DCM
		/// </summary>
		/// <param name="dcm">The DCM to rotate</param>
		/// <returns>The dcm</returns>
		public Dcm FromInertial(Dcm dcm)
		{
			return dcm.RotationFrom(3, clock.GreenwichHourAngle);
		}
		public void Initialize() { }
	}
}
