using System;
using System.ComponentModel;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Primitives;

namespace Aspire.Space
{
	public class CelestialBody : EnvironmentModel, IBody, ICelestialBody, IHaveFrames
	{
		#region Declarations

		AstroClock clock;
		//[Blackboard()]
		DynamicFrame bodyFrame;	// created in Discover
		Earth earth;
		EcefState ecef = new EcefState();
		//[Blackboard()]
		Frame centralBodyFrame;	// Body frame of the central body
		FrameList frames;	// created in Discover
		Sun sun;

		//[Blackboard()]
		double argumentOfPeriapsis;
		//[Blackboard()]
		double eccentricity;
		//[Blackboard()]
		double meanAnomaly;
		//[Blackboard()]
		double meanLongitude;
		//[Blackboard()]
		double inclination;
		//[Blackboard()]
		double semimajorAxis;
		//[Blackboard()]
		double longitudeAscendingNode;
		//[Blackboard()]
		double longitudePeriapsis;

		//[Blackboard()]
		double eclipticR;
		//[Blackboard()]
		double eclipticLat;
		//[Blackboard()]
		double eclipticLon;

		//[Blackboard()]
		double lamda;
		//[Blackboard()]
		double beta;

		//[Blackboard()]
		double rightAscension;
		//[Blackboard()]
		double declination;

		/// <summary>
		/// 
		/// </summary>
		[Blackboard()]
		protected Vector3 eclipticPosition = new Vector3();
		/// <summary>
		/// 
		/// </summary>
		[Blackboard()]
		protected Vector3 geocentricPosition = new Vector3();


		#endregion

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			clock = scenario.Clock.GetService(typeof(AstroClock)) as AstroClock;

			if (!(Parent is Environment))
				Environment.AddModel(this);

			base.Discover(scenario);

			frames = new FrameList(){Name = this.Name};
			bodyFrame = new DynamicFrame("body", frames);
			bodyFrame.RelativeVelocity = new Vector3();
			bodyFrame.Location = geocentricPosition;
			Initialize();
		}

		public override void Initialize()
		{
			if (centralBodyFrame == null)
			{
				var env = Parent as Environment;
				ICelestialBody centralBody = env.CelestialBody(centralBodyName);
				if (centralBody is IHaveFrames)
					centralBodyFrame = (centralBody as IHaveFrames).GetFrame("body");
				earth = env.CelestialBody("Earth") as Earth;
				sun = env.CelestialBody("Sun") as Sun;
			}

			Execute();
			// For now, only initialize velocity. Need to put in flag to update it.
			bodyFrame.RelativeVelocity = 
				bodyFrame.Location - CalculatePosition(clock.J2000Century - 1 * Constant.DayPerSec / Constant.DayPerCentury);
			base.Initialize();
		}

		public override void Execute()
		{
			Vector3 gcPos = CalculatePosition(clock.J2000CenturyTT);

			if (raDecl)
			{
				lamda = Limit.FoldTwoPi(Math.Atan2(gcPos.Y, gcPos.X));
				beta = gcPos.Z / Math.Sqrt(gcPos.X * gcPos.X + gcPos.Y * gcPos.Y);

				double sinLamda = Math.Sin(lamda);
				double cosLamda = Math.Cos(lamda);
				double sinBeta = Math.Sin(beta);
				double cosBeta = Math.Cos(beta);

				double sinEcl = earth.SinEcliptic;
				double cosEcl = earth.CosEcliptic;

				rightAscension = Limit.Fold360(Math.Atan2(sinLamda * cosEcl - sinBeta / cosBeta * sinEcl, cosLamda) * Constant.DegPerRad);
				declination = Math.Asin(sinBeta * cosEcl + cosBeta * sinEcl * sinLamda) * Constant.DegPerRad;

				lamda *= Constant.DegPerRad;
				beta *= Constant.DegPerRad;
			}

			//bodyFrame.Location.Assign( geocentricPosition );

			eciRunit = bodyFrame.Location;
			eciRunit.Normalize(out eciRmag);
			ecef.CalculateLatLon(this);
			base.Execute();
		}

		#endregion

