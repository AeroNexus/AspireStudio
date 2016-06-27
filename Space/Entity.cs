using System.ComponentModel;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Primitives;
using Aspire.Utilities;

namespace Aspire.Space
{
    public class Entity : Model, IHaveFrames
    {
		/// <summary>
		/// Default constructor
		/// </summary>
		public Entity()
		{
			frames = new FrameList();
			bodyFrame = new DynamicFrame("body", frames);
			lvlhFrame = new DynamicFrame("lvlh", frames);
			Enabled = false;
		}

		#region Declarations

		const string category = "Entity";

		/// <summary>
		/// The body frame. Can be wrt Inertial or some parent frame, such as ECEF.
		/// </summary>
		protected DynamicFrame bodyFrame;

		/// <summary>
		/// The ECEF state
		/// </summary>
		protected EcefState ecef = new EcefState();

		/// <summary>
		/// The Earth frame
		/// </summary>
		protected DynamicFrame ecefFrame;

		[Blackboard]
		protected Dcm eciToBodyDcm = new Dcm();
		[Blackboard]
		protected Dcm eciToLvlhDcm = new Dcm();

		/// <summary>
		/// A derived class can access the initial euler offset directly.
		/// </summary>
		protected Vector3? eulerOffset;

		/// <summary>
		/// Scratch rotation DCM used for euler rotation
		/// </summary>
		protected Dcm eulerRotDcm = new Dcm().Identity();

		[Blackboard]
		protected Dcm localEcefDcm = new Dcm().Identity();

		/// <summary>
		/// The Local Vertical / Local Horizontal frame
		/// </summary>
		protected DynamicFrame lvlhFrame;	// created in Discover

		AstroClock clock;
		bool positionIsEcef, updateEci;

		#endregion

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			var prevFrame = bodyFrame.SetAsCurrent();

			if (frames.Name.Length == 0) frames.Name = Name;
			frames.CollectAssociatedFrames(bodyFrame);

			clock = scenario.Clock.GetService(typeof(AstroClock)) as AstroClock;

			base.Discover(scenario);

			if (Parent != null)
			{
				Enabled = false;
				if (Parent.Name.Contains("Earth"))
					positionIsEcef = true;
			}

			prevFrame.SetAsCurrent();

		}

		public override void Initialize()
		{
			SetInitialState(initialState);
			SetInitialAttitude(initialAttitude);
			Enabled = true;
			var item = Blackboard.Subscribe(".Visuals.Visible");
			if (item != null) item.Value = true;

			base.Initialize();
		}

		public override void Execute()
		{
			if (updateEci)
			{
				position = ecefFrame.ToParentDcm * ecef.Position; // position is ECI
				UpdateAttitude();
			}

			base.Execute();

			if (positionIsEcef)
				position = ecef.Position;
		}

		#endregion

		void UpdateAttitude()
		{
			eciToLvlhDcm = localEcefDcm * ecefFrame.FromParentDcm;
			lvlhFrame.FromParentDcm = eciToLvlhDcm;
			if (eulerOffset.HasValue)
			{
				eciToBodyDcm = eulerRotDcm * eciToLvlhDcm;
				bodyFrame.FromParentDcm = eciToBodyDcm;
			}
			else
				bodyFrame.FromParentDcm = eciToLvlhDcm;
		}

		#region Initialization

		/// <summary>
		/// Euler angle offsets, [deg]
		/// </summary>
		[Blackboard("Initial.EulerOffset", Units = "deg"),Category(category),XmlAttribute("eulerOffset")]
		[Description("Euler angle offsets, [deg]")]
		[DefaultValue("0,0,0")]
		public string EulerOffset
		{
			get
			{
				return eulerOffset != null ? eulerOffset.ToString() : "0,0,0";
				//return str.Substring(1, str.Length - 2);
			}
			set
			{
				eulerOffset = new Vector3(value);
				eulerRotDcm.Identity().RotationFromDegrees("xyz", eulerOffset.Value);
				if (eulerOffset.Value.IsZero)
					eulerOffset = null;
			}
		}

		/// <summary>
		/// Sets the initial state
		/// </summary>
		/// <param name="state"></param>
		public virtual void SetInitialState(InitialState state)
		{
			if (ecefFrame == null)
			{
				var earthFrames = ModelMgr.Model("Earth") as IHaveFrames;
				if (earthFrames != null)
					ecefFrame = earthFrames.GetFrame("ecef") as DynamicFrame;
			}
			switch (state)
			{
				case InitialState.ECEF:
					ecef.SetPosition(initialPosition.Value);
					if (ecefFrame != null)
						Log.WriteLine("Can't set InitialState to ECEF without an Earth in the environment.");
					break;
				case InitialState.LLA:
					ecef.SetLatLonAlt(initialPosition.Value[0], initialPosition.Value[1], initialPosition.Value[2]);
					if (ecefFrame == null)
						Log.WriteLine("Can't set InitialState to LLA without an Earth in the environment.");
					break;
			}
			if (!(this is Vehicle) && ecefFrame != null)
			{
				bodyFrame.Location = position;
				bodyFrame.Parent = ecefFrame;
				frames.AddFrame(ecefFrame);
				position = ecefFrame.ToParentDcm * ecef.Position; // position is ECI
				updateEci = Parent == null && (state == InitialState.ECEF || state == InitialState.LLA);
			}
		}

