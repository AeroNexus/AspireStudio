using System;
using System.ComponentModel;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Primitives;
//using Aspire.Utilities;

namespace Aspire.Space
{
	public class Sun : EnvironmentModel, IHaveFrames, ICelestialBody, IVisualInfoProvider, IFieldOfView
	{
		const string category = "Sun";

		#region Declarations

		AstroClock clock;
		FrameList frames;	// created in Discover
		Earth earth;
		PositionCtx positionCtx = new PositionCtx(0);

		[Blackboard]
		DynamicFrame bodyFrame;	// created in Discover

		/// <summary>
		/// Field of View geometry
		/// </summary>
		FieldOfView fieldOfView = new FieldOfView();

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

			frames = new FrameList() { Name = this.Name };
			bodyFrame = new DynamicFrame("body", frames);
			bodyFrame.AngularRate = new Vector3(0, 0, 0);
			//bodyFrame.Alias = "body";
			bodyFrame.RelativeVelocity = new Vector3();
			bodyFrame.Location = positionCtx.pos;
		}

		public override void Initialize()
		{
			var env = Parent as Environment;
			if (env != null)
				earth = env.CelestialBody("Earth") as Earth;
			else
				earth = ModelMgr.Model("Earth") as Earth;

			Execute();
			// For now, only initialize velocity. Need to put in flag to update it.
			bodyFrame.RelativeVelocity = bodyFrame.Location - ComputePosition(clock.ModifiedJulianDate - 1 * Constant.DayPerSec);
			base.Initialize();
		}

		public override void Execute()
		{
			positionCtx.j2000Century = clock.J2000CenturyTT;
			positionCtx.sinEcliptic = earth.SinEcliptic;
			positionCtx.cosEcliptic = earth.CosEcliptic;
			ComputePosition(ref positionCtx);
			//bodyFrame.Location.Assign( positionCtx.pos );

			// Geocentric (Earth) lat and lon of sun
			Vector3 pos = positionCtx.pos;
			earthLatitude = Math.Asin(pos.Z / positionCtx.sunEarthDistance) * Constant.DegPerRad;
			double rtAscension = Math.Atan2(pos.Y, pos.X);
			earthLongitude = Limit.Fold180((rtAscension - clock.GreenwichHourAngle) * Constant.DegPerRad);
			base.Execute();
		}

		#endregion

