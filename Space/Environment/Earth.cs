using System;
using System.ComponentModel;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Primitives;
//using Aspire.Utilities;

namespace Aspire.Space
{
	public class Earth : EnvironmentModel, IHaveFrames, ICelestialBody, IReferenceFrameModelHost
	{
		/// <summary>
		/// default constructor
		/// </summary>
		public Earth()
		{
			if (theEarth == null) theEarth = this; // for static methods
		}

		#region Declarations

		static Earth theEarth;

		const double radius = Constant.EarthEquatorialRadius;
		const double RequatorSq = Constant.EarthEquatorialRadius * Constant.EarthEquatorialRadius;
		const double RpolarSq = Constant.EarthPolarRadius * Constant.EarthPolarRadius;

		AstroClock clock;
		FrameList frames;	// created in Discover
		[Blackboard]
		DynamicFrame bodyFrame;	// created in Discover
		// Can be overridden as a child model
		IReferenceFrameModel referenceFrameModel = new TrueOfDate();
		Scheduler scheduler;

		#endregion

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			clock = scenario.Clock.GetService(typeof(AstroClock)) as AstroClock;

			if (!(Parent is Environment))
				Environment.AddModel(this);

			base.Discover(scenario);

			scheduler = scenario.Executive.Scheduler;
			frames = new FrameList(){Name = this.Name};
			bodyFrame = new DynamicFrame("ecef", frames);
			bodyFrame.AngularRate = new Vector3(0, 0, Constant.EarthRate);
			bodyFrame.Alias = "body";
			bodyFrame.SetAsCurrent();
			bodyFrame.Parent.SetAsCurrent();
			referenceFrameModel.Clock = clock;
		}

		public override void Initialize()
		{
			referenceFrameModel.Initialize();
			eclipticCount = -1;
			Execute();
			base.Initialize();
		}

		public override void Execute()
		{
			// This assignment will create the transpose in the Frame.
			bodyFrame.FromParentDcm = referenceFrameModel.FromInertial(bodyFrame.FromParentDcm);

			if (scheduler != null && scheduler.FrameCount > eclipticCount)
				CalculateEclipticAngle();

			base.Execute();
		}

		#endregion

		/// <summary>
		/// Calculate the ecliptic angle as a function of J2000 century
		/// </summary>
		/// <param name="J2000Century"></param>
		public static double AngleOfEcliptic(double J2000Century)
		{
			if (theEarth != null)
				return theEarth.referenceFrameModel.EclipticAngle(J2000Century);
			else
				return TrueOfDate.eclipticAngle0;
		}

		void CalculateEclipticAngle()
		{
			eclipticAngle = referenceFrameModel.EclipticAngleDeg;// AngleOfEcliptic(clock.J2000Century);
			double ecliptic = eclipticAngle * Constant.RadPerDeg;
			sinEcliptic = Math.Sin(ecliptic);
			cosEcliptic = Math.Cos(ecliptic);
			eclipticCount = scheduler.FrameCount;
		} long eclipticCount = -1;

		#region Properties

		/// <summary>
		/// Angle of the earth's polar axis with the plane of the ecliptic, [deg]
		/// </summary>
		[Description("Angle of the earth's polar axis with the plane of the ecliptic, [deg]")]
		[Blackboard(Units = "deg")]
		public double EclipticAngle
		{
			get
			{
				if (scheduler != null && scheduler.FrameCount > eclipticCount)
					CalculateEclipticAngle();
				return eclipticAngle;
			}
		} double eclipticAngle;
		/// <summary>
		/// Sine of the EclipticAngle
		/// </summary>
		[Description("Sine of the EclipticAngle")]
		public double SinEcliptic
		{
			get
			{
				if (scheduler != null && scheduler.FrameCount > eclipticCount)
					CalculateEclipticAngle();
				return sinEcliptic;
			}
		} double sinEcliptic;
		/// <summary>
		/// Cosine of the EclipticAngle
		/// </summary>
		[Description("Cosine of the EclipticAngle")]
		public double CosEcliptic
		{
			get
			{
				if (scheduler != null && scheduler.FrameCount > eclipticCount)
					CalculateEclipticAngle();
				return cosEcliptic;
			}
		} double cosEcliptic;

		#endregion

		#region IReferenceFrameModelHost

		/// <summary>
		/// Access the IReferenceFramemodel
		/// </summary>
		[XmlIgnore]
		public IReferenceFrameModel ReferenceFrameModel
		{
			get { return referenceFrameModel; }
			set
			{
				referenceFrameModel = value;
				if (clock != null) referenceFrameModel.Clock = clock;
			}
		}

		#endregion

		#region ICelestialBody Members

		// Position is provided by IVsualEntity

		/// <summary>
		/// Radius of the body, [m]
		/// </summary>
		public double Radius { get { return radius; } }

		/// <summary>
		/// Radius at the poles of the body, [m]
		/// </summary>
		public double PolarRadius { get { return Constant.EarthPolarRadius; } }

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
		public double Mass { get { return Constant.EarthMass; } }

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
		[Category("Visual Entity"),DefaultValue("MediumResEarthWithClouds.xml"),XmlAttribute("visualFileName")]
		[Description("File name of visual model characteristics")]
		public string VisualFileName
		{
			get { return visualFileName; }
			set { visualFileName = value; }
		} string visualFileName = "MediumResEarthWithClouds.xml";

		#endregion

		#region Context
		/// <summary>
		/// 
		/// </summary>
		public class EarthContext : EnvironmentModelContext, IEarth, I_NBodyGravity
		{
			IBody ownerBody;
			Earth earth;

			public EarthContext() { }

			/// <summary>
			/// Constructor to set model and owner
			/// </summary>
			/// <param name="model"></param>
			/// <param name="owner"></param>
			public EarthContext(EnvironmentModel model, Model owner)
				: base(model, owner)
			{
				ownerBody = Body.GetBody(Owner);
				earth = Model as Earth;
				Enabled = false;
			}

			/// <summary>
			/// Do nothing for now.
			/// </summary>
			public override void Execute()
			{
				ecef.Calculate(Owner as IBody);
			}

			#region IEarth Members

			/// <summary>
			/// IEarth
			/// </summary>
			/// <returns></returns>
			public Earth GetEarth() { return earth; }

			/// <summary>
			/// Sets the ECEF context
			/// </summary>
			public EcefState Ecef
			{
				set { ecef = value; }
			}
			EcefState ecef;

			/// <summary>
			/// Earth ECEF frame
			/// </summary>
			public Frame EarthBodyFrame { get { return (Model as Earth).bodyFrame; } }

			#endregion

			#region IGravityContext Members

			/// <summary>
			/// Central body's position wrt/ perturbing body, [m]
			/// </summary>
			public Vector3 InertialToPerturbR { get { return earth.bodyFrame.Location; } }
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
		/// and the component passed in
		/// </summary>
		/// <param name="owner">The owning Component of the EnvironmentContext.</param>
		/// <param name="context">The containing EnvironmentContext. Used to defer creation until PostDiscover.</param>
		/// <returns>A new ModelContext or null if deferred until PostDiscover</returns>
		public override EnvironmentModelContext CreateModelContext(Model owner, EnvironmentContext context)
		{
			return new EarthContext(this, owner);
		}

		#endregion
	}
}
