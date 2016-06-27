using System;
using System.ComponentModel;

using Aspire.Framework;

namespace Aspire.Primitives
{
    public class OrbitalState
    {
		DateTime now;
		Vector3
			E = new Vector3(),
			H = new Vector3(),
			K = new Vector3(0,0,1),
			Hhat = new Vector3(),
			nodes = new Vector3(),
			position = new Vector3(),
			velocity = new Vector3(),
			vp = new Vector3(),
			workH = new Vector3();
		string localMeanSolarTime;
		double mu = Constant.EarthGravityConstant;
		double
			apoapsis, argumentOfLatitude, argumentOfPerigee, eccentricity, h, inclination,
			initialArgLatitude, meanAnomaly, meanMotion, omega,
			orbitCounter, orbits, period, periapsis, positionMag, prevOrbitFraction,
			rightAscension, semiLatusRectum, semiMajorAxis, semiMinorAxis, spEnergy,
			spKineticEnergy, spPotentialEnergy, timeToApoapsis, timeToPeriapsis, trueAnomaly,
			velocityMag;
		double
			Efactor = 0.000684,		// FME I wish I knew why this was required
			Ef0 = 0.002184,
			Ef1 = -2.204e-10;

		bool stateStale;

		private void CalculateLmst()
		{
			double angle = Limit.FoldTwoPi(-argumentOfPerigee * Constant.RadPerDeg);
			double DeltaTimeToAN = EpochToTrueAnomaly(angle);
			while (DeltaTimeToAN > 0) DeltaTimeToAN -= period;
			DateTime DateAtAN = now + new TimeSpan((long)(TimeSpan.TicksPerSecond * DeltaTimeToAN));
			// noon Jan 0, 1900
			DateTime refdate = new DateTime(1899, 12, 31, 12, 0, 0);
			// RA of fictitious Mean Sun, Vallado 3rd Ed., pg 185
			TimeSpan span = DateAtAN - refdate;
			double T = span.TotalDays / 36525;  // Julian Centuries, Vallado 3rd Ed., pg 4
			double RAFMS = ((18 * 60) + 38) * 60 + 8640184.542 * T + 0.0929 * T * T; // seconds of time
			RAFMS /= 3600;  // to hours of time
			RAFMS *= 15;  // convert to degrees (Meeus, pg 8)
			RAFMS *= Constant.RadPerDeg; // convert to radians
			// local mean solar time
			double lmst = Limit.FoldTwoPi(Math.PI + rightAscension * Constant.RadPerDeg - RAFMS); // radians
			double temp = lmst * 24 / Constant.TwoPi; // hours
			int hr = (int)temp;
			temp = (temp - hr) * 60;
			int min = (int)temp;
			int sec = (int)((temp - min) * 60);
			localMeanSolarTime = string.Format("{0:00}:{1:00}:{2:00}", hr, min, sec);
		}

