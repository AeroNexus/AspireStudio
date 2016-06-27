using System;
using System.ComponentModel;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Primitives;
//using Aspire.Utilities;

namespace Aspire.Space
{
	public class Moon : EnvironmentModel, IHaveFrames, ICelestialBody, IFieldOfView, IVisualInfoProvider
	{
		#region Definitiona
		/// <summary>
		/// The eight phases of the moon.
		/// </summary>
		public enum Phase
		{
			/// <summary>
			/// The disk is fully dark.
			/// </summary>
			New,
			/// <summary>
			/// Crescent after New.
			/// </summary>
			WaxingCrescent,
			/// <summary>
			/// The disk is half lit, on it way to Full.
			/// </summary>
			FirstQuarter,
			/// <summary>
			/// Almost fully lit.
			/// </summary>
			WaxingGibbous,
			/// <summary>
			/// The disk is fully lit.
			/// </summary>
			Full,
			/// <summary>
			/// Almost lit, nut waning.
			/// </summary>
			WaningGibbous,
			/// <summary>
			/// The disk is half lit on its way to New.
			/// </summary>
			LastQuarter,
			/// <summary>
			/// Crescent before New.
			/// </summary>
			WaningCrescent
		}
		#endregion

		#region Declarations

		AstroClock clock;
		[Blackboard]
		DynamicFrame bodyFrame;	// created in Discover
		FieldOfView fieldOfView = new FieldOfView();
		FrameList frames;	// created in Discover
		PositionCtx positionCtx = new PositionCtx(0);
		Sun sun;

		[Blackboard(Units = "m")]
		double MoonEarthDistance { get { return positionCtx.distance; } }

		[Blackboard(Units = "kg")]
		const double mass = Constant.MoonMass;

		[Blackboard(Units = "m")]
		const double radius = Constant.MoonRadius;

		/// <summary>
		/// Visible fraction that is lit.
		/// </summary>
		[Description("Visible fraction that is lit.")]
		[Blackboard]
		protected double visible;

		/// <summary>
		/// Which of the eight phases is the moon in.
		/// </summary>
		[Description("Which of the eight phases is the moon in.")]
		[Blackboard]
		protected Phase phase;

		#endregion

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			clock = scenario.Clock.GetService(typeof(AstroClock)) as AstroClock;

			if (!(Parent is Environment))
				Environment.AddModel(this);

			fieldOfView.Radius = 45;
			fieldOfView.Changed += new EventHandler(fieldOfView_Changed);

			base.Discover(scenario);

			frames = new FrameList(){Name = this.Name};
			bodyFrame = new DynamicFrame("mcmf", frames);
			bodyFrame.AngularRate = new Vector3(0, 0, Constant.MoonRate);
			bodyFrame.Alias = "body";
			bodyFrame.RelativeVelocity = new Vector3();
			bodyFrame.Location = positionCtx.pos;
			Initialize();
		}

		public override void Initialize()
		{
			var env = Parent as Environment;
			sun = env.CelestialBody("Sun") as Sun;
			
			Execute();
			// For now, only initialize velocity. Need to put in flag to update it.
			bodyFrame.RelativeVelocity = 
				bodyFrame.Location - ComputePosition(clock.ModifiedJulianDate - 1 * Constant.DayPerSec);
			base.Initialize();
		}

