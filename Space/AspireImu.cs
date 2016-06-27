using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;
using Aspire.CoreModels;
using Aspire.Framework;
using Aspire.Primitives;
using Aspire.Utilities;

namespace Aspire.Space
{
    public class AspireImu : Application
    {
		enum DevicePowerState { unkown, Off, On };

		IDataMessage mAngularRateMsg, mDevicePowerStatusMsg, mDeviceTemperatureMsg;
		MarshaledBuffer mCompUidBuf = new MarshaledBuffer();

		protected Vector3 mAttitudeRate = new Vector3(0.1, 0.2, 0.3);
		[Blackboard(Units="rad/s")]
		public Vector3 AttitudeRate { get { return mAttitudeRate; } }

		protected Vector3 mRateVariance = new Vector3(0, 0, 0);
		[Blackboard(Units = "(rad/s)^2")]
		public Vector3 RateVariance { get { return mRateVariance; } }

		protected Vector3 mAmplitude = new Vector3(0.05, 0.1, 0.2);
		public Vector3 Amplitude { get { return mAmplitude; } }

		protected Vector3 mFrequency = new Vector3(0.1, 0.1111, 0.0909);
		public Vector3 Frequency { get { return mFrequency; } }

		protected Vector3 mBias = new Vector3(0, 0, 0);
		public Vector3 Bias { get { return mBias; } }

		DevicePowerState mDevPowerState = DevicePowerState.On, mDevPowerStateCmd = DevicePowerState.Off;
		float mTemperature = 20;
		ushort mCRCProgCode = 0x1234, mCRCxTEDS = 0x5678;
		byte mSWCoreLibRev = 4, mHWFPGAFirmwareRev = 12;

		Aspire.Framework.Clock mClock;

		public override void Discover(Scenario scenario)
		{
			mClock = scenario.Clock;
			base.Discover(scenario);

			Vector3 test = "0,1,2";
		}

		public override void Initialize()
		{
			mAttitudeRate.Zero();
		}

		protected override void Initialize(bool dummy)
		{
		  if (Xteds == null) return;
			mAngularRateMsg = OwnDataMessage("AttitudeRate","AngularRateMsg");
			mAngularRateMsg.XtedsMessage.
				MapVariable("Time").
				MapVariable("SubS").
				MapVariable("AngularRate", "mAttitudeRate").
				MapVariable("RateVariance", "mRateVariance");

			XtedsMessage xmsg = OwnMessage("DevicePower","DevPwrSetState");
			xmsg.MapVariable("DevPwrState", "mDevPowerStateCmd");
			WhenMessageArrives(xmsg, new XtedsMessage.Handler(OnDevPwrSetState));
			xmsg.ReplyMessage.MapVariable("DevPwrState", "mDevPowerState");

			mDevicePowerStatusMsg = OwnDataMessage("DevicePower","DevicePowerStatus");
			mDevicePowerStatusMsg.XtedsMessage.
				MapVariable("Time").
				MapVariable("SubS").
				MapVariable("DevPwrState", "mDevPowerState");

			xmsg = OwnMessage("DeviceSafety","GetDeviceTemperature");
			WhenMessageArrives(xmsg, new XtedsMessage.Handler(OnGetDeviceTemperature));
			xmsg.ReplyMessage.MapVariable("Temperature", "mTemperature");

			mDeviceTemperatureMsg = OwnDataMessage("DeviceSafety","DeviceTemp");
			mDeviceTemperatureMsg.XtedsMessage.
				MapVariable("Time").
				MapVariable("SubS").
				MapVariable("Temperature", "mTemperature");

			xmsg = OwnMessage("ASIM","GetVersionInfo");
			WhenMessageArrives(xmsg, new XtedsMessage.Handler(OnGetVersionInfo));
			xmsg.ReplyMessage.
				MapVariable("CRCProgCode", "mCRCProgCode").
				MapVariable("CRCxTEDS", "mCRCxTEDS").
				MapVariable("SWCoreLibRev", "mSWCoreLibRev").
				MapVariable("HWFPGAFirmwareRev", "mHWFPGAFirmwareRev").
				MapVariable("GUID", "mCompUidBuf");

			xmsg = OwnMessage("Signal","Amplitude");
			xmsg.MapVariable("Value", "mAmplitude");

			OwnMessage("Signal","Bias").MapVariable("Value", "mBias");

			xmsg = OwnMessage("Signal","Frequency");
			xmsg.MapVariable("Value", "mFrequency");

			Uuid compUid = CompUid;
			var bytes = compUid.ToByteArray();
			mCompUidBuf.Set(bytes.Length,bytes,0);
			mDevPowerState = DevicePowerState.On;
		}

		public override void Execute()
		{
			//if (mBias.X != 0)
			//	Write("{0}: bias {1} et {2}", Path, mBias, mClock.ElapsedSeconds);

			const double twoPi = 2*System.Math.PI;
			double t = mClock.ElapsedSeconds;

			if ( mDevPowerState == DevicePowerState.Off ) return;

			Vector3 prevRate = mAttitudeRate;
			double a1 = twoPi * mFrequency.X * t;
			double a2 = System.Math.Sin(a1);
			double a3 = mAmplitude.X * a2 + mBias.X;
			mAttitudeRate.X = (float)(mAmplitude.X * System.Math.Sin(twoPi * mFrequency.X * t) + mBias.X);
			mAttitudeRate.Y = (float)(mAmplitude.Y * System.Math.Sin(twoPi * mFrequency.Y * t) + mBias.Y);
			mAttitudeRate.Z = (float)(mAmplitude.Z * System.Math.Sin(twoPi * mFrequency.Z * t) + mBias.Z);

			mRateVariance = mAttitudeRate - prevRate;
			mRateVariance = mRateVariance*mRateVariance;

			if (mAngularRateMsg!=null) Publish(mAngularRateMsg);
			if ( mClock.MicroSeconds < 50000 ) // half of execution period
			{
				mTemperature = (float)(20.0 + 2.0 * System.Math.Sin(twoPi * (1.0 / 6000.0) * t));
        if (mDeviceTemperatureMsg != null) Publish(mDeviceTemperatureMsg);
			}
		}

		void OnGetDeviceTemperature(XtedsMessage msg)
		{
			Write("OnGetDeviceTemperature() => {0}",mTemperature);
			ReplyTo(msg);
		}

		void OnGetVersionInfo(XtedsMessage msg)
		{
			Write("OnGetVersionInfo() => {0:X} {1:X} {2} {3} {4}",
				mCRCProgCode,mCRCxTEDS,mSWCoreLibRev,mHWFPGAFirmwareRev,
				CompUid.ToString());
			ReplyTo(msg);
		}

		void OnDevPwrSetState(XtedsMessage msg)
		{
			var prev = mDevPowerState;
			SetDevPowerState(mDevPowerStateCmd);
			Write("OnDevPwrSetState({0}) => {1}",prev, mDevPowerState);
			ReplyTo(msg);
		}

		void SetDevPowerState(DevicePowerState state)
		{
			mDevPowerState = state;
			Publish(mDevicePowerStatusMsg);
		}
	}
}
