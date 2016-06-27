using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Utilities;

namespace Aspire.CoreModels
{
	public class CcsdsMux : Model
	{
		Address
			aspireParseAddress = ProtocolFactory.CreateAddress(ProtocolId.Aspire),
			parseAddress = ProtocolFactory.CreateAddress(ProtocolId.Aspire);
		Dictionary<uint, Transport> transportsByApId = new Dictionary<uint, Transport>();
		Message
			inputHdr = ProtocolFactory.CreateMessage(ProtocolId.Aspire),
			parseAspireHdr = ProtocolFactory.CreateMessage(ProtocolId.Aspire);
		CcsdsHdr parseCcsds = new CcsdsHdr(), sendCcsds = new CcsdsHdr();
		Hpiu hpiu;
		UdpClientTransport udp;

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			udp = new UdpClientTransport(LocalUdpPort);
			udp.OnReceive = ParseCcsds;
			udp.DestinationAddress = RemoteUdpPort;

			foreach (var transport in InputTransports)
			{
				string addr = transport.ListenAddressString;
				var tokens = addr.Split(':');
				transportsByApId.Add(uint.Parse(tokens[2]), transport);
				if (transport is UdpClientTransport)
					(transport as UdpClientTransport).OnReceive = OnInputReceive;
				transport.Open();
			}

			if (Transport != null)
				(Transport as MsgTransport).HandleReadPacket = OnMsgTransportRead;

			base.Discover(scenario);

			hpiu = ModelMgr.Model("HPIU+Host shell") as Hpiu;
			udp.Open();
		}

		public override void Initialize()
		{
		}

		public override void Unload()
		{
			if (Transport != null) Transport.Close();
			if (udp != null) udp.Close();
			foreach ( var transport in InputTransports)
				transport.Close();
			base.Unload();
		}
		#endregion

		void OnInputReceive(byte[] buffer)
		{
			inputHdr.Unmarshal(buffer,0, buffer.Length);
			switch ((ProtocolId)inputHdr.Version)
			{
				case ProtocolId.Aspire:
				case ProtocolId.XtedsCommand:
				case ProtocolId.XtedsData:
					SendCcsdsOnUdp(buffer,0,buffer.Length,inputHdr.Destination.Hash);
					break;
				default:
					Log.WriteLine(BitConverter.ToString(buffer));
					break;
			}
		}

		void SendCcsdsOnUdp(byte[] buffer,int offset, int length,uint apId)
		{
			if (length + 6 > ccsdsBuffer.Length)
				ccsdsBuffer = new byte[length + 6];
			ccsdsBuffer[1] = (byte)apId;
			PutNetwork.USHORT(ccsdsBuffer, 4, (ushort)(length - 1));
			Buffer.BlockCopy(buffer, offset, ccsdsBuffer, 6, length);
			int len = udp.Write(ccsdsBuffer, 0, length + 6,null);
		}

		void OnMsgTransportRead(byte[] buffer, int offset, int length)
		{
			parseAspireHdr.Unmarshal(buffer, offset, length);
			switch ((ProtocolId)parseAspireHdr.Version)
			{
				case ProtocolId.Aspire:
				case ProtocolId.XtedsCommand:
				case ProtocolId.XtedsData:
					SendCcsdsOnUdp(buffer,offset,length,parseAspireHdr.Source.Hash);
					break;
			}
		} byte[] ccsdsBuffer = new byte[0];

		void ParseCcsds(byte[] bytes)
		{
			parseCcsds.Unmarshal(bytes);
			if (parseCcsds.ApId == 0x50 && hpiu != null)
				hpiu.ParseHost(bytes, 6);
			else
			{
				if (Transport != null)
				{
					parseAddress.Hash = (uint)parseCcsds.ApId;
					parseAddress.SetAddressPort(0x0100007f, parseCcsds.ApId);
					Transport.Write(bytes, parseCcsds.DataOffset, parseCcsds.DataLength, parseAddress);
				}
				else
				{
					parseAddress.Hash = (uint)parseCcsds.ApId+4;
					Transport transport;
					if (transportsByApId.TryGetValue(parseAddress.Hash, out transport))
						transport.Write(bytes, parseCcsds.DataOffset, parseCcsds.DataLength, parseAddress);
				}
			}
		}

		#region Properties

		[XmlAttribute("localUdpPort")]
		public int LocalUdpPort { get; set; }

		public Transport Transport { get; set; }

		[XmlElement("InputTransport",typeof(Transport))]
		public List<Transport> InputTransports { get; set; }

		[XmlAttribute("remoteUdpPort")]

		public string RemoteUdpPort { get; set; }

		[XmlIgnore]
		public uint BrowserApId { get; set; }

		[XmlIgnore]
		public string BrowserTest
		{
			get { return mBrowserTest; }
			set
			{
				mBrowserTest = value;
				int length = mBrowserTest.Length + 2;
				byte[] buffer = new byte[length];
				buffer[0] = (byte)'&';
				var bytes = ASCIIEncoding.ASCII.GetBytes(mBrowserTest);
				Buffer.BlockCopy(bytes,0, buffer, 1, bytes.Length);
				buffer[length - 1] = 0;
				parseAddress.Hash = BrowserApId+4;
//				parseAddress.SetAddressPort(0x0100007f, (int)BrowserApId);

				Transport transport;
				if ( transportsByApId.TryGetValue(BrowserApId+4, out transport) )
					transport.Write(buffer, 0, length, parseAddress);
			}
		} string mBrowserTest;

		#endregion
	}

	class CcsdsHdr
	{
		internal int ApId { get { return (mHeader[0] & 7) << 8 | mHeader[1]; } }

		internal int DataOffset { get { return 6; } }

		internal int DataLength { get { return GetNetwork.USHORT(mHeader, 4) + 1; } }

		internal void Unmarshal(byte[] packet)
		{
			mHeader = packet;
		} byte[] mHeader;
	}

}
