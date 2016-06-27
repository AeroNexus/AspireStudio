using System;
using System.ComponentModel;
using System.Text;

using Aspire.Utilities;

namespace Aspire.Primitives
{
	/// <summary>
	/// Describes an orbit with the classical Keplerian elements
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class KeplerElements
    {
		public static KeplerElements Parse(string text)
		{
			var ke = new KeplerElements();
			var elements = text.Split(',');
			foreach (var elem in elements)
			{
				var token = elem.Split('=');
				if (token.Length != 2) continue;
				var keyword = token[0];
				double value = 0;
				if ( keyword != "epoch")
					value = double.Parse(token[1]);
				switch (keyword[0])
				{
					case 'a': ke.ArgumentOfPerigee = value; break;
					case 'e':
						if ( keyword == "epoch" )
							ke.Epoch = DateTime.Parse(token[1]);
						else
							ke.Eccentricity = value; break;
					case 'i': ke.Inclination = value; break;
					case 'h': ke.Altitude = value * Constant.Kilo; ke.altitudeSet = true; break;
					case 's': ke.SemiMajorAxis = value*1000; break;
					case 'r': ke.RightAscension = value; break;
					case 'm': ke.MeanAnomaly = value; break;
					case 't': ke.TrueAnomaly = value; ke.trueAnomalySet = true;  break;
				}
			}
			return ke;
		}

		#region Properties

		[Description("Altitude [m] wrt/ equatorial radius.")]
		public double Altitude { get; set; }
		bool altitudeSet;

		[Description("Name of the central body these elements describe an orbit around."),DefaultValue("Earth")]
		public string CentralBodyName
		{
			get { return centralBodyName; }
			set { centralBodyName = value; }
		} string centralBodyName = "Earth";

		[Description("Argument of perigee [deg]. Location of the periapsis wrt/ line of nodes.")]
		public double ArgumentOfPerigee { get; set; }

		[Description("Eccentricity of the orbital ellipse.")]
		public double Eccentricity { get; set; }

		[Description("Epoch of these elements.")]
		public DateTime Epoch { get; set; }

		[Description("Inclination [deg] of the orbital plane wrt/ orbit normal.")]
		public double Inclination { get; set; }

		[Description("Mean anomaly [deg]. Average location of the orbiting object wrt/ periapsis.")]
		public double MeanAnomaly { get; set; }

		[Description("Orbital period [sec].")]
		public double Period { get; set; }

		[Description("Right Ascenscion of the ascending node [deg]. Intersection of the orbital plane w/ line of nodes.")]
		public double RightAscension { get; set; }

		[Description("Semi-major axis [m] of the orbital ellipse.")]
		public double SemiMajorAxis { get; set; }

		[Description("True anomaly [deg]. Location of the orbiting object wrt/ periapsis.")]
		public double TrueAnomaly { get; set; }
		bool trueAnomalySet;

		#endregion

		public void Propagate(DateTime when, ref Vector3 position, ref Vector3 velocity)
		{
			if (SemiMajorAxis == 0 && Altitude != 0)
			{
				if (CentralBodyName == "Earth")
					SemiMajorAxis = Altitude + Constant.EarthEquatorialRadius;
				else
					Log.WriteLine("Earth is the only central body supported at this time");
			}

			const double J2 = 1082.63e-6; // for earth for now. Need to go to celestialBody.Gravity model.
			const double Re = Constant.EarthEquatorialRadius;
			const double gravitation = Constant.EarthGravityConstant; // Need to use Gravity model
			double e = Eccentricity;
			double i = Inclination * Constant.RadPerDeg;

			// compute the mean motion, n
			double n = Math.Sqrt(gravitation / Math.Pow(SemiMajorAxis, 3.0)) * Constant.DegPerRad;// deg/s
			double Rratio = Re / SemiMajorAxis;
			double one_e2 = 1 - e * e;
			double sinI = Math.Sin(i);
			double meanAngularMotionDot = n * (1 + 1.5 * J2 * (Rratio * Rratio) * Math.Pow(one_e2, -1.5) * (1 - 1.5 * sinI * sinI));
			double rightAscensionDot = -1.5 * J2 * meanAngularMotionDot * (Rratio * Rratio) * Math.Pow(one_e2, -2) * Math.Cos(i);
			double argOfPerigeeDot = 1.5 * J2 * meanAngularMotionDot * (Rratio * Rratio) * Math.Pow(one_e2, -2) * (2 - 2.5 * sinI * sinI);

			double elapsedSec = 0;
			if (Epoch.Ticks > 0)
				elapsedSec = (when - Epoch).TotalSeconds;
			double meanAnomalyT = Maths.Mod(MeanAnomaly + meanAngularMotionDot * elapsedSec, 360);
			double rightAscensionT = Maths.Mod(RightAscension + rightAscensionDot * elapsedSec, 360);
			double argOfPerigeeT = Maths.Mod(ArgumentOfPerigee + argOfPerigeeDot * elapsedSec, 360);

			// compute the mean motion, n
			n = Math.Sqrt(gravitation / Math.Pow(SemiMajorAxis, 3.0));
			// compute mean anomaly at new time

			//caution: eccentricity should be checked before this function is called
			//         this should really be an exception
			//         further, we really shouln't allow parabolic orbit (e = 1)
			e = Limit.Clamp(e, 0.0, 1.0);

			// compute Eccentric Anomaly
			double eccentricAnomaly = Orbit.MeanAnomalyToEccentricAnomaly(meanAnomalyT*Constant.RadPerDeg, e);

			// temporary variables for values used multiple times
			double sEA = Math.Sin(eccentricAnomaly);
			double cEA = Math.Cos(eccentricAnomaly);
			double sqrtomesq = Math.Sqrt(1.0 - e * e);

			// note:
			// The following two equations are often written with with a denominator term of
			//   (1.0 - eccentricity*Math.Cos(eccentricAnomaly))
			// But since this is always a positive value, and it affects both terms the same way,
			// and the terms are ONLY used in an Atan2 function call, it is irrelevant.
			double sinTaPart = sqrtomesq * sEA;
			double cosTaPart = cEA - e;
			double trueAnomaly = Math.Atan2(sinTaPart, cosTaPart);

			double rtAscension = rightAscensionT * Constant.RadPerDeg;
			double argPerigee = argOfPerigeeT * Constant.RadPerDeg;

			// more temporary variables for values used multiple times
			double cRA = Math.Cos(rtAscension);
			double sRA = Math.Sin(rtAscension);
			double sI = Math.Sin(i);
			double cI = Math.Cos(i);
			double sAP = Math.Sin(argPerigee);
			double cAP = Math.Cos(argPerigee);
			double sAL = Math.Sin(argPerigee + trueAnomaly);  // sin(Argument of Latitude)
			double cAL = Math.Cos(argPerigee + trueAnomaly);  // cos(Argument of Latitude)

			double r = SemiMajorAxis * (1.0 - e * cEA);

			position.X = r * (cAL * cRA - sAL * sRA * cI);
			position.Y = r * (cAL * sRA + sAL * cRA * cI);
			position.Z = r * sAL * sI;

			double semiminorAxis = SemiMajorAxis * sqrtomesq;
			double l1 = cRA * cAP - sRA * sAP * cI;
			double m1 = sRA * cAP + cRA * sAP * cI;
			double n1 = sAP * sI;
			double l2 = -cRA * sAP - sRA * cAP * cI;
			double m2 = -sRA * sAP + cRA * cAP * cI;
			double n2 = cAP * sI;

			velocity.X = (n * SemiMajorAxis / r) * (semiminorAxis * l2 * cEA - SemiMajorAxis * l1 * sEA);
			velocity.Y = (n * SemiMajorAxis / r) * (semiminorAxis * m2 * cEA - SemiMajorAxis * m1 * sEA);
			velocity.Z = (n * SemiMajorAxis / r) * (semiminorAxis * n2 * cEA - SemiMajorAxis * n1 * sEA);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			if ( altitudeSet )
				sb.AppendFormat("h={0}",Altitude/Constant.Kilo);
			else
				sb.AppendFormat("s={0}",SemiMajorAxis/Constant.Kilo);
			if ( Eccentricity > 0 )
				sb.AppendFormat(",e={0}",Eccentricity);
			if ( Inclination != 0 )
				sb.AppendFormat(",i={0}",Inclination);
			if ( RightAscension != 0 )
				sb.AppendFormat(",raan={0}",RightAscension);
			if ( ArgumentOfPerigee != 0 )
				sb.AppendFormat(",argP={0}",ArgumentOfPerigee);
			if ( trueAnomalySet )
				sb.AppendFormat(",ta={0}",TrueAnomaly);
			else if ( MeanAnomaly != 0 )
				sb.AppendFormat(",ma={0}",MeanAnomaly);
			if ( Epoch.Ticks != 0 )
				sb.AppendFormat(",epoch={0}",Epoch);

			return sb.ToString();
		}
	}
}
