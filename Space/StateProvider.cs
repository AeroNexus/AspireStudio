using System;
using System.Xml.Serialization;

using Aspire.Core.Messaging;
using Aspire.CoreModels;
using Aspire.Framework;
using Aspire.Primitives;
//using Aspire.Utilities;

namespace Aspire.Space
{
    public class StateProvider : Model
    {
		AspireClock clock;
		Frame bodyFrame;
		Spacecraft spacecraft;
		Sun.SunContext sun;
		UdpClientTransport udp;
		byte[] ccsdsBuffer = new byte[256], swap = new byte[8];

		public override void Discover(Scenario scenario)
		{
			scenario.Clock.SecondsChanged += Clock_SecondsChanged;
			clock = scenario.Clock.GetService(typeof(AspireClock)) as AspireClock;
			spacecraft = Parent as Spacecraft;
			bodyFrame = spacecraft.BodyFrame;
			udp = new UdpClientTransport();
			udp.DestinationAddress = RemoteAddress;
			udp.Open();

			ccsdsBuffer[1] = 0x50;

			Enabled = false; // using clock top of second to send data

			base.Discover(scenario);

			foreach (var model in spacecraft.Models)
				if (model is Sun.SunContext)
				{
					sun = model as Sun.SunContext;
					break;
				}
		}

		public override void Initialize()
		{
		}

		void Clock_SecondsChanged(object sender, System.EventArgs e)
		{
			int offset = 6;

			PutNetwork.UINT(ccsdsBuffer, offset, clock.GpsSeconds);
			PutNetwork.DOUBLE(ccsdsBuffer, offset + 4, spacecraft.EciR[0], swap);
			PutNetwork.DOUBLE(ccsdsBuffer, offset + 12, spacecraft.EciR[1], swap);
			PutNetwork.DOUBLE(ccsdsBuffer, offset + 20, spacecraft.EciR[2], swap);
			PutNetwork.FLOAT(ccsdsBuffer, offset + 28, Convert.ToSingle(spacecraft.EciV.X), swap);
			PutNetwork.FLOAT(ccsdsBuffer, offset + 32, Convert.ToSingle(spacecraft.EciV.Y), swap);
			PutNetwork.FLOAT(ccsdsBuffer, offset + 36, Convert.ToSingle(spacecraft.EciV.Z), swap);
			PutNetwork.DOUBLE(ccsdsBuffer, offset + 40, bodyFrame.FromParentQ[0], swap);
			PutNetwork.DOUBLE(ccsdsBuffer, offset + 48, bodyFrame.FromParentQ[1], swap);
			PutNetwork.DOUBLE(ccsdsBuffer, offset + 56, bodyFrame.FromParentQ[2], swap);
			PutNetwork.DOUBLE(ccsdsBuffer, offset + 64, bodyFrame.FromParentQ[3], swap);
			PutNetwork.FLOAT(ccsdsBuffer, offset + 72, Convert.ToSingle(spacecraft.BodyRate.X), swap);
			PutNetwork.FLOAT(ccsdsBuffer, offset + 76, Convert.ToSingle(spacecraft.BodyRate.Y), swap);
			PutNetwork.FLOAT(ccsdsBuffer, offset + 80, Convert.ToSingle(spacecraft.BodyRate.Z), swap);
			PutNetwork.UCHAR(ccsdsBuffer, offset+84, (byte)(sun.Visible>0.5?1:0));
			PutNetwork.USHORT(ccsdsBuffer, 4, (ushort)(85-1));

			udp.Write(ccsdsBuffer, 0, offset+85, null);
		}

		[XmlAttribute("remoteAddress")]
		public string RemoteAddress { get; set; }
	}
}
