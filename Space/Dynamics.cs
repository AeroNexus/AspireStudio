using Aspire.Framework;
using Aspire.Primitives;
using Aspire.Utilities;

namespace Aspire.Space
{
    public class Dynamics : Model, IHaveDerivatives, ILoadReceiver
    {
		public override void Discover(Scenario scenario)
		{
			base.Discover(scenario);
		}

		public override void Initialize()
		{
			base.Initialize();
		}

		public override void Execute()
		{
			base.Execute();
		}

		#region IHaveDerivatives Members

		public void CalculateDerivatives()
		{
		}

		#endregion

		#region ILoadReceiver Members

		public void ApplyLoads(IGenerateMechanicalLoads loader, LoadType loadType, Vector3 force, Vector3 torque)
		{
		}

		#endregion
	}
}