		/// <summary>
		/// Sets the initial attitude
		/// </summary>
		/// <param name="initAttitude"></param>
		public virtual void SetInitialAttitude(InitialAttitude initAttitude)
		{
			switch (initAttitude)
			{
				case InitialAttitude.LLA:
					localEcefDcm.Identity().RotateToDegrees("zy",
						-ecef.Longitude, ecef.Latitude - 90);
					break;
				case InitialAttitude.LLANegative:
					localEcefDcm.Identity().RotateToDegrees("zy",
						clock.GreenwichHourAngle * Constant.DegPerRad + ecef.Longitude,
						90 - ecef.Latitude);
					break;
			}
			if (updateEci)
			{
				lvlhFrame.Location = position;
				UpdateAttitude();
			}
			else if (initAttitude != InitialAttitude.LVLH && ecefFrame != null)
			{
				eciToLvlhDcm = localEcefDcm * ecefFrame.FromParentDcm;
				if (eulerOffset != null)
				{
					eciToBodyDcm = eulerRotDcm * eciToLvlhDcm;
					bodyFrame.FromParentDcm = eciToBodyDcm;
				}
				else
					bodyFrame.FromParentDcm = eciToLvlhDcm;
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// The body frame, created in Discover
		/// </summary>
		[Blackboard,Category(category),XmlIgnore]
		public DynamicFrame BodyFrame { get { return bodyFrame; } }

		/// <summary>
		/// Earth Centered, Earth Fixed reference frame
		/// </summary>
		[Blackboard, Category(category), XmlIgnore]
		[Description("Earth Centered, Earth Fixed reference frame")]
		public EcefState Ecef { get { return ecef; } set { ecef = value; } }

		/// <summary>
		/// How is the position and velocity of the entity set at initialization.
		/// </summary>
		[Blackboard("Initial.State"),Category(category),XmlAttribute("initialState"),Instanced]
		[DefaultValue(InitialState.None)]
		[Description("How is the position and velocity of the entity set at initialization.")]
		public virtual InitialState InitialState
		{
			get { return initialState; }
			set { initialState = value; }
		} InitialState initialState = InitialState.None;

		/// <summary>
		/// How is the attitude of the entity set at initialization.
		/// </summary>
		[Blackboard("Initial.Attitude"),Category(category),XmlAttribute("initialAttitude"),Instanced]
		[DefaultValue(InitialAttitude.Identity)]
		[Description("How is the attitude of the entity set at initialization.")]
		public virtual InitialAttitude InitialAttitude
		{
			get { return initialAttitude; }
			set { initialAttitude = value; }
		} InitialAttitude initialAttitude = InitialAttitude.Identity;

		/// <summary>
		/// Initial position. Can be ECI, ECF, NED, LLA, UTM
		/// </summary>
		[Blackboard("Initial.Position"),Category(category),XmlAttribute("initialPosition"),Instanced]
		[Description("Initial position. Can be ECI, ECF, NED, LLA, UTM")]
		[DefaultValue("0,0,0")]
		public string InitialPosition
		{
			get
			{
				string str = initialPosition.HasValue ? initialPosition.Value.ToString() : "[0,0,0]";
				return str.Substring(1, str.Length - 2);
			}
			set { initialPosition = new Vector3(value); }
		} Vector3? initialPosition;

		/// <summary>
		/// 
		/// </summary>
		[Blackboard, Category(category), XmlIgnore]
		public DynamicFrame LvlhFrame { get { return lvlhFrame; } }

		/// <summary>
		/// Position of entity
		/// </summary>
		[Blackboard(Units = "m"), Category(category), XmlIgnore]
		[Description("ECI position vector [m]")]
		public Vector3 Position
		{
			get { return position; }
			set { position = value; }
		} Vector3 position = new Vector3();

		#endregion

		#region IHaveFrames Members

		FrameList frames;

		public Frame GetFrame(string name)
		{
			return frames.GetFrame(name);
		}

		[Category(category),XmlIgnore]
		public FrameList Frames
		{
			get { return frames; }
		}

		#endregion

	}

	/// <summary>
	/// Use the InitialState method to initialize an Entity's position and velocity.
	/// </summary>
	public enum InitialState
	{
		/// <summary>
		/// Not used
		/// </summary>
		None,
		/// <summary>
		/// Interpret the initial position as Earth Centered Inertial
		/// </summary>
		ECI,
		/// <summary>
		/// Interpret the initial position as Earth Centered, Earth Fixed
		/// </summary>
		ECEF,
		/// <summary>
		/// Interpret the initial position as ...? (not implemented)
		/// </summary>
		NED,
		/// <summary>
		/// Interpret the initial position as latitude, longitude and altitude
		/// </summary>
		LLA,
		/// <summary>
		/// Use the OrbitalElements to set ECI position
		/// </summary>
		OrbitalElements,
		/// <summary>
		/// Interpret the initial position as Universal Transverse Mercator grid + altitude
		/// </summary>
		UTMGrid
	}

	/// <summary>
	/// Use the InitialAttitude method to initialize an Entity's attitude.
	/// </summary>
	public enum InitialAttitude
	{
		/// <summary>
		/// Leave the attitude initialized to Identity
		/// </summary>
		Identity,
		/// <summary>
		/// Set the attitude to align the body to the earth local frame
		/// </summary>
		LLA,
		/// <summary>
		/// Set the attitude to align the body to the -earth local frame
		/// </summary>
		LLANegative,
		/// <summary>
		/// Set the attitude to the Local Vertical / Local Horizontal frame
		/// </summary>
		LVLH
	}

}
