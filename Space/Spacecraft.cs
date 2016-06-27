using System.ComponentModel;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Primitives;
using Aspire.Utilities;

namespace Aspire.Space
{
    public class Spacecraft : Vehicle, IBody
	{
		#region Declarations

		const string category = "Spacecraft";

		AstroClock astroClock;
		BodyDynamics bodyDynamics;
		Clock clock;
		Dcm lvlhRotationDcm;
		OrbitalState orbitalState = new OrbitalState();
		Quaternion prevFromParentQ = new Quaternion();
		Vector3
			lvlhRotation,
			nadir = new Vector3(),
			temp = new Vector3();

		#endregion

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			clock = scenario.Clock;
			astroClock = clock.GetService(typeof(AstroClock)) as AstroClock;

			base.Discover(scenario);

			foreach (var model in Models)
				if (model is BodyDynamics)
					bodyDynamics = model as BodyDynamics;
			if (bodyDynamics == null)
				bodyDynamics = AddChildAtFront(new BodyDynamics(), scenario) as BodyDynamics;
		}

		public override void Initialize()
		{
			// If initializing to LVLH, let euler offset also serve for lvlh rotation
			if (InitialAttitude == InitialAttitude.LVLH && ForceLvlh &&
				eulerOffset != null && !eulerOffset.Value.IsZero)
				LvlhRotation = EulerOffset;

			bool saveForceLvlh = ForceLvlh;
			ForceLvlh = false;

			base.Initialize();

			ForceLvlh = saveForceLvlh;

			if (orbitalState != null) orbitalState.Initialize();
		}

		public override void Execute()
		{
			if (deltaVApplied)
			{
				mEciV += deltaV;
				deltaV.Zero();
				deltaVApplied = false;
			}
			// This has to be done here for LVLH to be correct. Its also done in
			// Dynamics.Execute, but that's too late
			mEciVunit = mEciV;
			mEciVunit.Normalize(out mEciVmag);

			lvlhFrame.FromParentDcm = Orbit.LvlhFromParent(mEciRunit, mEciVunit);
			lvlhFrame.Location = mEciR;

			base.Execute();

			if (ForceLvlh)
			{
				if (eulerOffset != null)
					bodyFrame.FromParentDcm = lvlhRotationDcm * lvlhFrame.FromParentDcm;
				else
					bodyFrame.FromParentDcm = lvlhFrame.FromParentDcm;
				bodyFrame.FromParentQ.AngularRate(ref mBodyRate, prevFromParentQ, clock.StepSize);
				prevFromParentQ = bodyFrame.FromParentQ;
				bodyDynamics.SyncAttitude();
			}

			temp = -EciRunit;
			temp = bodyFrame.FromParentDcm * temp;
			nadir = temp;

			// keep orbit state in sync.
			orbitalState.Update(mEciR,mEciV, clock.DateTime);
		}

		#endregion

		#region Initialization

		/// <summary>
		/// How is the position and velocity of the entity set at initialization.
		/// </summary>
		[Category(category)]
		[XmlAttribute("initialState")]
		[DefaultValue(InitialState.None)]
		[Description("How is the position and velocity of the entity set at initialization.")]
		[Blackboard("Initial.State")]
		public override InitialState InitialState
		{
			get
			{
				if (base.InitialState == InitialState.OrbitalElements && IsSaving)
					return InitialState.None; // The default value, so as not to clutter up the XML
				return base.InitialState;
			}
			set { base.InitialState = value; }
		}

		/// <summary>
		/// Sets the initial state
		/// </summary>
		/// <param name="state"></param>
		public override void SetInitialState(InitialState state)
		{
			switch (state)
			{
				case InitialState.OrbitalElements:
					if (KeplerElements != null)
					{
						KeplerElements.Propagate(clock.DateTime, ref mEciR, ref mEciV);
						bodyFrame.Location = mEciR;
					}
					break;
			}
			base.SetInitialState(state);
		}

		/// <summary>
		/// Set the initial attitude attitude 
		/// </summary>
		/// <param name="initAttitude"></param>
		public override void SetInitialAttitude(InitialAttitude initAttitude)
		{
			switch (initAttitude)
			{
				case InitialAttitude.LVLH:
					NormalizePosVel();
					bodyFrame.FromParentDcm = Orbit.LvlhFromParent(mEciRunit, mEciVunit);
					break;
			}
			base.SetInitialAttitude(initAttitude);
		}

		#endregion

		/// <summary>
		/// 
		/// </summary>
		[Category(category)]
		[XmlIgnore]
		[Blackboard(Description = "Inertial delta velocity to be applied in the next Execute")]
		public Vector3 ApplyDeltaV
		{
			get { return deltaV; }
			set { deltaV = value; deltaVApplied = true; }
		} Vector3 deltaV = new Vector3();
		bool deltaVApplied;

		#region Properties

		[Category(category)]
		[Blackboard(Description = "Force the SC into LVLH attitude (always)")]
		[DefaultValue(false)]
		[XmlAttribute("forceLvlh")]
		public bool ForceLvlh { get; set; }

		[Category(category), XmlIgnore]
		public KeplerElements KeplerElements { get; set; }

		Vector3 LvlhRotation
		{
			set
			{
				lvlhRotation = new Vector3();
				lvlhRotation = value;
				lvlhRotationDcm = new Dcm().Identity();
				lvlhRotationDcm.RotationFromDegrees("123", lvlhRotation);
				//ToDo: Fix lvlhRotation. vis a vis from EulerOffset. If 90,0,0, attitude, attitude rate are bad if forceLvlh
			}
		}

		[Category(category), Blackboard, XmlIgnore]
		public Vector3 Nadir { get { return nadir; } }

		[Category(category), Blackboard, XmlIgnore]
		public OrbitalState OrbitalState { get { return orbitalState; } }

		[Category(category), XmlAttribute("orbit"), Browsable(false)]
		public string OrbitString
		{
			get
			{
				if (KeplerElements == null) return string.Empty;
				return KeplerElements.ToString();
			}
			set
			{
				KeplerElements = KeplerElements.Parse(value);
				InitialState = InitialState.OrbitalElements;
			}
		}

		#endregion

		#region IHaveDerivatives Members

		public void CalculateDerivatives()
		{
		}

		#endregion

	}
}
