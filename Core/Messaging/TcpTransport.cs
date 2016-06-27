using System;
using System.Net;

using Aspire.Core.Utilities;

namespace Aspire.Core.Messaging
{
	public partial class TcpTransport : Transport
	{
		int mPort;
		TcpPeerToPeerClient mClient;
		Address2 mListenAddress = new Address2("127.0.0.1");

		public TcpTransport(int listenPort) :
			base("tcp", new SecTime(1, 0), new SecTime(1, 0))
		{
			mPort = listenPort;
			mClient = new TcpPeerToPeerClient(listenPort);
			Open();
		}

		~TcpTransport()
		{
			mClient.Stop();
		}

		public override int Open()
		{
			if (mClient.Start() != 0)
				return -1;

			mListenAddress.SetAddressPort(0,mPort);
			MsgConsole.WriteLine("TCP listening on {0}", mClient.Port);
			return 0;
		}

		public override int Close()
		{
			return mClient.Stop();
		}

		public override int Read(byte[] buf, SecTime timeout)
		{
			int rlen;
			lock (this)
				rlen = mClient.Read(buf);
			return rlen;
		}

		public override int Write(byte[] buf, int offset, int length, Address destination, SecTime timeout)
		{
			long ip;
			int port = destination.GetAddressPort(out ip);

			var endPoint = new IPEndPoint(ip, port);
			int wlen;
			lock (this)
				wlen = mClient.Write(buf, offset, length, endPoint);
			return wlen;
		}

		public override Address ListenAddress
		{
			get
			{
				if (mLocalAddress == null)
				{
					var endPoint = mClient.LocalEndpoint;
					if (endPoint != null)
					{
						long addr = BitConverter.ToInt64(endPoint.Address.GetAddressBytes(), 0);
						mLocalAddress = new Address2((uint)addr, (ushort)endPoint.Port);
					}
				}
				return mLocalAddress;
			}
		} Address mLocalAddress;

		public override string ListenAddressString
		{
			get { return string.Format("127.0.0.1:{0}", mClient.Port); }
		}

		public override bool SupportsReliableDelivery { get { return true; } }
		public override bool SupportsBestEffortDelivery { get { return false; } }
		public override bool SupportsBroadcast { get { return false; } }
		public override bool SupportsMulticast { get { return false; } }

	}
}