		public override void Execute()
		{
			positionCtx.j2000Century = clock.J2000CenturyTT;
			ComputePosition(ref positionCtx);
			//bodyFrame.Location.Assign( positionCtx.Rmoon );

			// Geocentric (Earth) lat and lon
			Vector3 pos = positionCtx.pos;
			earthLatitude = Math.Asin(pos.Z / positionCtx.distance) * Constant.DegPerRad;
			double rtAscension = Math.Atan2(pos.Y, pos.X);
			earthLongitude = Limit.Fold180((rtAscension - clock.GreenwichHourAngle) * Constant.DegPerRad);

			//// Allen's Astrophysical Quantities, pg. 303:
			//// http://books.google.com/books?id=w8PK2XFLLH8C&pg=PA303&lr=&as_brr=3&sig=ACfU3U0q22fq_E1G1rWLJDZAa9fxr26Edg#PPA303,M1
			////const double siderealRotationPeriodInDays = 27.321661;
			//// Note:
			//// This is the average rotation rate for the moon.  In reality, the actual rotation rate varies around
			//// this average causing a slight "wobble" as seen from the earth.  This is not yet being modeled here.
			//// This "wobble" is one of several "libration" effects.  Most of the other libration effects are simply
			//// due to the moon's orbital path or the position of the viewer... both of which are handled automatically
			//// by the graphics engine. Also, this rotates the moon about Earth's Z direction, not about the Moon's
			//// Z direction.
			////bodyFrame.FromParentDcm = bodyFrame.FromParentDcm.Identity().RotationFromDegrees("z", RotationOffsetDegrees + clock.JulianEpochDate*360/siderealRotationPeriodInDays);
			////Vector3 eulertemp = new Vector3();
			////bodyFrame.FromParentDcm.ToEuler(eulertemp, "312");

			// Rather brute force computation of orientation.  Ignores all variations
			// in orientation.  Just forces moon's X axis toward earth and moon's Z
			// axis along the moon's orbit normal direction.
			Vector3 vunit = new Vector3();
			Vector3.CrossProduct(vunit, OrbitalAngularMomentumUnit, positionCtx.posUnit);
			bodyFrame.FromParentDcm.SetRow(0, -positionCtx.posUnit);
			bodyFrame.FromParentDcm.SetRow(1, -vunit);
			bodyFrame.FromParentDcm.SetRow(2, OrbitalAngularMomentumUnit);

			// using sun, get ecliptic longitude. take difference in
			// ecliptic longitude to get moon visible fraction and phase
			// Vallado, p282
			if (sun == null) return;
			double phaseAngle = sun.EclipticLongitude - positionCtx.eclipticLongitude;
			visible = 0.5 * (1.0 - Math.Cos(phaseAngle));
			phase = (Phase)(int)(Limit.Fold360(phaseAngle * Constant.DegPerRad + 22.5) / 45.0);

			base.Execute();
		}

		#endregion

		#region Position context
		private struct PositionCtx
		{
			internal double
				distance,
				eclipticLongitude,
				j2000Century;
			internal Vector3 pos, posUnit;

			internal PositionCtx(double j2Kcentury)
			{
				j2000Century = j2Kcentury;
				eclipticLongitude = distance = 0;
				pos = new Vector3();
				posUnit = new Vector3();
			}
		}
		#endregion

		// --------------------------------------------------------------------------- }
		// This function calculates the Geocentric Equatorial (IJK) position
		// vector for the moon given the Modified Julian Date.  This is the low
		// precision formula and is valid for years between 1950 and 2050.
		// Notice many of the calculations are performed in degrees.  This
		// coincides with the development in the Almanac.
		// The program results are as follows:
		//   (not sure what these mean, perhaps they are accuracy limits?)
		//               Eclpitic Longitude  0.3   degrees
		//               Eclpitic Latitude   0.2   degrees
		//               Horiz Parallax      0.003 degrees
		//               Distance from Earth 0.2   DUs
		//               Right Ascension     0.3   degrees
		//               Declination         0.2   degrees
		//
		//Author        : Capt Dave Vallado  USAFA/DFAS  719-472-4109  25 Aug 1988
		//              Converted to C++, Mark V. Tollefson, 1995
		//              Converted to use my classes, Tollefson, 1999
		//              Converted to C#, jd->mjd, Tollefson 2008
		//
		//Inputs        :
		//  dt          a datetime object
		//  (JD          - Julian Date, days from 4713 B.C. -- former input)
		//
		//Outputs       :
		//  RMoon       - IJK Position vector of the Moon in meters
		//                Originally returned units of DU.  As used here,
		//                the DU appears to be one earth radius.
		//
		//Locals        :
		//  EclpLong    - Ecliptic Longitude
		//  EclpLat     - Eclpitic Latitude
		//  HzParal     - Horizontal Parallax
		//  l           - Geocentric Direction Cosines
		//  m           -             "     "
		//  n           -             "     "
		//  Tu          - Julian Centuries from 1 Jan 1900
		//  x           - Temporary REAL variable
		//
		//References    :
		//  1987 Astronomical Almanac Pg. D46
		//  Explanatory Supplement(1960) pg. 106-111
		//  Roy, Orbital Motion Pg. 61-62 (Discussion of parallaxes)
		//
		/// <summary>
		/// Compute the position of the moon in meters in ECI coordinates.
		/// <P>Medium fidelity model from Vallado.</P>
		/// </summary>
		/// <example>To compute the moon's position at 28 Apr 1994 00:00 UT1:
		/// <p><code>
		/// Primitives.Vector3 moonPosition = Space.Moon.ComputePosition(49470.0);
		/// </code></p></example>
		/// <param name="mjd">Modified Julian Date for which to compute position.</param>
		/// <returns>ECI position in meters.</returns>
		public static Vector3 ComputePosition(double mjd)
		{
			PositionCtx ctx = new PositionCtx(AstroClock.J2000CenturyOf(mjd));
			return ComputePosition(ref ctx);
		}

