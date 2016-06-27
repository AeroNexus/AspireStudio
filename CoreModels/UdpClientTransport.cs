using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml.Serialization;

using Aspire.Core.Utilities;
using Aspire.Utilities;

namespace Aspire.Core.Messaging
{
	public class UdpClientTransport : Transport
	{
		public delegate void ReceiveHandler(byte[] buffer);

		[XmlIgnore]
		public ReceiveHandler OnReceive;

		int mId, mPort, mReadTimeoutMs;
		Address listenAddress;
		IPEndPoint endpoint, remoteEndpoint = new IPEndPoint(IPAddress.Loopback,0);
		Socket udpSocket;
		UdpClient udp;

		public UdpClientTransport()
			: this(0)
		{
		}

		public UdpClientTransport(int port) : base("udp client")
		{
			Port = port;
		}

		public UdpClientTransport(Address address) : base("udp client")
		{
			listenAddress = address.Clone();
			long nda;
			mPort = listenAddress.GetAddressPort(out nda);
			if (mPort != 0)
			{
				mId = (short)(mPort % 100);
				listenAddress.Hash = (uint)mId;
			}
		}

		public override int Close()
		{
			if ( udp != null )
				udp.Close();
			udp = null;
			udpSocket = null;
			return 0;
		}

		[XmlAttribute("crc")]
		public bool CRC { get; set; }

		[XmlAttribute("destination")]
		public string DestinationAddress
		{
			get { return remoteEndpoint.ToString();  }
			set
			{
				var tokens = value.Split(':');
				if ( tokens.Length == 1 )
					destinationPort = int.Parse(tokens[0]);
				else if ( tokens.Length == 2 )
				{
					destinationPort = int.Parse(tokens[1]);
					remoteEndpoint = new IPEndPoint(IPAddress.Parse(tokens[0]),destinationPort);
				}
			}
		} int destinationPort;

		public override Address ListenAddress { get { return listenAddress; } }

		public override string ListenAddressString { get { return listenAddress.ToString(); } }

		void OnUdpReceive(IAsyncResult result)
		{
			byte[] bytes;
			try
			{
				if (udp == null) return;
				bytes = udp.EndReceive(result, ref endpoint);
				// Don't think CRC needs to be handled here
			}
			catch (SocketException e)
			{
				udp.BeginReceive(new AsyncCallback(OnUdpReceive), null);
				Log.WriteLine("{0} {1}: {2}", mPort, endpoint.Port, e.Message);
				return;
			}
			catch (ArgumentException)
			{
				udp.BeginReceive(new AsyncCallback(OnUdpReceive), null);
				return;
			}
			catch (ObjectDisposedException)
			{
				return;
			}
			OnReceive(bytes);
			//if (debug > 0)
			//    MsgConsole.WriteLine("{0} shell <= {1}", asim.Name, MsgLabel(bytes[0]));
			udp.BeginReceive(new AsyncCallback(OnUdpReceive), null);
		}

		public override int Open()
		{
			//MsgConsole.WriteLine("Open");
			try
			{
				udp = new UdpClient(mPort, AddressFamily.InterNetwork);
				udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			}
			catch (SocketException ex)
			{
				if (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
				{
					Log.WriteLine("Can't open a UdpClient @ {0}", mPort);
					return -1;
				}
			}
			catch (Exception ex)
			{
				Log.ReportException(ex, "UdpClientTransport.{0}", mPort);
				return -1;
			}


			if (OnReceive != null)
				udp.BeginReceive(OnUdpReceive, this);
			else
			{
				udpSocket = udp.Client;
				udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, mReadTimeoutMs);
			}
			return 0;
		}

		[XmlAttribute("port")]
		public int Port
		{
			get { return mPort; }
			set
			{
				mPort = value;
				if (mPort != 0)
				{
					mId = (short)(mPort % 100);
					listenAddress = new Address2("127.0.0.1", (ushort)mPort, (ushort)mId);
				}
			}
		}

		public override int Read(byte[] buffer, SecTime timeout)
		{
			int len = 0;
			try
			{
				if (udpSocket == null) return 0;
				len = udpSocket.Receive(buffer);
				// Don't think CRC needs to be handled here
			}
			catch (SocketException ex)
			{
				if (ex.SocketErrorCode == SocketError.TimedOut) return 0; // timed out
			}
			catch (System.Threading.ThreadAbortException) { }
			catch (NullReferenceException) { }
			catch (Exception ex)
			{
				Log.ReportException(ex, "UdpClientTransport.{0}", mPort);
			}
			return len;
		}

		public override SecTime ReadTimeout
		{
			get { return base.ReadTimeout; }
			set
			{
				base.ReadTimeout = value;
				mReadTimeoutMs = ReadTimeout.ToMilliSeconds;
				if (udpSocket != null)
					udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, mReadTimeoutMs);
			}
		}

		public int Send(Message message, byte[] buffer, int payload, int length, Address agent)
		{
			long address;
			int port;
			if (agent != null)
				port = agent.GetAddressPort(out address);
			else if (message.Destination != null)
				port = message.Destination.GetAddressPort(out address);
			else
			//{
			//	var mId = new MessageId();
			//	mId.SetHash(message.Selector, message.ShiftWidth);
			//	MsgConsole.WriteLine("Can't send message {0}: no destination", mId.ToString());
				return 0;
			//}

			if (destinationPort > 0) port = destinationPort;
			remoteEndpoint.Port = port;

			int msgSize = message.Size;
			int datagramStart = payload - msgSize;
			int len = message.Marshal(buffer, datagramStart, length + msgSize, length, null);
			if (CRC)
			{
				ushort crc = IPchecksum(buffer, datagramStart);
				PutNetwork.USHORT(buffer, len, crc);
				len += 2;
			}
			//if (LogOutput != null)
			//	LogOutput(message, buffer, datagramStart, len);
			if (datagramStart > 0)
			{
				if (len > mSendBuffer.Length)
					mSendBuffer = new byte[len];
				Buffer.BlockCopy(buffer, datagramStart, mSendBuffer, 0, len);
				return udp.Send(mSendBuffer, len, remoteEndpoint);
			}
			else if ( udp != null )
				return udp.Send(buffer, len, remoteEndpoint);
			return 0;
		} byte[] mSendBuffer = new byte[0];

