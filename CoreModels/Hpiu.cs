//using System;
////using System.ComponentModel;
//using System.Net;
//using System.Net.Sockets;
//using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Primitives;
////using Aspire.Core;
using Aspire.Core.Messaging;
using Aspire.Utilities;

namespace Aspire.CoreModels
{
	public class Hpiu : AspireShell
	{
		Blackboard.Item GpsSeconds, EphemerisGps, Position, Velocity, AttitudeGps, AttitudeRate, Attitude, SunStatus;

		#region Model implementation

		#endregion

		byte[] swap = new byte[8];

		public void ParseHost(byte[] bytes, int offset)
		{
			if (GpsSeconds == null) return;

			uint gpsSeconds = GetNetwork.UINT(bytes, offset, swap);
			GpsSeconds.Value = gpsSeconds;
			EphemerisGps.Value = gpsSeconds;
			AttitudeGps.Value = gpsSeconds;
			IArrayProxy ap = Position.Value as IArrayProxy;
			ap[0] = GetNetwork.DOUBLE(bytes, offset + 4, swap);
			ap[1] = GetNetwork.DOUBLE(bytes, offset + 12, swap);
			ap[2] = GetNetwork.DOUBLE(bytes, offset + 20, swap);
			ap = Velocity.Value as IArrayProxy;
			ap[0] = GetNetwork.FLOAT(bytes, offset + 28, swap);
			ap[1] = GetNetwork.FLOAT(bytes, offset + 32, swap);
			ap[2] = GetNetwork.FLOAT(bytes, offset + 36, swap);
			ap = Attitude.Value as IArrayProxy;
			ap[0] = GetNetwork.DOUBLE(bytes, offset + 40, swap);
			ap[1] = GetNetwork.DOUBLE(bytes, offset + 48, swap);
			ap[2] = GetNetwork.DOUBLE(bytes, offset + 56, swap);
			ap[3] = GetNetwork.DOUBLE(bytes, offset + 64, swap);
			ap = AttitudeRate.Value as IArrayProxy;
			ap[0] = GetNetwork.FLOAT(bytes, offset + 72, swap);
			ap[1] = GetNetwork.FLOAT(bytes, offset + 76, swap);
			ap[2] = GetNetwork.FLOAT(bytes, offset + 80, swap);
			SunStatus.Value = GetNetwork.UCHAR(bytes, offset+84);
		}

		public override void Publish()
		{
			base.Publish();

			GpsSeconds = Blackboard.Subscribe(this, ".iHostState.TimeMsg.GpsSeconds");
			EphemerisGps = Blackboard.Subscribe(this, ".iHostState.EphemerisMsg.GpsSeconds");
			Position = Blackboard.Subscribe(this, ".iHostState.EphemerisMsg.Position");
			Velocity = Blackboard.Subscribe(this, ".iHostState.EphemerisMsg.Velocity");
			AttitudeGps = Blackboard.Subscribe(this, ".iHostState.AttitudeMsg.GpsSeconds");
			Attitude = Blackboard.Subscribe(this, ".iHostState.AttitudeMsg.Attitude");
			AttitudeRate = Blackboard.Subscribe(this, ".iHostState.AttitudeMsg.AttitudeRate");
			SunStatus = Blackboard.Subscribe(this, ".iHostState.SunStatusMsg.SunStatus");
		}
	}


}