		/// <summary>
		/// Compute the position of the moon in meters in ECI coordinates.
		/// <P>Medium fidelity model from Vallado.</P>
		/// </summary>
		/// <example>To compute the moon's position at 28 Apr 1994 00:00 UT1:<p>
		/// <code>Primitives.Vector3 moonPosition = Space.Moon.ComputePosition(49470.0);</code>
		/// </p></example>
		/// <param name="ctx">Collection of parameters.</param>
		/// <returns>ECI position in meters.</returns>
		private static Vector3 ComputePosition(ref PositionCtx ctx)
		{
			// ---------------------------------------------------------------------------
			double EclpLat;
			double HzParal;
			// --------------------  Initialize values   -------------------
			double Tu = ctx.j2000Century;
			const double dtr = Constant.RadPerDeg;

			double x = 218.32 + 481267.883 * Tu
				+ 6.29 * Math.Sin((134.9 + 477198.85 * Tu) * dtr)
				- 1.27 * Math.Sin((259.2 - 413335.38 * Tu) * dtr)
				+ 0.66 * Math.Sin((235.7 + 890534.23 * Tu) * dtr);

			ctx.eclipticLongitude = x + 0.21 * Math.Sin((269.9 + 954397.70 * Tu) * dtr)
				- 0.19 * Math.Sin((357.5 + 35999.05 * Tu) * dtr)
				- 0.11 * Math.Sin((186.6 + 966404.05 * Tu) * dtr);  // Deg

			EclpLat = 5.13 * Math.Sin((93.3 + 483202.03 * Tu) * dtr)
				+ 0.28 * Math.Sin((228.2 + 960400.87 * Tu) * dtr)
				- 0.28 * Math.Sin((318.3 + 6003.18 * Tu) * dtr)
				- 0.17 * Math.Sin((217.6 - 407332.20 * Tu) * dtr);  // Deg

			x = 0.9508 + 0.0518 * Math.Cos((134.9 + 477198.85 * Tu) * dtr);

			HzParal = x + 0.0095 * Math.Cos((259.2 - 413335.38 * Tu) * dtr)
				+ 0.0078 * Math.Cos((235.7 + 890534.23 * Tu) * dtr)
				+ 0.0028 * Math.Cos((269.9 + 954397.70 * Tu) * dtr); // Deg

			ctx.eclipticLongitude = Limit.FoldTwoPi(ctx.eclipticLongitude * dtr);
			EclpLat = Limit.FoldTwoPi(EclpLat * dtr);
			HzParal = Limit.FoldTwoPi(HzParal * dtr);

			// Find the geocentric direction cosines
			ctx.posUnit[0] = Math.Cos(EclpLat) * Math.Cos(ctx.eclipticLongitude);
			double cosEclpLat = Math.Cos(EclpLat);
			double sinEclpLat = Math.Sin(EclpLat);
			double sinEclpLon = Math.Sin(ctx.eclipticLongitude);
			ctx.posUnit[1] = 0.9175 * cosEclpLat * sinEclpLon - 0.3978 * sinEclpLat;
			ctx.posUnit[2] = 0.3978 * cosEclpLat * sinEclpLon + 0.9175 * sinEclpLat;

			// Calculate position
			// following constant probably must remain to be consistent with
			// the constants in the formulas above
			const double er = 6378135.0;    // earth radius in m
			ctx.distance = er * 1.0 / Math.Sin(HzParal);
			ctx.pos = ctx.posUnit * ctx.distance;

			// --------------- Find Rt Ascension and Declination -----------
			// Following computations are correct, but were not needed.
			// They are left in for possible future use.
			//RtAsc= ATan2( m,l );
			//Decl= Arcsin( n );
			return ctx.pos;
		}