		// --------------------------------------------------------------------------- }
		// This function calculates the Geocentric Equatorial position vector for
		// the Sun given the Modified Julian Date.  This is the low precision formula and
		// is valid for years from 1950 to 2050.  Accuaracy of apparent coordinates
		// is 0.01 degrees.  Notice many of the calculations are performed in
		// degrees, and are not changed until later.  This is due to the fact that
		// the Almanac uses degrees exclusively in their formulations.
		//
		// Algorithm     : Calculate the several values needed to find the vector
		//                 Be careful of quadrant checks
		//
		// Author        : Capt Dave Vallado  USAFA/DFAS  719-472-4109  25 Aug 1988
		//
		// Inputs        :
		//   dt          - date and time
		//
		// Outputs       :
		//   pos         - IJK Position vector of the Sun (in meters)
		//
		// Locals        :
		//   MeanLong    - Mean Longitude
		//   MeanAnomaly - Mean anomaly
		//   N           - Number of days from 1 Jan 2000
		//   EclpLong    - Ecliptic longitude
		//   Obliquity   - Mean Obliquity of the Ecliptic
		//
		// Coupling      :
		//   fmod        - double precision MOD function
		//   asin        - Arc Sine function
		//
		// References             :
		//   1987 Astronomical Almanac Pg. C24
		//
		//     *****************  NOTICE OF GOVERNMENT ORIGIN  ****************
		//
		//  This software has been developed by an employee of the United States
		//  Government at the United States Air Force Academy, and is therefore
		//  a work of the United States, and is NOT subject to copyright protection
		//  under the provisions of 17 U.S.C. 105.  ANY use of this work, or
		//  inclusion in other works, must comply with the notice provisions of
		//  17 U.S.C. 403.
		//
		//            12 Apr 93 Dave Vallado
		//                        Fixes to Cowells and misc
		//---------------------------------------------------------------------------
		/// <summary>
		/// Compute the position of the sun in meters in ECI coordinates.
		/// <P>Medium fidelity model from Vallado.</P>
		/// </summary>
		/// <example>To compute the sun's position at 28 Apr 1994 00:00 UT1:<p><code>Primitives.Vector3 sunPosition = Space.Sun.ComputePosition(49470.0);</code></p></example>
		/// <param name="mjd">Modified Julian Date for which to compute position.</param>
		/// <returns>ECI position in meters.</returns>
		public static Vector3 ComputePosition(double mjd)
		{
			PositionCtx ctx = new PositionCtx(AstroClock.J2000CenturyOf(mjd));
			return ComputePosition(ref ctx);
		}
		/// <summary>
		/// Compute the position of the sun in meters in ECI coordinates.
		/// <P>Medium fidelity model from Vallado.</P>
		/// </summary>
		/// <example>To compute the sun's position at 28 Apr 1994 00:00 UT1:<p><code>Primitives.Vector3 sunPosition = Space.Sun.ComputePosition(49470.0);</code></p></example>
		/// <param name="ctx">Collection of parameters.</param>
		/// <returns>ECI position [m].</returns>
		private static Vector3 ComputePosition(ref PositionCtx ctx)
		{
			// Vallado, pg 183
			double Tu = ctx.j2000Century;
			double meanlon = Limit.Fold360(280.4606184 + 36000.77005361 * Tu);//deg
			double meananom = Limit.FoldTwoPi((357.5277233 + 35999.05034 * Tu) * Constant.RadPerDeg); //rad
			ctx.eclipticLongitude = (meanlon + 1.914666471 * Math.Sin(meananom) +
				0.019994643 * Math.Sin(2 * meananom)) * Constant.RadPerDeg; //rad
			ctx.sunEarthDistance = (1.000140612 - 0.016708617 * Math.Cos(meananom) -
				0.000139589 * Math.Cos(2 * meananom)) * Constant.MperAU; //m
			ctx.cosEclipticLongitude = Math.Cos(ctx.eclipticLongitude);
			ctx.sinEclipticLongitude = Math.Sin(ctx.eclipticLongitude);
			ctx.posUnit.X = ctx.cosEclipticLongitude;
			ctx.posUnit.Y = ctx.cosEcliptic * ctx.sinEclipticLongitude;
			ctx.posUnit.Z = ctx.sinEcliptic * ctx.sinEclipticLongitude;
			ctx.pos = ctx.posUnit * ctx.sunEarthDistance;
			return ctx.pos;
		}

		#region Properties

		/// <summary>
		/// Distance from sun to earth, [m]
		/// </summary>
		//[Blackboard(Units = "m")]
		[Description("Distance from sun to earth, [m]")]
		public double SunEarthDistance { get { return positionCtx.sunEarthDistance; } }

		internal double EclipticLongitude { get { return positionCtx.eclipticLongitude; } }
		internal double CosEclipticLongitude { get { return positionCtx.cosEclipticLongitude; } }
		internal double SinEclipticLongitude { get { return positionCtx.sinEclipticLongitude; } }

		/// <summary>
		/// Earth latitude of Sun vector
		/// </summary>
		[Category(category)]
		[Description("Geocentric latitude [deg] + North")]
		public double Latitude
		{
			get { return earthLatitude; }
			set { earthLatitude = value; }
		} double earthLatitude;

		/// <summary>
		/// Earth longitude of Sun vector
		/// </summary>
		[Category(category)]
		[Description("Longitude [deg] + East")]
		public double Longitude
		{
			get { return earthLongitude; }
			set { earthLongitude = value; }
		} double earthLongitude;

		#endregion

		#region Context
		/// <summary>
		/// 
		/// </summary>
		public class SunContext : EnvironmentModelContext, ISun, I_NBodyGravity
		{
			#region Declarations

			//IBody body;
			//Sun sun;
			//EcefState ecef = new EcefState();
			IBody ownerBody;
			Frame scBodyFrame, scLvlhFrame;
			Sun sun;