		// Calculate the orbital elements and related values from the position and velocity.
		void CalculateState()
		{
			lock (this)
			{
				positionMag = position.Magnitude;
				velocityMag = velocity.Magnitude;
				// compute specific energy related values (energy per unit mass)
				spKineticEnergy = velocityMag * velocityMag * 0.5;
				spPotentialEnergy = -mu / positionMag;
				spEnergy = spKineticEnergy + spPotentialEnergy;

				// H = specific angular momentum, BMW, page 61.
				H.CrossProduct(position, velocity);

				// unitH is orbit normal unit vector, h is specific angular momentum
				Hhat = H;
				Hhat.Normalize(out h);

				// node vector, BMW, page 61.
				nodes.CrossProduct(K, H);

				bool equatorial = false;
				if (nodes.MagnitudeSquared < 1.0e-7)
				{
					// this is an equatorial orbit
					// Rotate r and v about x axis so the orbit is inclined, with a right ascension of zero.
					// This will allow the computations to work.  The change will be removed later.
					// Somewhat artificially makes equatorial osculating elements more stable.
					equatorial = true;
					position.RotateX(Constant.HalfPi);
					velocity.RotateX(Constant.HalfPi);
					workH.CrossProduct(position, velocity);
					nodes.CrossProduct(K, workH);
				}
				else
					workH = H;

				// E vector's magnitude will equal eccentricity, BMW, page 62.
				E = position;
				Efactor = Ef0 + Ef1 * positionMag;
				double V1 = (1 - Efactor) * velocityMag;
				E *= (V1 * V1 - mu / positionMag);
				vp = velocity*position.Dot(velocity);  // vp is a temporary variable
				E = (E - vp) * (1 / mu);
				eccentricity = E.Magnitude;  // eccentricity

				// semi-Latus Rectum, BMW page 26.
				semiLatusRectum = workH.Dot(workH) / mu;

				// inclination, BMW page 63.
				// the clamp prevents occasional round-off error from making the value slightly >1 or < -1.
				inclination = Math.Acos(Limit.Clamp(workH[2] / workH.Magnitude, 1.0)) * Constant.DegPerRad;

				// right ascension, BMW page 63.
				// the clamp prevents occasional round-off error from making the value slightly >1 or < -1.
				omega = Math.Acos(Limit.Clamp(nodes[0] / nodes.Magnitude, 1.0)) * Constant.DegPerRad;
				if (nodes[1] < 0)
					rightAscension = 360 - omega;
				else
					rightAscension = omega;

				if (eccentricity < 1.0e-10)
				{
					// circular orbit
					// compute remaining elements
					argumentOfPerigee = 0;              // arg. perigee, we define to be zero
					Vector3 nunit = nodes;
					nunit.Normalize();
					Vector3 unitR = position;
					unitR.Normalize();
					trueAnomaly = Math.Acos(Limit.Clamp(nunit.Dot(unitR), 1.0)) * Constant.DegPerRad;  // true anomaly
					argumentOfLatitude = trueAnomaly;
					meanAnomaly = trueAnomaly;       // mean anomaly
					semiMajorAxis = positionMag;     // semi-major axis
				}
				else
				{
					// non-circular orbit
					// argument of perigee, BMW page 63.
					double arg = Math.Acos(Limit.Clamp(nodes.Dot(E) / (nodes.Magnitude * eccentricity), 1.0)) * Constant.DegPerRad;
					if (E[2] < 0)
						argumentOfPerigee = 360 - arg;
					else
						argumentOfPerigee = arg;

					// true anomaly, BMW page 63.
					trueAnomaly = Math.Acos(Limit.Clamp(E.Dot(position) / (eccentricity * positionMag), 1.0)) * Constant.DegPerRad;
					if (position.Dot(velocity) < 0) trueAnomaly = 360 - trueAnomaly;

					double arglat = argumentOfPerigee + trueAnomaly;
					if (arglat < 360)
						argumentOfLatitude = arglat;
					else
						argumentOfLatitude = arglat - 360;

					// semi-major axis, BMW page 24.
					double oneE2 = 1 - eccentricity * eccentricity;
					semiMajorAxis = semiLatusRectum / oneE2;

					// mean anomaly, Vallado, pg 211 and 213
					// note: denominator terms are the same and always > 0 (since e<1), so no need to compute
					double sinetop = Math.Sqrt(oneE2) * Math.Sin(trueAnomaly * Constant.RadPerDeg);
					double cosetop = eccentricity + Math.Cos(trueAnomaly * Constant.RadPerDeg);
					double eccanon = Math.Atan2(sinetop, cosetop);
					meanAnomaly = (eccanon - eccentricity * Math.Sin(eccanon)) * Constant.DegPerRad;
					if (meanAnomaly < 0) meanAnomaly += 360;
				}
				double orbitFraction = Limit.Fold360(argumentOfLatitude - initialArgLatitude) / 360;
				if (orbitFraction < prevOrbitFraction)
					orbitCounter++;
				prevOrbitFraction = orbitFraction;
				orbits = orbitCounter + orbitFraction;

				if (equatorial)
				{
					// return inclination to proper value
					inclination = 0;
					if (Hhat.Z < 0)
					{
						// retrograde equatorial (highly unlikely)
						inclination = 180;
					}
				}

				// other useful values
				period = Constant.TwoPi * Math.Sqrt(semiMajorAxis * semiMajorAxis * semiMajorAxis / mu); // BMW page 33
				meanMotion = Constant.TwoPi / period;
				periapsis = semiMajorAxis * (1 - eccentricity); // BMW page 25
				apoapsis = semiMajorAxis * (1 + eccentricity);  // BMW page 25
				semiMinorAxis = Math.Sqrt(semiMajorAxis * semiLatusRectum);  // source?
				timeToPeriapsis = (360 - meanAnomaly) * Constant.RadPerDeg / meanMotion;
				timeToApoapsis = timeToPeriapsis + period / 2;
				if (timeToApoapsis > period) timeToApoapsis -= period;

				// record that computations have been done
				stateStale = false;
			}
		}

		// Move this to Orbit ?

