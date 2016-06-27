using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Primitives;
using Aspire.Utilities;

namespace Aspire.Space
{
    public class Vehicle : Entity, IBody
    {
		public Vehicle()
		{
			Enabled = true; // override Entity's initialization
		}

		#region Declarations

		const string category = "Vehicle";

		/// <summary>
		/// The Entity's context in the Environment model
		/// </summary>
		protected EnvironmentContext environCtx;

		#endregion

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			if (MassProperties == null)
				MassProperties = new MassProperties();

			var env = ModelMgr.Model("Environment") as Environment;
			environCtx = env.Register(this);

			base.Discover(scenario);
		}

		public override void Initialize()
		{
			base.Initialize();
			Execute();
			ecef.Calculate(this);

			base.Initialize();
		}

		public override void Execute()
		{
			bodyFrame.FromParentQ.Normalize();
			base.Execute();
		}

		#endregion

		#region Initialization

		/// <summary>
		/// Angular rate of the body wrt the inertial frame, [rad/s]
		/// </summary>
		[Category(category)]
		[XmlAttribute("initialBodyW")]
		[Description("Initial angular rate of the body wrt the inertial frame, [rad/s]")]
		[Instanced]
		[DefaultValue("0,0,0")]
		public string InitialBodyW
		{
			get
			{
				return initialBodyW.ToString();
			}
			set { initialBodyW = new Vector3(value); }
		} Vector3 initialBodyW = new Vector3();

		/// <summary>
		/// Initial velocity. Can be ECI, ECF, NED, LLA, UTM
		/// </summary>
		[Category(category)]
		[XmlAttribute("initialVelocity")]
		[Description("Initial velocity. Can be in ECI, ECF, NED, LLA, UTM")]
		[Instanced]
		[DefaultValue("0,0,0")]
		public string InitialVelocity
		{
			get
			{
				return initialVelocity.ToString();
			}
			set { initialVelocity = new Vector3(value); }
		} Vector3 initialVelocity = new Vector3();

		/// <summary>
		/// Initialize the attitude and body rates
		/// </summary>
		/// <param name="initAttitude"></param>
		public override void SetInitialAttitude(InitialAttitude initAttitude)
		{
			base.SetInitialAttitude(initAttitude);
			if (initAttitude != InitialAttitude.LVLH && initAttitude != InitialAttitude.Identity)
			{
				BodyRate = initialBodyW;

				Frame ecefFrame = GetFrame("ecef");
				if (ecefFrame == null) return;
				bodyFrame.FromParentDcm = bodyFrame.FromParentDcm * ecefFrame.FromParentDcm;
				//bodyFrame.Sync();
			}
		}

		/// <summary>
		/// Sets the initial state
		/// </summary>
		/// <param name="state"></param>
		public override void SetInitialState(InitialState state)
		{
			base.SetInitialState(state);
			IEarth iearth;

			switch (state)
			{
				case InitialState.ECI:
					mEciR = InitialPosition;
					iearth = environCtx.Earth;
					if (iearth != null)
					{
						Dcm eciToEcef = iearth.GetEarth().GetFrame("ecef").FromParentDcm;
						Vector3 ecefR = new Vector3();
						ecefR = eciToEcef * mEciR;
						ecef.SetPosition(ecefR);
					}
					else
						Log.WriteLine("Can't set InitialState to ECI without an Earth in the environment.");
					mEciV = initialVelocity;
					break;
				case InitialState.ECEF:
				case InitialState.LLA:
					iearth = environCtx.Earth;
					if (iearth != null)
					{
						Dcm ecefToEci = iearth.GetEarth().GetFrame("ecef").ToParentDcm;
						if (BodyFrame.Parent == Frame.InertialFrame)
							mEciR = ecefToEci * ecef.Position; // ECI
						else
							mEciR = ecef.Position; // ECEF

						ecef.Velocity = initialVelocity;
						mEciV = ecef.ToEciVelocity;
					}
					else
						Log.WriteLine("Can't set Vehicle.InitialState to ECEF without an Earth in the environment.");
					break;
			}

			Position = bodyFrame.Location;

			NormalizePosVel();
		}

		#endregion

		#region IPropertyCategoryInitializer Members

		/// <summary>
		/// Gets a collection of category names to set expanded or collapsed on the Property view
		/// </summary>
		/// <returns></returns>
		public override Dictionary<string, bool> GetCategoryStates()
		{
			var dict = base.GetCategoryStates();
			dict.Add("Entity", false);
			return dict;
		}

		#endregion

		#region IBody implementation

		/// <summary>
		/// Body angular rates wrt/ the inertial frame, [rad/s]]
		/// </summary>
		protected Vector3 mBodyRate = new Vector3();
		/// <summary>
		/// Inertial position
		/// </summary>
		protected Vector3 mEciR = new Vector3();
		/// <summary>
		/// Inertial position unit vector
		/// </summary>
		protected Vector3 mEciRunit = new Vector3();
		/// <summary>
		/// Inertial velocity
		/// </summary>
		protected Vector3 mEciV = new Vector3();
		/// <summary>
		/// Inertial velocity unit vector
		/// </summary>
		protected Vector3 mEciVunit = new Vector3();

		[Blackboard, Category(category), XmlIgnore]
		public Vector3 BodyRate { get { return mBodyRate; } set { mBodyRate = value; } }

		[Blackboard("Eci.R"), Category(category), XmlIgnore]
		public Vector3 EciR { get { return mEciR; } set { mEciR = value; } }

		[Blackboard("Eci.Runit"), Category(category), XmlIgnore]
		public Vector3 EciRunit { get { return mEciRunit; } }

		[Blackboard("Eci.Rmag"), Category(category), XmlIgnore]
		public double EciRmag { get { return mEciRmag; } } protected double mEciRmag;

		[Blackboard("Eci.V"),Category(category), XmlIgnore]
		public Vector3 EciV { get { return mEciV; } set { mEciV = value; } }

		[Blackboard("Eci.Vunit"), Category(category), XmlIgnore]
		public Vector3 EciVunit { get { return mEciVunit; } }

		[Blackboard("Eci.Vmag"), Category(category), XmlIgnore]
		public double EciVmag { get { return mEciVmag; } } protected double mEciVmag;

		public void NormalizePosVel()
		{
			mEciRunit = mEciR;
			mEciRunit.Normalize(out mEciRmag);
			mEciVunit = mEciV;
			mEciVunit.Normalize(out mEciVmag);
		}

		#endregion

		#region Properties

		[Blackboard, Category(category),XmlIgnore]
		public Quaternion Attitude { get { return bodyFrame.FromParentQ; } }

		[Blackboard,Category(category)]
		public MassProperties MassProperties { get; set; }

		#endregion
	}
}