		#region Properties

		// Nore: Lat/Lon are used in the Mercator, which used to use IVisualEntity. That is now IVisualInfoProvider,
		// but I want to re-work the lat/lon interface, so Lat/Lon are not currently in IVisualInfoProvider
		/// <summary>
		/// Earth latitude of moon
		/// </summary>
		[XmlIgnore]
		[Category("Visual Entity")]
		[Description("Geocentric latitude [deg] + North")]
		public double Latitude
		{
			get { return earthLatitude; }
			set { earthLatitude = value; }
		} double earthLatitude;

		/// <summary>
		/// Earth longitude of moon
		/// </summary>
		[XmlIgnore]
		[Category("Visual Entity")]
		[Description("Longitude [deg] + East")]
		public double Longitude
		{
			get { return earthLongitude; }
			set { earthLongitude = value; }
		} double earthLongitude;

		/// <summary>
		/// Good approximation of Orbital Angular Momentum unit vector.
		/// </summary>
		public Vector3 OrbitalAngularMomentumUnit
		{
			get
			{
				// on first use, compute the value
				if (m_OrbitalAngularMomentumUnit.IsZero)
				{
					// following date is arbitrarily 01 Jan 2020
					var ctx = new PositionCtx(AstroClock.J2000CenturyOf(58849));
					var pos1 = ComputePosition(ref ctx);
					ctx = new PositionCtx(AstroClock.J2000CenturyOf(58849 + 7)); // one week later
					var pos2 = ComputePosition(ref ctx);
					m_OrbitalAngularMomentumUnit = new Vector3();
					Vector3.CrossProduct(m_OrbitalAngularMomentumUnit, pos1, pos2);
					m_OrbitalAngularMomentumUnit.Normalize();
				}
				return m_OrbitalAngularMomentumUnit;
			}
		}
		private Vector3 m_OrbitalAngularMomentumUnit = new Vector3();
		/// <summary>
		/// Initial rotation at JulianDateEpoch2000.
		/// </summary>
		//public double RotationOffsetDegrees { get; set; }

		#endregion

		#region ICelestialBody Members

		/// <summary>
		///
		/// </summary>
		public Dcm BodyToInertialDcm { get { return bodyFrame.ToParentDcm; } }

		/// <summary>
		///
		/// </summary>
		public Dcm InertialToBodyDcm { get { return bodyFrame.FromParentDcm; } }

		/// <summary>
		///Mass used to calculate GM
		/// </summary>
		public double Mass { get { return Constant.MoonMass; } }

		// Position is provided by IVsualEntity

		/// <summary>
		/// Radius at the poles of the body, [m]
		/// </summary>
		public double PolarRadius { get { return Constant.MoonRadius; } }

		/// <summary>
		/// Radius of the body, [m]
		/// </summary>
		public double Radius { get { return Constant.MoonRadius; } }

		/// <summary>
		/// 
		/// </summary>
		public Vector3 Velocity { get { return bodyFrame.RelativeVelocity; } }

		#endregion

		#region IFieldOfView Members

		/// <summary>
		/// FovChanged is raised whenever the FOV's geometry changes
		/// </summary>
		public event EventHandler FovChanged;

		/// <summary>
		/// Access the geometry of a FOV.
		/// </summary>
		public FieldOfView FieldOfView
		{
			get { return fieldOfView; }
			set { fieldOfView = value; }
		}

		/// <summary>
		/// Inertial boresight unit vector
		/// </summary>
		[Blackboard(Description = "Inertial boresight unit vector")]
		public Vector3 FovBoresight
		{
			get
			{
				fovBoresight = -positionCtx.posUnit;
				return fovBoresight;
			}
		}
		Vector3 fovBoresight = new Vector3();

		/// <summary>
		/// Inertial position, [m]
		/// </summary>
		[Blackboard(Description = "Inertial position", Units = "m")]
		public Vector3 FovPosition { get { return positionCtx.pos; } }