		private Vector3 CalculatePosition(double julianCentury)
		{
			semimajorAxis = semimajorAxisTable.Value(julianCentury);
			double sma = semimajorAxis;// *Constant.AUtoM;
			eccentricity = Limit.Clamp(eccentricityTable.Value(julianCentury), 0.0, 1.0);

			inclination = inclinationTable.Value(julianCentury);
			double i = inclination * Constant.RadPerDeg;
			double sinI = Math.Sin(i);
			double cosI = Math.Cos(i);

			meanLongitude = Limit.Fold360(meanLongitudeTable.Value(julianCentury));

			longitudeAscendingNode = Limit.Fold360(longitudeAscendingNodeTable.Value(julianCentury));
			double o = longitudeAscendingNode * Constant.RadPerDeg;
			double sinO = Math.Sin(o);
			double cosO = Math.Cos(o);

			longitudePeriapsis = Limit.Fold360(longitudePeriapsisTable.Value(julianCentury));

			argumentOfPeriapsis = Limit.Fold360(longitudePeriapsis - longitudeAscendingNode);
			double W = argumentOfPeriapsis * Constant.RadPerDeg;
			double sinW = Math.Sin(W);
			double cosW = Math.Cos(W);

			// Eccentric Anomaly
			meanAnomaly = Limit.Fold180(meanLongitude - longitudePeriapsis);
			double eccentricAnomaly = Orbit.MeanAnomalyToEccentricAnomaly(meanAnomaly * Constant.RadPerDeg, eccentricity);
			double sinEA = Math.Sin(eccentricAnomaly);
			double cosEA = Math.Cos(eccentricAnomaly);

			// position in the orbital plane: x, y and radius, r
			double x = sma * (cosEA - eccentricity);
			double y = sma * Math.Sqrt(1.0 - eccentricity * eccentricity) * sinEA;
			double r = sma * (1.0 - eccentricity * cosEA);

			// Ecliptic position
			eclipticPosition.X = (cosW * cosO - sinW * sinO * cosI) * x + (-sinW * cosO - cosW * sinO * cosI) * y;
			eclipticPosition.Y = (cosW * sinO + sinW * cosO * cosI) * x + (-sinW * sinO + cosW * cosO * cosI) * y;
			eclipticPosition.Z = (sinW * sinI) * x + (cosW * sinI) * y;

			eclipticR = eclipticPosition.Magnitude * Constant.MperAU;
			double L = Math.Atan2(eclipticPosition.Y, eclipticPosition.X);
			eclipticLon = Limit.Fold360(L * Constant.DegPerRad);
			double Rxy = Math.Sqrt(eclipticPosition.X * eclipticPosition.X + eclipticPosition.Y * eclipticPosition.Y);
			double B = Math.Asin(eclipticPosition.Z / Rxy);
			eclipticLat = B * Constant.DegPerRad;

			double cosB = Math.Cos(B);
			double sinB = Math.Sin(B);
			double cosL = Math.Cos(L);
			double sinL = Math.Sin(L);
			double Ro = sun.SunEarthDistance;
			const double cosBo = 1;
			const double sinBo = 0;
			double cosLo = -sun.CosEclipticLongitude;
			double sinLo = -sun.SinEclipticLongitude;
			Vector3 gcPos = geocentricPosition;
			gcPos.X = eclipticR * cosB * cosL - Ro * cosBo * cosLo;
			gcPos.Y = eclipticR * cosB * sinL - Ro * cosBo * sinLo;
			gcPos.Z = eclipticR * sinB - Ro * sinBo;
			return gcPos;
		}