		/// <summary>
		/// Computes the time (in seconds) needed to get from the position at epoch
		/// to the requested true anomaly (in radians).
		/// <para>Always flys forward in time to reach the desired True Anomaly.</para>
		/// </summary>
		/// <param name="desiredTA">desired true anomaly (radians)</param>
		/// <returns>time to reach desired true anomaly (seconds past epoch)</returns>
		public double EpochToTrueAnomaly(double desiredTA)
		{
			// find desired Eccentric Anomaly
			double desiredEA = Orbit.TrueAnomalyToEccentricAnomaly(desiredTA, eccentricity);
			// find the corresponding mean anomaly
			double desiredMA = Orbit.EccentricAnomalyToMeanAnomaly(desiredEA, eccentricity);
			// find the needed ma change
			double deltama = desiredMA - meanAnomaly * Constant.RadPerDeg;
			if (deltama < 0) deltama += Constant.TwoPi;
			// convert deltama to time in seconds
			double tof = (deltama / Constant.TwoPi) * period;
			return tof;
		}

		/// <summary>
		/// Latch the initial position for the orbit counter
		/// </summary>
		public void Initialize()
		{
			prevOrbitFraction = 0;
			orbitCounter = 0;
			CalculateState();
			initialArgLatitude = argumentOfLatitude;
			orbits = 0;
			stateStale = true;
		}

		/// <summary>
		/// Record that position or velocity changed.
		/// Since this class maintains a reference to the position and velocity vectors
		/// given in the constructor, when these values are changed, then this function
		/// should be called.  All state properties then are flagged to indicate that
		/// they must be recomputed.
		/// </summary>
		/// <param name="positionEci"></param>
		/// <param name="velocityEci"></param>
		public void Update(Vector3 positionEci, Vector3 velocityEci, DateTime now)
		{
			position = positionEci;
			velocity = velocityEci;
			this.now = now;
			// This allows the state to be updated often, but the relatively expensive
			// computations needed to compute the osculating orbital elements are only
			// done if they are needed.
			stateStale = true;
		}

		#region Properties

		/// <summary>radius to apoapsis</summary>
		[Blackboard(Units = "m"),Description("radius to apoapsis, [m]")]
		public double Apoapsis
		{
			get
			{
				if (stateStale) CalculateState();
				return apoapsis;
			}
		}

		/// <summary>Argument of Latitude</summary>
		[Blackboard(Units = "deg"),Description("Argument of Latitude, [m]")]
		public double ArgumentOfLatitude
		{
			get
			{
				if (stateStale) CalculateState();
				return argumentOfLatitude;
			}
		}

		/// <summary>Argument of Perigee</summary>
		[Blackboard(Units = "deg"), Description("Argument of Perigee, [deg]")]
		public double ArgumentOfPerigee
		{
			get
			{
				if (stateStale) CalculateState();
				return argumentOfPerigee;
			}
		}

		/// <summary>Eccentricity</summary>
		[Blackboard,Description("Orbital Eccentricity")]
		public double Eccentricity
		{
			get
			{
				if (stateStale) CalculateState();
				return eccentricity;
			}
		}

		/// <summary>Unit angular momentum</summary>
		[Blackboard,Description("Unit angular momentum (orbit normal)")]
		public Vector3 Hunit
		{
			get
			{
				if (stateStale) CalculateState();
				return Hhat;
			}
		}

		/// <summary>Inclination</summary>
		[Blackboard(Units = "deg"),Description("Orbital Inclination [deg]")]
		public double Inclination
		{
			get
			{
				if (stateStale) CalculateState();
				return inclination;
			}
		}

		/// <summary>Local Mean Solar Time</summary>
		[Blackboard, Description("Local Mean Solar Time (HH:MM:SS)")]
		public string LocalMeanSolarTime
		{
			get
			{
				if (stateStale) CalculateState();
				CalculateLmst();
				return localMeanSolarTime;
			}
		}

		/// <summary>Mean Anomaly</summary>
		[Blackboard(Units = "deg"), Description("Mean Anomaly, [deg]")]
		public double MeanAnomaly
		{
			get
			{
				if (stateStale) CalculateState();
				return meanAnomaly;
			}
		}

		/// <summary>Mean Motion</summary>
		[Blackboard(Units = "rad/s"),Description("Mean Motion, [rad/s]")]
		public double MeanMotion
		{
			get
			{
				if (stateStale) CalculateState();
				return meanMotion;
			}
		}

		/// <summary>Gravitational constant, Earth</summary>
		[Blackboard(Units = "m^3/s^2"),Description("Gravitational constant, Earth, [m^2/s^2]")]
		public double Mu { get { return mu; } }

