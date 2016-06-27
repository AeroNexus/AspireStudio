using System;
using System.ComponentModel;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Primitives;
using Aspire.Utilities;

namespace Aspire.Space
{
	public class Gravity : EnvironmentModel
	{
		/// <summary>
		/// default constructor
		/// </summary>
		public Gravity()
		{
			Enabled = false;
		}

		#region Declarations

		#endregion

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			base.Discover(scenario);

			if (!(Parent is Environment))
				Environment.AddModel(this);
		}

		//public override void Initialize()
		//{
		//}

		//public override void Execute()
		//{
		//}

		#endregion

		#region EnvironmentModel

		/// <summary>
		/// returns a model context for this environmental model and the model passed in
		/// </summary>
		/// <param name="owner">The owning Model of the EnvironmentContext.</param>
		/// <param name="context">The containing EnvironmentContext. Used to defer creation until PostDiscover.</param>
		/// <returns>A new ModelContext or null if deferred until PostDiscover</returns>
		public override EnvironmentModelContext CreateModelContext(Model owner, EnvironmentContext context)
		{
			var cb = context.CelestialBody(mCentralBodyName);
			if (cb != null)
			{
				var ctx = new GravityContext(this, owner);
				ctx.SetCentralBody(cb, context.NBodyGravity(cb));
				return ctx;
			}
			else
			{
				Log.WriteLine("{0}: can't find {1}", Name, mCentralBodyName);
				return null;
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// The name of the CentralBody so we can get the mass and gravityConstant
		/// </summary>
		[XmlAttribute("centralBody")]
		[DefaultValue("Earth")]
		public string CentralBody
		{
			get { return mCentralBodyName; }
			set { mCentralBodyName = value; }
		} string mCentralBodyName = "Earth";

		/// <summary>
		/// Should we calculate gravity gradient torques ?
		/// </summary>
		[DefaultValue(true)]
		[XmlAttribute("gravityGradient")]
		public bool GravityGradient
		{
			get { return gravityGradient; }
			set
			{
				gravityGradient = value;
				if (GravityGradientChanged != null)
					GravityGradientChanged(this, EventArgs.Empty);
			}
		} bool gravityGradient = true;
		internal event EventHandler GravityGradientChanged;

		/// <summary>
		/// The J2 constant
		/// </summary>
		[XmlAttribute]
		public double J2
		{
			get { return j2; }
			set
			{
				j2 = value;
				higherOrder = (j3 != 0.0 || j4 != 0.0);
			}
		} double j2;
		/// <summary>
		/// The J3 constant
		/// </summary>
		[XmlAttribute]
		public double J3
		{
			get { return j3; }
			set
			{
				j3 = value;
				higherOrder = (j3 != 0.0 || j4 != 0.0);
			}
		} double j3;
		/// <summary>
		/// The J4 constant
		/// </summary>
		[XmlAttribute]
		public double J4
		{
			get { return j4; }
			set
			{
				j4 = value;
				higherOrder = (j3 != 0.0 || j4 != 0.0);
			}
		} double j4;
		bool higherOrder;

		#endregion

		public class GravityContext : EnvironmentModelContext, IGenerateMechanicalLoads
		{
			IBody body;
			Frame bodyFrame;
			Gravity gravityModel;
			Vector3 Runit;
			Vector3 mAccel = new Vector3(), bodyRunit = new Vector3(), Ir = new Vector3(), mTorque = new Vector3();

			double Requator;

			public GravityContext() { }

			/// <summary>
			/// Constructor to set model and owner
			/// </summary>
			/// <param name="model"></param>
			/// <param name="owner"></param>
			public GravityContext(EnvironmentModel model, Model owner) : base(model,owner)
			{
				Enabled = false;
				gravityModel = model as Gravity;
				GravityGradient = gravityModel.GravityGradient;
				gravityModel.GravityGradientChanged += gravityModel_GravityGradientChanged;
				body = Body.GetBody( owner );
				if ( body == null )
				{
					Write( "{0} can't find body. Disabling", Path );
					Enabled = false;
				}
				else if ( body is IHaveFrames )
					bodyFrame = (body as IHaveFrames).GetFrame( "body" );
			}

			#region Model implementation

			public override void Initialize()
			{
				if (!GravityGradient)
					mTorque.Zero();
				base.Initialize();
			}

			public override void Execute()
			{
				// Note, using ECI coordinates because this gravity model is a body of revolution,
				// implying that ECEF = ECI. If the model varied with longitude, we would need to use 
				// ECEF position from the body's Ecef information.
				var inertialPos = mNBodyGravity.PerturbToOwnerR;

				double r = mNBodyGravity.PerturbToOwnerRmag;
				if (r == 0) r = 1;

				double k2 = GravityConstant / (r*r);
				double k3 = k2 / r;

				// corresponds to sin( Latitude )
				double sd = inertialPos.Z / r;

				double sd2 = sd * sd;

				double sd4 = sd2 * sd2;

				double SS = Requator / r;

				double SS2 = SS * SS;

				double SS3 = SS2 * SS;

				double SS4 = SS2 * SS2;

				double j3 = gravityModel.j3;

				double V2x = 1.5 * gravityModel.j2 * SS2 * (1.0 - 5.0 * sd2);

				double V3x = 2.5 * j3 * SS3 * (3.0 - 7.0 * sd2) * sd;

				double V4x = (5.0 / 8.0) * gravityModel.j4 * SS4 * (3.0 - 42.0 * sd2 + 63.0 * sd4);

				double Vxa = -k3 * (1.0 + V2x + V3x - V4x);

				double V2z = 1.5 * gravityModel.j2 * SS2 * (3.0 - 5.0 * sd2);
				double V3z = 2.5 * j3 * SS3 * (6.0 - 7.0 * sd2) * sd;

				double V4z = (5.0 / 8.0) * gravityModel.j4 * SS4 * (15.0 - 70.0 * sd2 + 63.0 * sd4);

				double Vz = -k3 * inertialPos.Z * (1.0 + V2z + V3z - V4z) + 1.5 * k2 * j3 * SS3;

				mAccel.Set(Vxa * inertialPos.X, Vxa * inertialPos.Y, Vz);

				if (GravityGradient)
				{
					bodyRunit = bodyFrame.FromParentDcm * mNBodyGravity.PerturbToOwnerRunit;
					Ir = body.MassProperties.Inertia * bodyRunit;
					mTorque.CrossProduct(bodyRunit, Ir);
					mTorque *= 3.0 * mAccel.Magnitude / r;
				}
			}

			#endregion

			void gravityModel_GravityGradientChanged(object sender, EventArgs e)
			{
			}

			internal void SetCentralBody(ICelestialBody cb, I_NBodyGravity nBodyGravity)
			{
				if (nBodyGravity == null)
				{
					Log.WriteLine("{0} can't find {1}'s context", Name, (cb as IPublishable).Name);
					return;
				}
				GravityConstant = Constant.Gravitational * cb.Mass;
				mNBodyGravity = nBodyGravity;
				Runit = nBodyGravity.PerturbToOwnerRunit;
				Requator = cb.Radius;
				Enabled = true;
			} I_NBodyGravity mNBodyGravity;

			#region Properties

			[Description("Should the loads be applied to the load receiver"),DefaultValue(true)]
			public bool Contribute { get { return mContribute; } set { mContribute = value; } } bool mContribute = true;

			public Vector3 Accel { get { return mAccel; } }

			/// <summary>
			/// The local gravity constant = GM
			/// </summary>
			[Description("The local gravity constant, kgm^3/s^2"),XmlIgnore]
			public double GravityConstant { get; private set; }

			[Description("Use gravity gradient torque"),XmlIgnore]
			public bool GravityGradient { get; private set; }

			#endregion

			#region IGenerateMechanicalLoads implementation

			/// <summary>
			/// Run the model and apply the loads
			/// </summary>
			public void CalculateMechanicalLoads(ILoadReceiver loadReceiver)
			{
				Execute();

				if (Contribute)
					loadReceiver.ApplyLoads(this, LoadType.Gravitation, mAccel, mTorque);
			}

			/// <summary>
			/// Tests for generate mechanical loads. 
			/// </summary>
			/// <returns></returns>
			public bool GeneratesMechanicalLoads { get { return true; } }

			#endregion
		}
	}
}
