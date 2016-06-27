using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Xml.Serialization;

using Aspire.Core.Utilities;
using Aspire.Utilities;

namespace Aspire.Core.Messaging
{
	public class MsgTransport : Transport
	{
		static Dictionary<int, MsgTransport> transportsByPort = new Dictionary<int, MsgTransport>();

		public delegate void ReadPacketHandler(byte[] buffer, int offset, int length);

		[XmlIgnore]
		public ReadPacketHandler HandleReadPacket;

		AutoResetEvent readAvailable = new AutoResetEvent(false);
		int mId, mPort;
		Address listenAddress;
		Queue<MarshaledBuffer> readPackets = new Queue<MarshaledBuffer>();

		public MsgTransport()
			: this(0)
		{
			HandleReadPacket = QueueReadPacket;
		}

		public MsgTransport(int port) : base ("msg Q")
		{
			Port = port;
		}

		public MsgTransport(Address address) : base("msg Q")
		{
			HandleReadPacket = QueueReadPacket;
			listenAddress = address.Clone();
			long nda;
			mPort = listenAddress.GetAddressPort(out nda);
			if (mPort != 0)
			{
				transportsByPort[mPort] = this;
				mId = mPort%100;
				listenAddress.Hash = (uint)mId;
			}
		}

		public override int Close()
		{
			transportsByPort.Remove(mPort);
			readPackets.Clear();
			return 0;
		}

		[XmlAttribute("destination"),DefaultValue(0)]
		public int DestinationPort { get; set; }

		public override Address ListenAddress { get { return listenAddress; } }

		public override string ListenAddressString { get { return listenAddress.ToString(); } }

		public override int Open()
		{
			//MsgConsole.WriteLine("Open");
			return 0;
		}

		int readTimeoutMs = 1000;

		[XmlAttribute("port"), DefaultValue(0)]
		public int Port
		{
			get { return mPort; }
			set
			{
				mPort = value;
				if (mPort != 0)
				{
					transportsByPort[mPort] = this;
					mId = mPort%100;// transportsByPort.Count;
					listenAddress = new Address2("127.0.0.1", (ushort)mPort, (ushort)mId);
				}
			}
		}

		void QueueReadPacket(byte[] buffer, int offset, int length)
		{
			byte[] buf = new byte[length];
			Buffer.BlockCopy(buffer, offset, buf, 0, length);
			var ioPkt = new MarshaledBuffer(length, buf, 0);
			lock (readPackets)
				readPackets.Enqueue(ioPkt);
			readAvailable.Set();
		}

		public override int Read(byte[] buffer,SecTime timeout)
		{
			MarshaledBuffer ioPkt;
			if (readPackets.Count > 0)
			{
				lock (readPackets)
					ioPkt = readPackets.Dequeue();
				Buffer.BlockCopy(ioPkt.Bytes, 0, buffer, 0, ioPkt.Length);
				return ioPkt.Length;
			}
			readAvailable.WaitOne(timeout.ToMilliSeconds);
			if (readPackets.Count > 0)
			{
				lock (readPackets)
					ioPkt = readPackets.Dequeue();
				Buffer.BlockCopy(ioPkt.Bytes, 0, buffer, 0, ioPkt.Length);
				return ioPkt.Length;
			}
			else
				return 0;
		}

		public override SecTime ReadTimeout
		{
			get { return mReadTimeout; }
			set { mReadTimeout = value; readTimeoutMs = mReadTimeout.ToMilliSeconds; }
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
			{
				var mId = new MessageId();
				mId.SetHash(message.Selector, message.ShiftWidth);
				MsgConsole.WriteLine("Can't send message {0}: no destination", mId.ToString());
				return 0;
			}
			MsgTransport receiver;

			if (DestinationPort > 0) port = DestinationPort;

			if (transportsByPort.TryGetValue(port, out receiver))
			{
				int msgSize = message.Size;
				int datagramStart = payload - msgSize;
				int len = message.Marshal(buffer, datagramStart, length + msgSize, length, null);
				//Log(dest,datagram,len);
				//if ( dump ) Dump();
				//if (LogOutput != null)
				//	LogOutput(message, buffer, datagramStart, len);
				receiver.HandleReadPacket(buffer, datagramStart, len);
				return len;
			}

			return 0;
		}

		public override int Write(byte[] buffer, int offset, int length, Address destination, SecTime timeout)
		{
			long address;
			int port = destination.GetAddressPort(out address);
			if (DestinationPort > 0) port = DestinationPort;

			MsgTransport receiver;

			if (transportsByPort.TryGetValue(port, out receiver))
			{
				receiver.HandleReadPacket(buffer, offset, length);
				return length;
			}

			return 0;
		}

		#region IReliableTransport

		public override bool SupportsReliableDelivery { get { return true; } }
		public override bool SupportsBestEffortDelivery { get { return false; } }
		public override bool SupportsBroadcast { get { return false; } }
		public override bool SupportsMulticast { get { return false; } }

		#endregion

	}
}

