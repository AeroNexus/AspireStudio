using Aspire.Framework;
using Aspire.Primitives;
using Aspire.Utilities;

namespace Aspire.Space
{
    public class ReactionWheel : Model, IHaveDerivatives
    {
		public override void Discover(Scenario scenario)
		{
			base.Discover(scenario);
		}

		public override void Initialize()
		{
			mRpm = 0;
		}

		public override void Execute()
		{
			mRpm += 0.1;
			base.Execute();
		}

		double mRpm;
		public double Rpm { get { return mRpm; } }

		#region IHaveDerivatives Members

		public void CalculateDerivatives()
		{
			mRpm += 0.1;
		}

		public void Synchronize(bool force = false)
		{
		}

		#endregion
	}
}