			/// <summary/>
			//[Blackboard("sunToOwnerRmag", Description = "sunToOwnerRmag to sun from the body", Units = "m")]
			protected double sunToOwnerRmag;
			/// <summary/>
			//[Blackboard("Fraction", Description = "Fraction of sun visible at the body")]
			protected double visibleFraction;
			/// <summary/>
			//[Blackboard(Description = "Unit vector from body to sun in body frame")]
			protected Vector3 RunitBody = new Vector3().Identity();
			/// <summary/>
			//[Blackboard(Description = "Sun unit vector from body to sun in the LVLH frame", Units = "m")]
			protected Vector3 RunitLvlh = new Vector3().Identity();
			/// <summary>
			/// The angle that the spacecraft position vector makes with the
			/// sun's position vector projected to the eclipse side of the
			/// earth. All in the orbital plane.[deg]
			/// </summary>
			//[Blackboard(Description = "Sun alpha angle", Units = "deg")]
			protected double alpha;
			/// <summary/>
			//[Blackboard(Description = "cos(Sun alpha angle)")]
			protected double cosAlpha;
			/// <summary>
			/// Angle between the sun vector [ECI} and the orbital plane.
			/// </summary>
			//[Blackboard(Description = "Sun beta angle", Units = "deg")]
			protected double beta;
			/// <summary/>
			//[Blackboard(Description = "Local satellite time", Units = "hours")]
			protected double satelliteTime;

			private Vector3 sunToOwner = new Vector3();
			private Vector3 sunToOwnerUnit = new Vector3();
			private Vector3 ownerToSunUnit = new Vector3();

			#endregion

			public SunContext(){}

			/// <summary>
			/// Constructor to set model and owner
			/// </summary>
			/// <param name="model"></param>
			/// <param name="owner"></param>
			public SunContext(EnvironmentModel model, Model owner)
				: base(model, owner)
			{
				ownerBody = Body.GetBody(Owner);
				if (ownerBody == null)
				{
					Enabled = false;
					return;
				}
				var sf = ownerBody as IHaveFrames;
				scBodyFrame = sf.GetFrame("body");
				sun = model as Sun;
			}

			#region Model implementation
			/// <summary>
			/// Discovery phase
			/// </summary>
			/// <param name="cfg"></param>
			public override void Discover(Scenario scenario)
			{
				base.Discover(scenario);
				scLvlhFrame = (Owner as IHaveFrames).GetFrame("lvlh");
			}

			/// <summary>
			/// Initialization phase
			/// </summary>
			public override void Initialize()
			{
				Execute();
			}

			/// <summary>
			/// Do nothing for now.
			/// </summary>
			public override void Execute()
			{
				sunToOwner = sun.positionCtx.pos - ownerBody.EciR;
				sunToOwnerUnit = sunToOwner;
				sunToOwnerUnit.Normalize(out sunToOwnerRmag);
				ownerToSunUnit *= -1;

				double cosTheta = sunToOwnerUnit.Dot(ownerBody.EciRunit);
				if (scBodyFrame != null)
					RunitBody = scBodyFrame.FromParentDcm * ownerToSunUnit;

				// Find the visible fraction of the sun
				double theta = Math.Acos(cosTheta);
				double sinRe = Math.Min(Constant.EarthEquatorialRadius / ownerBody.EciRmag, 1);
				double radiusEarth = Math.Asin(sinRe);
				double sinRs = Constant.SunRadius / sunToOwnerRmag;
				double radiusSun = sinRs > 0.01 ? Math.Asin(sinRs) : sinRs;

				if (theta > (radiusEarth + radiusSun))
					visibleFraction = 1.0;
				else if (theta < (radiusEarth - radiusSun))
					visibleFraction = 0.0;
				else
				{
					double cosRs = Math.Sqrt(1.0 - sinRs * sinRs);
					double cosRe = Math.Sqrt(1.0 - sinRe * sinRe);
					double sinTheta = Math.Sin(theta);

					double overlapArea = Constant.Pi - cosRs * Math.Acos((cosRe - cosRs * cosTheta) / sinRs / sinTheta)
						- cosRe * Math.Acos((cosRs - cosRe * cosTheta) / sinRe / sinTheta)
						- Math.Acos((cosTheta - cosRs * cosRe) / sinRs / sinRe);
					visibleFraction = 1.0 - overlapArea / (Constant.Pi * (1.0 - cosRs));
				}

				// Calculate Sun Alpha and Beta Angles
				if (scLvlhFrame != null)
				{
					//ToDo:Verify Sun alpha/beta/sat time
					RunitLvlh = scLvlhFrame.FromParentDcm * sun.positionCtx.posUnit;
					beta = Constant.DegPerRad * Math.Asin(-RunitLvlh[1]);
					alpha = Math.Atan2(RunitLvlh[0], RunitLvlh[2]);
					cosAlpha = RunitLvlh[2] / Math.Sqrt(RunitLvlh[0] * RunitLvlh[0] + RunitLvlh[2] * RunitLvlh[2]);
					alpha = Limit.Fold360(Constant.DegPerRad * alpha);
					satelliteTime = alpha * 24.0 / 360.0;
				}
			}
			#endregion

