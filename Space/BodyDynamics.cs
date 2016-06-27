using System.Collections.Generic;

using Aspire.Framework;
using Aspire.Primitives;
using Aspire.Utilities;

namespace Aspire.Space
{
    public class BodyDynamics : Model, IHaveDerivatives, ILoadReceiver
	{
		#region Declarations

		IBody body;
		Dynamics dynamics;
		Frame bodyFrame;
		List<IGenerateMechanicalLoads> loadGenerators = new List<IGenerateMechanicalLoads>();
		MassProperties massProps;
		[Blackboard]
		Dynamics.State stateR, stateV, stateQ, stateW;
		Quaternion Q;
		Vector3
			accel = new Vector3(),
			Aenv = new Vector3(),
			Fact = new Vector3(),
			Fenv = new Vector3(),
			Ftotal = new Vector3(),
			relW = new Vector3(),
			Tact = new Vector3(),
			Tenv = new Vector3(),
			Tgyro = new Vector3(),
			Ttotal = new Vector3(),
			Tuser = new Vector3(),
			Wdot = new Vector3();
		double[] Qdot = new double[4];

		#endregion

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			dynamics = scenario.Dynamics;

			body = Body.GetBody(this);
			bodyFrame = (body as IHaveFrames).GetFrame("body");
			massProps = (body as Vehicle).MassProperties;

			stateR = dynamics.NewState("Pos", body.EciR, this);
			stateV = dynamics.NewState("Vel", body.EciV, this);
			stateQ = dynamics.NewState("Q", bodyFrame.FromParentQ, this);
			stateW = dynamics.NewState("W",body.BodyRate, this);

			base.Discover(scenario);

			foreach (var model in (body as Model).Models)
				if (model is IGenerateMechanicalLoads)
					if ((model as IGenerateMechanicalLoads).GeneratesMechanicalLoads)
						loadGenerators.Add(model as IGenerateMechanicalLoads);
		}

		public override void Initialize()
		{
			Synchronize(true);
			base.Initialize();
		}

		public override void Execute()
		{
			// Copy state X to actual states
			body.EciR = (Vector3)stateR.Yproxy;
			body.EciV = (Vector3)stateV.Yproxy;
			bodyFrame.FromParentQ = ((Quaternion)stateQ.Yproxy).Normalize();
			body.BodyRate = (Vector3)stateW.Yproxy;

			base.Execute();
		}

		#endregion

		#region IHaveDerivatives Members

		public void CalculateDerivatives()
		{
			// Copy the integrated states into the actual states
			body.EciR = (Vector3)stateR.Yproxy;
			body.EciV = (Vector3)stateV.Yproxy;
			body.NormalizePosVel();

			Q = ((Quaternion)stateQ.Yproxy).Normalize();
			bodyFrame.FromParentQ = Q;
			relW = (Vector3)stateW.Yproxy;
			body.BodyRate = relW;

			Fenv.Zero(); Fact.Zero(); Aenv.Zero();
			Tenv.Zero(); Tact.Zero(); Tuser.Zero();

			foreach (var lg in loadGenerators)
				lg.CalculateMechanicalLoads(this);

			Ftotal = Fenv + Fact;
			accel = Ftotal * massProps.InvMass + Aenv;

			Tgyro.Zero(); // for now
			Ttotal = Tenv + Tact + Tgyro + Tuser;
			Wdot = massProps.InvInertia * Ttotal;

			Qdot[0] = 0.5 * ( relW[2] * Q[1] - relW[1] * Q[2] + relW[0] * Q[3]);
			Qdot[1] = 0.5 * (-relW[2] * Q[0] + relW[0] * Q[2] + relW[1] * Q[3]);
			Qdot[2] = 0.5 * ( relW[1] * Q[0] - relW[0] * Q[1] + relW[2] * Q[3]);
			Qdot[3] = 0.5 * (-relW[0] * Q[0] - relW[1] * Q[1] - relW[2] * Q[2]);

			// Set the derivatives
			stateR.YdotProxy = body.EciV;
			stateV.YdotProxy = accel;
			stateQ.Ydot = Qdot;
			stateW.YdotProxy = Wdot;
		}

		internal void SyncAttitude()
		{
			stateQ.Yproxy = bodyFrame.FromParentQ;
			stateW.Yproxy = body.BodyRate;
		}

		public void Synchronize(bool force=false)
		{
			if (syncRequired || force)
			{
				stateR.Yproxy = body.EciR;
				stateV.Yproxy = body.EciV;
				stateQ.Yproxy = bodyFrame.FromParentQ;
				stateW.Yproxy = body.BodyRate;
				syncRequired = false;
			}
		} bool syncRequired;

		#endregion

		#region ILoadReceiver Members

		public void ApplyLoads(IGenerateMechanicalLoads loader, LoadType loadType, Vector3 force, Vector3 torque)
		{
			switch (loadType)
			{
				case LoadType.Gravitation:
					Aenv += force;
					Tenv += torque;
					break;
			}
		}

		#endregion
	}
}