		/// <summary>
		/// Inertial up unit vector, ALWAYS perpendicular to the boresight
		/// </summary>
		[Blackboard(Description = "Inertial up unit vector, ALWAYS perpendicular to the boresight")]
		public Vector3 FovUp
		{
			get
			{
				normal.CrossProduct(upReference, positionCtx.posUnit);
				fovUp.CrossProduct(positionCtx.posUnit, normal);
				return fovUp;
			}
		}
		Vector3 fovUp = new Vector3(), upReference = new Vector3(0, 0, 1), normal = new Vector3();

		private void fieldOfView_Changed(object sender, EventArgs e)
		{
			if (FovChanged != null)
				FovChanged(this, EventArgs.Empty);
		}

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
		[Category("Visual Entity"),DefaultValue("Moon.xml"),XmlAttribute("visualFileName")]
		[Description("File name of visual model characteristics")]
		public string VisualFileName
		{
			get { return visualFileName; }
			set { visualFileName = value; }
		} string visualFileName = "Moon.xml";

		#endregion

		#region Context
		/// <summary>
		/// 
		/// </summary>
		public class MoonContext : EnvironmentModelContext, IMoon, I_NBodyGravity
		{
			IBody ownerBody;
			Moon moon;
			[Blackboard]
			Vector3 moonToOwnerR = new Vector3();
			[Blackboard]
			Vector3 moonToOwnerRunit = new Vector3();
			[Blackboard]
			double distance;

			public MoonContext() { }

			/// <summary>
			/// Constructor to set model and owner
			/// </summary>
			/// <param name="model"></param>
			/// <param name="owner"></param>
			public MoonContext(EnvironmentModel model, Model owner)
				: base(model, owner)
			{
				ownerBody = Body.GetBody(Owner);
				moon = Model as Moon;
			}

			#region Model implementation
			/// <summary>
			/// Initialization phase
			/// </summary>
			public override void Initialize()
			{
				Execute();
				base.Initialize();
			}

			/// <summary>
			/// Calculate moon to owner geometry
			/// </summary>
			public override void Execute()
			{
				moonToOwnerR = ownerBody.EciR - moon.Position;
				moonToOwnerRunit = moonToOwnerR;
				moonToOwnerRunit.Normalize(out distance);
				base.Execute();
			}
			#endregion

			#region IMoon Members

			public Moon GetMoon()
			{
				return Model as Moon;
			}

			#endregion

			#region IGravityContext Members

			/// <summary>
			/// Central body's position wrt/ perturbing body, [m]
			/// </summary>
			public Vector3 InertialToPerturbR { get { return moon.positionCtx.pos; } }
			/// <summary>
			/// Owner body's position wrt/ perturbing body, [m]
			/// </summary>
			public Vector3 PerturbToOwnerR { get { return moonToOwnerR; } }
			/// <summary>
			/// Owner body's unit position wrt/ perturbing body, [m]
			/// </summary>
			public Vector3 PerturbToOwnerRunit { get { return moonToOwnerRunit; } }
			/// <summary>
			/// Position centered, fixed wrt/ central body (ECEF), [m]
			/// NOTE: only used for Earth Spherical Harmonics
			/// </summary>
			public Vector3 CenteredFixedPosition { get { return null; } }
			/// <summary>
			/// Distance from perturbing body to central body, [m]
			/// </summary>
			public double InertialToPerturbRmag { get { return moon.positionCtx.distance; } }
			/// <summary>
			/// Distance from perturbing body to owner body, [m]
			/// </summary>
			public double PerturbToOwnerRmag { get { return distance; } }

			#endregion
		}
		/// <summary>
		/// returns a model context for this environmental model
		/// and the component passed in
		/// </summary>
		/// <param name="owner">The owning Component of the EnvironmentContext.</param>
		/// <param name="context">The containing EnvironmentContext. Used to defer creation until PostDiscover.</param>
		/// <returns>A new ModelContext or null if deferred until PostDiscover</returns>
		public override EnvironmentModelContext CreateModelContext(Model owner, EnvironmentContext context)
		{
			return new MoonContext(this, owner);
		}

		#endregion
	}
}