			#region ISun Members

			/// <summary>
			/// Access the alpha angle, which is the angle that the 
			/// spacecraft position vector makes with the sun's position vector
			/// projected to the eclipse side of the earth. All in the orbital
			/// plane.[deg]
			/// </summary>
			[Blackboard(Units = "deg")]
			public double Alpha { get { return alpha; } }

			/// <summary>
			/// Position vector in inertial frame, [m]] 
			/// </summary>
			public Vector3 EciR { get { return sun.positionCtx.pos; } }

			/// <summary>
			/// Position vector in inertial frame, [m]] 
			/// </summary>
			public Vector3 EciRunit { get { return sun.positionCtx.posUnit; } }

			/// <summary>
			///  Position unit vector in EnvironmentContext's body frame
			/// </summary>
			public Vector3 UnitBody { get { return RunitBody; } }

			/// <summary>
			///  Visible fraction. 0=occluded, 1=fully visible
			/// </summary>
			[Blackboard(Units="0-1")]
			public double Visible { get { return visibleFraction; } }

			/// <summary>
			/// Beta angle, [deg].  Equivalent to inclination with body's orbital plane.
			/// </summary>
			[Blackboard(Units = "deg")]
			public double Beta { get { return beta; } }

			/// <summary>
			/// Equivalent alpha, expressed in a 24 hour clock hour [0-24 hours]
			/// </summary>
			[Blackboard]
			public double SatelliteTime { get { return satelliteTime; } }

			/// <summary>
			///  Cos(alpha), useful for temperature profiles.
			/// </summary>
			public double CosAlpha { get { return cosAlpha; } }

			#endregion

			#region IGravityContext Members

			/// <summary>
			/// Central body's position wrt/ perturbing body, [m]
			/// </summary>
			public Vector3 InertialToPerturbR { get { return sun.positionCtx.pos; } }
			/// <summary>
			/// Owner body's position wrt/ perturbing body, [m]
			/// </summary>
			public Vector3 PerturbToOwnerR { get { return sunToOwner; } }
			/// <summary>
			/// Owner body's unit position wrt/ perturbing body, [m]
			/// </summary>
			public Vector3 PerturbToOwnerRunit { get { return sunToOwnerUnit; } }
			/// <summary>
			/// Position centered, fixed wrt/ central body (ECEF), [m]
			/// NOTE: only used for Earth Spherical Harmonics
			/// </summary>
			public Vector3 CenteredFixedPosition { get { return null; } }
			/// <summary>
			/// Distance from perturbing body to central body, [m]
			/// </summary>
			public double InertialToPerturbRmag { get { return sun.positionCtx.sunEarthDistance; } }
			/// <summary>
			/// Distance from perturbing body to owner body, [m]
			/// </summary>
			public double PerturbToOwnerRmag { get { return sunToOwnerRmag; } }

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
			return new SunContext(this, owner);
		}

		#endregion

		#region PositionCtx
		private struct PositionCtx
		{
			internal double
				sunEarthDistance,
				eclipticLongitude,
				sinEclipticLongitude,
				cosEclipticLongitude,
				sinEcliptic,
				cosEcliptic,
				j2000Century;
			internal Vector3 pos, posUnit;

			internal PositionCtx(double j2Kcentury)
			{
				j2000Century = j2Kcentury;
				double ecliptic = Earth.AngleOfEcliptic(j2000Century) * Constant.RadPerDeg;
				sinEcliptic = Math.Sin(ecliptic);
				cosEcliptic = Math.Cos(ecliptic);
				eclipticLongitude = sunEarthDistance = sinEclipticLongitude = cosEclipticLongitude = 0;
				pos = new Vector3();
				posUnit = new Vector3();
			}
		}
		#endregion

		#region ICelestialBody Members

		// Position is provided by IVsualEntity

		/// <summary>
		/// Radius of the body, [m]
		/// </summary>
		public double Radius { get { return Constant.SunRadius; } }

		/// <summary>
		/// Radius at the poles of the body, [m]
		/// </summary>
		public double PolarRadius { get { return Constant.SunRadius; } }

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
		public double Mass { get { return Constant.SunMass; } }

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
		[Category("Visual Entity"),DefaultValue("Sun.xml"),XmlAttribute("visualFileName")]
		[Description("File name of visual model characteristics")]
		public string VisualFileName
		{
			get { return visualFileName; }
			set { visualFileName = value; }
		} string visualFileName = "Sun.xml";

		#endregion
	}
}
