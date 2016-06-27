using System;
using System.ComponentModel;

using Aspire.Utilities;

namespace Aspire.Primitives
{
	/// <summary>
	/// Describes an orbit with the classical Keplerian elements
	/// </summary>
	public static class Orbit
    {
		// two constants used in MeanAnomalyToEccentricAnomaly
		const double LowLimit = 45 * Constant.RadPerDeg;
		const double HighLimit = Constant.TwoPi - LowLimit;
		static Vector3 unitPos = new Vector3();
		static Vector3 unitVel = new Vector3();

		static Vector3 negNormal = new Vector3();
		static Vector3 forward = new Vector3();

		/// <summary>
		/// Convert Eccentric Anomaly to Mean Anomaly
		/// </summary>
		/// <param name="eccentricAnomaly">Eccentric Anomaly (radians)</param>
		/// <param name="eccentricity">eccentricity</param>
		/// <returns>Mean Anomaly (radians)</returns>
		static public double EccentricAnomalyToMeanAnomaly(double eccentricAnomaly, double eccentricity)
		{
			// Vallado, 3rd Ed., pg 54
			double meanAnomaly = Limit.FoldTwoPi(eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly));
			return meanAnomaly;
		}

		/// <summary>
		/// Calculate orbit normal LVLH (local vertical, local horizontal) DCM, inertial to body
		/// </summary>
		/// <param name="unitPos">Unit position vector from orbited body to orbiting body</param>
		/// <param name="unitVel">Unit velocity vector of orbiting body around orbited body.</param>
		/// <param name="lvlh">Local vertical, local horizontal DCM on output</param>
		/// <returns>LVLH DCM</returns>
		public static Dcm LvlhFromParent(Vector3 unitPos, Vector3 unitVel)
		{
			var lvlh = new Dcm();
			// Negative normal (negative of angular momentum direction)
			negNormal.CrossProduct(unitVel, unitPos);
			negNormal.Normalize();
			forward.CrossProduct(unitPos, negNormal);

			lvlh.SetRow(0, forward);
			lvlh.SetRow(1, negNormal);
			lvlh.SetRow(2, -unitPos);

			return lvlh;
		}

		/// <summary>
		/// Calculate orbit normal LVLH (local vertical, local horizontal) DCM, body to inertial
		/// </summary>
		/// <param name="pos">Position vector from orbited body to orbiting body, [m].</param>
		/// <param name="vel">Velocity vector of orbiting body around orbited body, [m/s].</param>
		/// <returns>LVLH DCM</returns>
		public static Dcm LvlhToParent(Vector3 pos, Vector3 vel)
		{
			var lvlh = new Dcm();
			unitPos = pos;
			unitPos.Normalize();
			unitVel = vel;
			unitVel.Normalize();

			// Negative normal (negative of angular momentum direction)
			negNormal.CrossProduct(unitVel, unitPos);
			negNormal.Normalize();
			forward.CrossProduct(unitPos, negNormal);

			lvlh.SetRow(0, forward);
			lvlh.SetRow(1, negNormal);
			lvlh.SetRow(2, -unitPos);

			return lvlh;
		}

		/// <summary>
		/// Converts Mean Anomaly to Eccentric Anomaly
		/// </summary>
		/// <param name="meanAnomaly">Mean Anomaly (rad)</param>
		/// <param name="eccentricity">Eccentricity</param>
		/// <returns>Eccentric Anomaly (rad)</returns>
		public static double MeanAnomalyToEccentricAnomaly(double meanAnomaly, double eccentricity)
		{
			// Find the eccentric anomaly by iteration.
			// ref "Practical Astronomy with your calculator", 2nd ed, pg 84
			// The required number of iterations is mainly a function of eccentricity, but as
			// eccentricity increases, high iteration counts occur near perigee.
			// With epsilon set to 1e-15, the simple version has:
			//   max of 4 iterations for e <= 0.13   Vast majority of satellites!
			//   max of 5 iterations for e <= 0.43
			//   max of 6 iterations for e <= 0.70
			//   max of 7 iterations for e <= 0.86
			//   max of 8 iterations for e <= 0.94
			double ea;
			double epsilon = 1e-15; // leave it here, works well
			double delta = 999.9;
			ea = meanAnomaly; // eccentric anomaly, set first guess
			bool simple = ((eccentricity < 0.7) || ((meanAnomaly > LowLimit) && (meanAnomaly < HighLimit)));
			if (simple)
			{
				while (Math.Abs(delta) > epsilon)
				{  // tolerance in radians
					delta = ea - eccentricity * Math.Sin(ea) - meanAnomaly;
					ea = ea - (delta / (1.0 - eccentricity * Math.Cos(ea)));
				}
				return ea;
			}

			// slightly more complex form of same iteration, Meeus, Astronautical Algorithms, 2nd Ed., page 205
			// worst case seen is 8 iterations
			while (Math.Abs(delta) > epsilon)
			{  // tolerance in radians
				delta = ea - eccentricity * Math.Sin(ea) - meanAnomaly;
				double delta2 = (delta / (1.0 - eccentricity * Math.Cos(ea)));
				if (delta2 > 0.5)
				{
					delta2 = 0.5;
				}
				else if (delta2 < -0.5)
				{
					delta2 = -0.5;
				}
				ea = ea - delta2;
			}
			return ea;
		}

		/// <summary>
		/// Convert Mean Anomaly to True Anomaly
		/// </summary>
		/// <param name="meanAnomaly">Mean Anomaly [rad]</param>
		/// <param name="eccentricity">Eccentricity</param>
		/// <param name="epsilon">convergence limit</param>
		/// <returns>True Anomaly [rad]</returns>
		public static double MeanAnomalyToTrueAnomaly(double meanAnomaly, double eccentricity, double epsilon)
		{
			// compute Eccentric Anomaly
			double eccentricAnomaly = MeanAnomalyToEccentricAnomaly(meanAnomaly, eccentricity);
			// compute true anomaly
			// note:
			// The following two equations are often written with with a denominator term of
			//   (1.0 - eccentricity*Math.Cos(eccentricAnomaly))
			// But since this is always a positive value, and it affects both terms the same way,
			// and the terms are ONLY used in an Atan2 function call, it is irrelevant.
			double sinTaPart = Math.Sqrt(1.0 - eccentricity * eccentricity) * Math.Sin(eccentricAnomaly);
			double cosTaPart = Math.Cos(eccentricAnomaly) - eccentricity;
			return Math.Atan2(sinTaPart, cosTaPart);
		}

		/// <summary>
		/// Converts True anomaly Eccentric Anomaly
		/// </summary>
		/// <param name="trueAnomaly">True Anomaly (rad)</param>
		/// <param name="eccentricity">Eccentricity</param>
		/// <returns>Eccentric Anomaly (rad)</returns>
		public static double TrueAnomalyToEccentricAnomaly(double trueAnomaly, double eccentricity)
		{
			// Vallado, 3rd Ed., pg 55 (also on pg 85)
			if (eccentricity == 0) return trueAnomaly;
			// the denominator of both terms in the text is the same and
			// is always positive for elliptical orbits
			double sinETop = Math.Sin(trueAnomaly) * Math.Sqrt(1 - eccentricity * eccentricity);
			double cosETop = eccentricity + Math.Cos(trueAnomaly);
			double eccentricAnomaly = Limit.FoldTwoPi(Math.Atan2(sinETop, cosETop));
			return eccentricAnomaly;
		}
	}
}