		#region Properties

		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute("raDec")]
		public bool CalculateRaDecl
		{
			get { return raDecl; }
			set { raDecl = value; }
		} bool raDecl;

		/// <summary>
		/// Central body that this celestial body orbits around
		/// </summary>
		[XmlAttribute("centralBody")]
		public string CentralBody
		{
			get { return centralBodyName; }
			set { centralBodyName = value; }
		} string centralBodyName = string.Empty;

		/// <summary>
		/// 
		/// </summary>
		[Blackboard()]
		public string Dec { get { return Maths.FormatAngle(decFormat, declination); } }

		/// <summary>
		/// 
		/// </summary>
		public string DecFormat
		{
			get { return decFormat; }
			set { decFormat = value; }
		} string decFormat = "%D%@%M'%S\"%.1f";

		/// <summary>
		/// Which equinox are we using: J2000 TOD, B1950, B2000, mean of date
		/// </summary>
		[XmlAttribute("equinox")]
		public string Equinox
		{
			get { return equinox; }
			set { equinox = value; }
		} string equinox = string.Empty;

		/// <summary>
		/// Earth Latitude
		/// </summary>
		[XmlIgnore]
		public double Latitude
		{
			get { return ecef.Latitude; }
		}

		/// <summary>
		/// Earth Longitude
		/// </summary>
		[XmlIgnore]
		public double Longitude
		{
			get { return ecef.Longitude; }
		}
		/// <summary>
		/// 
		/// </summary>
		[Blackboard()]
		public string RA { get { return Maths.FormatAngle(raFormat, rightAscension); } }

		/// <summary>
		/// 
		/// </summary>
		public string RAformat
		{
			get { return raFormat; }
			set { raFormat = value; }
		} string raFormat = "%Hh%M'%S\"%.2f";

		/// <summary>
		/// Half the major axis length, [AU]
		/// </summary>
		public string SemimajorAxisAu
		{
			get { return semimajorAxisTable.Polynomials[0].ToString(); }
			set { semimajorAxisTable = new PolyTable(value); }
		} PolyTable semimajorAxisTable;

		/// <summary>
		/// Eccentricity
		/// </summary>
		public string Eccentricity
		{
			get { return eccentricityTable.Polynomials[0].ToString(); }
			set { eccentricityTable = new PolyTable(value); }
		} PolyTable eccentricityTable;

		/// <summary>
		/// Inclination of the orbit, [deg]
		/// </summary>
		public string Inclination
		{
			get { return inclinationTable.Polynomials[0].ToString(); }
			set { inclinationTable = new PolyTable(value); }
		} PolyTable inclinationTable;

		/// <summary>
		/// Half the major axis length, [AU]
		/// </summary>
		public string MeanLongitude
		{
			get { return meanLongitudeTable.Polynomials[0].ToString(); }
			set { meanLongitudeTable = new PolyTable(value); }
		} PolyTable meanLongitudeTable;

		/// <summary>
		/// longitude of the ascending node of the orbit, [deg]
		/// </summary>
		public string LongitudeAscendingNode
		{
			get { return longitudeAscendingNodeTable.Polynomials[0].ToString(); }
			set { longitudeAscendingNodeTable = new PolyTable(value); }
		} PolyTable longitudeAscendingNodeTable;

		/// <summary>
		/// longitude of periapsis of the orbit, [deg]
		/// </summary>
		public string LongitudePeriapsis
		{
			get { return longitudePeriapsisTable.Polynomials[0].ToString(); }
			set { longitudePeriapsisTable = new PolyTable(value); }
		} PolyTable longitudePeriapsisTable;

		#endregion

		#region IBody Members

		/// <summary>
		/// Get the inertial position magnitude
		/// </summary>
		public Vector3 EciR { get { return bodyFrame.Location; } set { bodyFrame.Location = value; } }

		/// <summary>
		/// Get the inertial position
		/// </summary>
		public double EciRmag { get { return eciRmag; } } double eciRmag;

		/// <summary>
		/// Get the inertial position unit vector
		/// </summary>
		public Vector3 EciRunit { get { return eciRunit; } } Vector3 eciRunit = new Vector3();

		/// <summary>
		/// Get the inertial velocity
		/// </summary>
		public Vector3 EciV { get { return eciV; } set { eciV = value; } } Vector3 eciV = new Vector3();

		/// <summary>
		/// Get the inertial position
		/// </summary>
		public double EciVmag { get { return eciVmag; } } double eciVmag = 0;

		/// <summary>
		/// Get the inertial position unit vector
		/// </summary>
		public Vector3 EciVunit { get { return eciVunit; } } Vector3 eciVunit = new Vector3();

		public Vector3 BodyRate
		{
			get { return bodyRate; }
			set { bodyRate = value; ; }
		} Vector3 bodyRate = new Vector3();

		public MassProperties MassProperties
		{
			get { return null; }
		}

		public void NormalizePosVel()
		{
		}

		#endregion

		#region ICelestialBody Members

		// Position is provided by IVsualEntity

		/// <summary>
		/// Radius of the body, [m]
		/// </summary>
		public double Radius { get { return radius; } } double radius = 0;

		/// <summary>
		/// Radius at the poles of the body, [m]
		/// </summary>
		public double PolarRadius { get { return polarRadius; } } double polarRadius = 0;

		/// <summary>
		///
		/// </summary>
		public Dcm InertialToBodyDcm { get { return bodyFrame.FromParentDcm; } }

		/// <summary>
		///
		/// </summary>
		public Dcm BodyToInertialDcm { get { return bodyFrame.ToParentDcm; } }


		/// <summary>
		///Mass used to calculate GM
		/// </summary>
		public double Mass { get { return mass; } } double mass = 0;

		/// <summary>
		/// 
		/// </summary>
		public Vector3 Velocity { get { return bodyFrame.RelativeVelocity; } }

		#endregion

		#region IHaveFrames

		/// <summary>
		/// Access the array
		/// </summary>
		[TypeConverter(typeof(ExpandableObjectConverter)),XmlIgnore]
		public FrameList Frames { get { return frames; } }
		/// <summary>
		/// Access the frame list
		/// </summary>
		public void AddFrame(Frame frame) { frames.AddFrame(frame); }
		/// <summary>
		/// Get a specific frame
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Frame GetFrame(string name)
		{
			return frames.GetFrame(name);
		}

		#endregion

		#region IVisualInfoProvider

		/// <summary>
		/// Position of entity
		/// </summary>
		[Category("Visual Entity")]
		[Description("ECI position vector [m]")]
		public Vector3 Position { get { return bodyFrame.Location; } }

		/// <summary>
		/// Attitude DCM of entity
		/// </summary>
		[Category("Visual Entity")]
		[Description("Direction cosine matrix representation of ECI to body rotation")]
		public Dcm Dcm { get { return bodyFrame.FromParentDcm; } }

		/// <summary>
		/// Attitude Quaternion of entity
		/// </summary>
		[Category("Visual Entity")]
		[Description("Quaternion representation of ECI to body rotation")]
		public Quaternion Orientation { get { return bodyFrame.FromParentQ; } }

		/// <summary>
		/// Visual File Name
		/// </summary>
		[Category("Visual Entity"), DefaultValue("Moon.xml"), XmlAttribute("visualFileName")]
		[Description("File name of visual model characteristics")]
		public string VisualFileName { get; set; }

		#endregion

		#region Context
		/// <summary>
		/// 
		/// </summary>
		public class CelestialBodyContext : EnvironmentModelContext, I_NBodyGravity
		{
			IBody ownerBody;
			CelestialBody body;
			EcefState ecef = new EcefState();

			public CelestialBodyContext() { }

			/// <summary>
			/// Constructor to set model and owner
			/// </summary>
			/// <param name="model"></param>
			/// <param name="owner"></param>
			public CelestialBodyContext(EnvironmentModel model, Model owner)
				: base(model, owner)
			{
				ownerBody = Body.GetBody(Owner);
				body = Model as CelestialBody;
			}

			/// <summary>
			/// Do nothing for now.
			/// </summary>
			public override void Execute()
			{
				ecef.Calculate(Owner as IBody);
			}

			#region IGravityContext Members

			/// <summary>
			/// Central body's position wrt/ perturbing body, [m]
			/// </summary>
			public Vector3 InertialToPerturbR { get { return body.bodyFrame.Location; } }
			/// <summary>
			/// Owner body's position wrt/ perturbing body, [m]
			/// </summary>
			public Vector3 PerturbToOwnerR { get { return ownerBody.EciR; } }
			/// <summary>
			/// Owner body's unit position wrt/ perturbing body, [m]
			/// </summary>
			public Vector3 PerturbToOwnerRunit { get { return ownerBody.EciRunit; } }
			/// <summary>
			/// Position centered, fixed wrt/ central body (ECEF), [m]
			/// NOTE: only used for Earth Spherical Harmonics
			/// </summary>
			public Vector3 CenteredFixedPosition { get { return null; } }
			/// <summary>
			/// Distance from perturbing body to central body, [m]
			/// </summary>
			public double InertialToPerturbRmag { get { return 0; } }
			/// <summary>
			/// Distance from perturbing body to owner body, [m]
			/// </summary>
			public double PerturbToOwnerRmag { get { return ownerBody.EciRmag; } }

			#endregion

		}
		/// <summary>
		/// returns a model context for this environmental model
		/// and the model passed in
		/// </summary>
		/// <param name="owner">The owning Component of the EnvironmentContext.</param>
		/// <param name="context">The containing EnvironmentContext. Used to defer creation until PostDiscover.</param>
		/// <returns>A new ModelContext or null if deferred until PostDiscover</returns>
		public override EnvironmentModelContext CreateModelContext(Model owner, EnvironmentContext context)
		{
			return new CelestialBodyContext(this, owner);
		}

		#endregion
	}
}