		/// <summary>Number of elapsed orbits</summary>
		[Blackboard(Units = "orbit"),Description("Number of elapsed orbits, [m]")]
		public double Orbits
		{
			get
			{
				if (stateStale) CalculateState();
				return orbits;
			}
		}

		/// <summary>radius to periapsis</summary>
		[Blackboard(Units = "m"),Description("radius to periapsis, [m]")]
		public double Periapsis
		{
			get
			{
				if (stateStale) CalculateState();
				return periapsis;
			}
		}

		/// <summary>Period</summary>
		[Blackboard(Units = "s"),Description("Orbital Period [s]")]
		public double Period
		{
			get
			{
				if (stateStale) CalculateState();
				return period;
			}
		}

		/// <summary>Distance from central body</summary>
		[Blackboard(Units = "m"),Description("Distance from central body to object, [m]")]
		public double PositionMagnitude
		{
			get
			{
				if (stateStale) CalculateState();
				return positionMag;
			}
		}

		/// <summary>ight ascension</summary>
		[Blackboard(Units = "deg"), Description("Right ascension of the ascending node, [deg]")]
		public double RightAscension
		{
			get
			{
				if (stateStale) CalculateState();
				return rightAscension;
			}
		}

		/// <summary>Semi-latus rectum length</summary>
		[Blackboard(Units = "m"),Description("Semi-latus rectum length, [m]")]
		public double SemiLatusRectum
		{
			get
			{
				if (stateStale) CalculateState();
				return semiLatusRectum;
			}
		}

		/// <summary>Semi-major axis length</summary>
		[Blackboard(Units = "m"),Description("Semi-major axis length [m]")]
		public double SemiMajorAxis
		{
			get
			{
				if (stateStale) CalculateState();
				return semiMajorAxis;
			}
		}

		/// <summary>Semi-minor axis length</summary>
		[Blackboard(Units = "m"),Description("Semi-minor axis length of elliptical orbit, [m]")]
		public double SemiMinorAxis
		{
			get
			{
				if (stateStale) CalculateState();
				return semiMinorAxis;
			}
		}

		/// <summary>Specific angular momentum</summary>
		[Blackboard(Units = "m2/s"), Description("Specific angular momentum, [m^2/s]")]
		public Vector3 SpAngularMomentum
		{
			get
			{
				if (stateStale) CalculateState();
				return H;
			}
		}

		/// <summary>Specific angular momentum magnitude</summary>
		[Blackboard(Units = "m^2/s"),Description("Specific angular momentum magnitude, [m^2/s]")]
		public double SpAngularMomentumMag
		{
			get
			{
				if (stateStale) CalculateState();
				return h;
			}
		}

		/// <summary>Specific energy</summary>
		[Blackboard(Units = "J"),Description("Specific energy, [J]")]
		public double SpEnergy
		{
			get
			{
				if (stateStale) CalculateState();
				return spEnergy;
			}
		}

		/// <summary>Specific kinetic energy</summary>
		[Blackboard(Units = "J"),Description("Specific kinetic energy, [J]")]
		public double SpKineticEnergy
		{
			get
			{
				if (stateStale) CalculateState();
				return spKineticEnergy;
			}
		}

		/// <summary>Specific potential energy</summary>
		[Blackboard(Units = "J"),Description("Specific potential energy, [J]")]
		public double SpPotentialEnergy
		{
			get
			{
				if (stateStale) CalculateState();
				return spPotentialEnergy;
			}
		}

		/// <summary>Time to Periapsis</summary>
		[Blackboard(Units = "s"),Description("Time to Apoapsis, [s]")]
		public double TimeToApoapsis
		{
			get
			{
				if (stateStale) CalculateState();
				return timeToApoapsis;
			}
		}

		/// <summary>Time to Periapsis</summary>
		[Blackboard(Units = "s"),Description("Time to Periapsis [s]")]
		public double TimeToPeriapsis
		{
			get
			{
				if (stateStale) CalculateState();
				return timeToPeriapsis;
			}
		}

		/// <summary>True anomaly</summary>
		[Blackboard(Units = "deg"),Description("True anomaly, [deg]")]
		public double TrueAnomaly
		{
			get
			{
				if (stateStale) CalculateState();
				return trueAnomaly;
			}
		}

		/// <summary>Current Velocity magnitude</summary>
		[Blackboard(Units = "m/s"),Description("Current Velocity magnitude, [m/s]")]
		public double VelocityMagnitude
		{
			get
			{
				if (stateStale) CalculateState();
				return velocityMag;
			}
		}

		#endregion
	}
}