		public override int Write(byte[] buffer, int offset, int length, Address destination, SecTime timeout)
		{
			long address;
			int port = 0;
			if ( destination != null ) port = destination.GetAddressPort(out address);
			if (destinationPort > 0) port = destinationPort;
			remoteEndpoint.Port = port;

			if (CRC)
			{
				ushort crc = IPchecksum(buffer, offset);
				PutNetwork.USHORT(buffer, length, crc);
				length += 2;
			}
			if (offset > 0)
			{
				if (length > mSendBuffer.Length)
					mSendBuffer = new byte[length];
				Buffer.BlockCopy(buffer, offset, mSendBuffer, 0, length);
				return udp.Send(mSendBuffer, length, remoteEndpoint);
			}
			else if ( udp != null )
				return udp.Send(buffer, length, remoteEndpoint);
			return 0;
		}

		#region IReliableTransport

		public override bool SupportsReliableDelivery { get { return false; } }
		public override bool SupportsBestEffortDelivery { get { return true; } }
		public override bool SupportsBroadcast { get { return true; } }
		public override bool SupportsMulticast { get { return true; } }

		#endregion

		#region CRC

		static byte[] RMAP_CRCTable = new byte[]{
			0x00,0x07,0x0e,0x09,0x1c,0x1b,0x12,0x15,
			0x38,0x3f,0x36,0x31,0x24,0x23,0x2a,0x2d,
			0x70,0x77,0x7e,0x79,0x6c,0x6b,0x62,0x65,
			0x48,0x4f,0x46,0x41,0x54,0x53,0x5a,0x5d,
			0xe0,0xe7,0xee,0xe9,0xfc,0xfb,0xf2,0xf5,
			0xd8,0xdf,0xd6,0xd1,0xc4,0xc3,0xca,0xcd,
			0x90,0x97,0x9e,0x99,0x8c,0x8b,0x82,0x85,
			0xa8,0xaf,0xa6,0xa1,0xb4,0xb3,0xba,0xbd,
			0xc7,0xc0,0xc9,0xce,0xdb,0xdc,0xd5,0xd2,
			0xff,0xf8,0xf1,0xf6,0xe3,0xe4,0xed,0xea,
			0xb7,0xb0,0xb9,0xbe,0xab,0xac,0xa5,0xa2,
			0x8f,0x88,0x81,0x86,0x93,0x94,0x9d,0x9a,
			0x27,0x20,0x29,0x2e,0x3b,0x3c,0x35,0x32,
			0x1f,0x18,0x11,0x16,0x03,0x04,0x0d,0x0a,
			0x57,0x50,0x59,0x5e,0x4b,0x4c,0x45,0x42,
			0x6f,0x68,0x61,0x66,0x73,0x74,0x7d,0x7a,
			0x89,0x8e,0x87,0x80,0x95,0x92,0x9b,0x9c,
			0xb1,0xb6,0xbf,0xb8,0xad,0xaa,0xa3,0xa4,
			0xf9,0xfe,0xf7,0xf0,0xe5,0xe2,0xeb,0xec,
			0xc1,0xc6,0xcf,0xc8,0xdd,0xda,0xd3,0xd4,
			0x69,0x6e,0x67,0x60,0x75,0x72,0x7b,0x7c,
			0x51,0x56,0x5f,0x58,0x4d,0x4a,0x43,0x44,
			0x19,0x1e,0x17,0x10,0x05,0x02,0x0b,0x0c,
			0x21,0x26,0x2f,0x28,0x3d,0x3a,0x33,0x34,
			0x4e,0x49,0x40,0x47,0x52,0x55,0x5c,0x5b,
			0x76,0x71,0x78,0x7f,0x6a,0x6d,0x64,0x63,
			0x3e,0x39,0x30,0x37,0x22,0x25,0x2c,0x2b,
			0x06,0x01,0x08,0x0f,0x1a,0x1d,0x14,0x13,
			0xae,0xa9,0xa0,0xa7,0xb2,0xb5,0xbc,0xbb,
			0x96,0x91,0x98,0x9f,0x8a,0x8d,0x84,0x83,
			0xde,0xd9,0xd0,0xd7,0xc2,0xc5,0xcc,0xcb,
			0xe6,0xe1,0xe8,0xef,0xfa,0xfd,0xf4,0xf3,
		};
		public static byte Crc8(byte[] bytes, int offset, int length)
		{
			byte crc = 0;

			for (int i = offset; i < length + offset; i++)
				crc = RMAP_CRCTable[crc ^ (bytes[i] & 0xff)];
			return crc;
		}

		/* This is the code from RFC 1071, fixed for endianess and specialized for ip headers */
		public static ushort IPchecksum(byte[] hdr, int offset)
		{
			int sum = 0;
			int i = offset;

			for (int curWord = 0; curWord < 10; curWord++)
			{
				sum += (0xFFFF & ((hdr[i] << 8) | hdr[i + 1]));
				i += 2;
			}
			while ((sum >> 16) != 0)
				sum = (sum & 0xFFFF) + (sum >> 16);

			return (ushort)(sum ^ 0xFFFF);
		}

		#endregion


	}
}

