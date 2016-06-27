using Aspire.Framework;
using Aspire.Primitives;
using Aspire.Utilities;

namespace Aspire.Space
{
    public class Imu : Model
    {
		public override void Discover(Scenario scenario)
		{
			base.Discover(scenario);
		}

		public override void Initialize()
		{
		}

		public override void Execute()
		{
			base.Execute();
		}

		public Vector3 mAttitudeRate = new Vector3(0.1, 0.2, 0.3);
		public Vector3 AttitudeRate { get { return mAttitudeRate; } }
	}
}
