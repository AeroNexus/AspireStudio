using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using Aspire.Core.Utilities;

namespace Aspire.Core.Messaging
{
	public class UdpTransport : Transport
	{
		const int DatagramSize = 64*1024;

		Address mListenAddress;
		Socket mReadSocket, mWriteSocket;
		IPEndPoint mReadEndPoint, mRemoteEndPoint = new IPEndPoint(0,0);
		IPEndPoint mLocalEndpoint = new IPEndPoint(IPAddress.Loopback, 0);
		int mListenPort;
		bool mIsOpen, mReadOnly;
		//static byte logBuf[2<<16], *logP=logBuf;

		public UdpTransport() : this(0)
		{
		}

		public UdpTransport(int listenPort) : this(listenPort,false)
		{
		}

		public UdpTransport(int listenPort, bool readOnly) : base("udp")
		{
			mListenPort = listenPort;
			mIsOpen = false;
			mReadOnly = readOnly;
		}

		public override int Open()
		{
		    mReadSocket = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
			if ( !mReadOnly )
				mWriteSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			mReadEndPoint = new IPEndPoint(IPAddress.Any, mListenPort);

			try
			{
				mReadSocket.Bind(mReadEndPoint);
			}
			catch (SocketException e)
			{
				MsgConsole.ReportException("UpdTransport.Open cannot bind", MsgLevel.Warning, e);
				return -1;
			}
			mListenPort = (mReadSocket.LocalEndPoint as IPEndPoint).Port;

			var bytes = mReadEndPoint.Address.GetAddressBytes();
			long addr=0;
			if ( bytes.Length == 4 )
				addr = BitConverter.ToInt32(bytes,0);
			else if ( bytes.Length == 8 )
				addr = BitConverter.ToInt64(bytes, 0);
			if (addr != 0)
				mListenAddress = new Address2((uint)addr, (ushort)mListenPort, 0);
			else
				mListenAddress = new Address2(IPAddress.Loopback.ToString(), (ushort)mListenPort,0);

			mReadSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 65536);
			mWriteSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 65536);
			mReadSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			mWriteSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

		    // set MarshaledBuffer sizes
		    // set reuse addr
		    mIsOpen = true;
			return 0;
		}

		public override int Close()
		{
			if (!mIsOpen)
				return -1;

			mIsOpen = false;
			mReadSocket.Close();
			if (!mReadOnly)
				mWriteSocket.Close();
			return 0;
		}

		//static bool dump;

		static void Dump()
		{
		    //FILE* fp = fopen("Dump.txt","w");
		    //for( int i=0; i< logP-logBuf; i += 16 )
		    //{
		    //    fprintf(fp,"\n%08x ",&logBuf[i]);
		    //    for(int j=0; j<16; j++) fprintf(fp,"%02x-",logBuf[i+j]);
		    //}
		    //fprintf(fp,"\n");
		    //fclose(fp);
		    //dump = false;
		}

		private IPAddress GetNodeAddress(AddressFamily af)
		{
			var hostname = Dns.GetHostName();
			var addresses = Dns.GetHostAddresses(hostname);
			if (addresses.Length == 0) return null;

			uint address=0, network=0, node=0;
			int i = 0, which=0;
			foreach (var addr in addresses)
			{
				if (addr.AddressFamily == af)
				{
					address = BitConverter.ToUInt32(addr.GetAddressBytes(), 0);
					if (node == 0)
					{
						network = address&0xffffff;
						node = address&0xff000000;
						which = i;
					}
					else if ((address&0xffffff) == network && (address&0xff000000)<node)
					{
						node = address&0xff000000;
						which = i;
					}
				}
				i++;
			}
			mNodeAddress = network|node;
			if (node > 0)
				return addresses[which];
			else
				return null;
		} uint mNodeAddress;

		public override Address ListenAddress { get { return mListenAddress; } }

		public override string ListenAddressString
		{
			get
			{
				IPAddress address = GetNodeAddress(AddressFamily.InterNetwork);
				var ipAddr = address == null ? "127.0.0.1" : address.ToString();
				return string.Format("{0}:{1}", ipAddr, mListenPort);
			}
		}

		void Log(IPEndPoint dest,byte[] datagram,int length)
		{
		    //memcpy(logP,&dest.sin_addr.s_addr,4);
		    //memcpy(&logP[4],&dest.sin_port,2);
		    //memcpy(&logP[6],dgram,length);
		    //logP += 6+length;
		}

		public override int Read(byte[] datagram, SecTime timeout)
		{
			int length = 0;
			if (mReadSocket.Poll(timeout.ToMicroSeconds, SelectMode.SelectRead))
			{
				try
				{
					length = mReadSocket.Receive(datagram);
				}
				catch (SocketException e)
				{
					MsgConsole.ReportException("UdpTransport.Read",MsgLevel.Warning,e);
				}
				return length;
			}
			return length;
		}

		// This can be global within the process.
		class ArpState
		{
			internal ArpState(uint address, int ping)
			{
				ipAddress = address;
				LastPing = ping;
			}
			internal uint ipAddress;
			internal int LastPing;
		}
		static List<ArpState> arpStates = new List<ArpState>();
		//static List<ArpState> lanArpStates = new List<ArpState>();
		//static int minLanNode = 255;

		static uint ResolveNextHop(uint ipAddress)
		{
			IpHelper.MIB_IPFORWARDROW routeRow = new IpHelper.MIB_IPFORWARDROW();
			int success = IpHelper.GetBestRoute((int)ipAddress, 0, ref routeRow);
			if(0 == success)
			{
				switch(routeRow.ForwardType)
				{
					// INDIRECT -> use next hop
					case IpHelper.ForwardType.MIB_IPROUTE_TYPE_INDIRECT:
						ipAddress = routeRow.ForwardNextHop;
						break;
				}
			}
			return ipAddress;
		}

		static ArpState FindArpState(uint ipAddress)
		{
			foreach ( var arpState in arpStates )
				if ( arpState.ipAddress == ipAddress )
					return arpState;

			var arp = new ArpState(ipAddress,-1000); // make sure that the first use causes a ping
			arpStates.Add(arp);

			return arp;
		}

		SecTime arpTime = new SecTime();
		
		void MaintainArp(uint ipAddress)
		{
			// This is correct, but will result in unnecessary work
			// and it should be possible to skip all this processing
			// for large classes of IPs (multicast, broadcast, loopback).
			// I don't think we care for now since the core problem is
			// the protocol instead of some obscure winsock limitation.
			ipAddress = ResolveNextHop(ipAddress);
			var arp = FindArpState(ipAddress);

			Clock.GetTime(ref arpTime);
			if (arpTime.Seconds-13 >= arp.LastPing)
			{
				byte[] macAddr = new byte[32];
				uint addrLen = 32;
				IpHelper.SendARP((int)arp.ipAddress, (int)mNodeAddress, macAddr, ref addrLen);
			}
			// always update, since ARP cache entry won't expire if written to
			// allow for the time taken in the ping/response
			Clock.GetTime(ref arpTime);
			arp.LastPing = arpTime.Seconds;
		}

		public override int SendToLocal(uint destPort, byte[] datagram, int length)
		{
			mLocalEndpoint.Port = (int)destPort;
			int count = 0;
			try
			{
				count = mWriteSocket.SendTo(datagram, length, SocketFlags.None, mLocalEndpoint);
			}
			catch (SocketException e)
			{
				MsgConsole.ReportException("UdpTransport.SendToLocal", MsgLevel.Warning, e);
			}
			return count;
		}

		IPEndPoint writeEndpoint = new IPEndPoint(IPAddress.Any, 0);

		public override int Write(byte[] payload, int offset, int length, Address destination, SecTime timeout)
		{
			long address;
			writeEndpoint.Port = destination.GetAddressPort(out address);
			writeEndpoint.Address = new IPAddress(address);
			int count;
			try
			{
				count = mWriteSocket.SendTo(payload,offset, length, SocketFlags.None, writeEndpoint);
			}
			catch (SocketException e)
			{
				MsgConsole.ReportException("UdpTransport.Write", MsgLevel.Warning, e);
				count = 0;
			}
		    return count;
		}

		public long WriteIpAddr
		{
			get
			{
				return writeIpAddr;
			}
			set
			{
				writeIpAddr = value;
				writeEndpoint.Address = new IPAddress(writeIpAddr);
			}
		} long writeIpAddr;
		public int WritePort
		{
			get
			{
				return writeEndpoint.Port;
			}
			set
			{
				writeEndpoint.Port = value;
			}
		}

		#region IReliableTransport

		public override bool SupportsReliableDelivery { get { return false; } }
		public override bool SupportsBestEffortDelivery { get { return true; } }
		public override bool SupportsBroadcast { get { return true; } }
		public override bool SupportsMulticast { get { return true; } }

		#endregion

	}
}

internal class IpHelper
{
	internal enum ForwardType {
		/// <summary>
		/// Some other type not specified in RFC 1354.
		/// </summary>
		MIB_IPROUTE_TYPE_OTHER=1, 
		/// <summary>
		/// Some other type not specified in RFC 1354.
		/// </summary>
		MIB_IPROUTE_TYPE_INVALID=2,
		/// <summary>
		/// An invalid route. This value can result from a route added by an ICMP redirect.
		/// </summary>
		MIB_IPROUTE_TYPE_DIRECT=3,
		/// <summary>
		/// A local route where the next hop is the final destination (a local interface).
		/// </summary>
		MIB_IPROUTE_TYPE_INDIRECT=4
		};
	internal struct MIB_IPFORWARDROW
	{
		internal uint ForwardDest;
		internal uint ForwardMask;
		internal uint ForwardPolicy;
		internal uint ForwardNextHop;
		internal uint ForwardIfIndex;
		internal ForwardType ForwardType;
		internal uint ForwardProto;
		internal uint ForwardAge;
		internal uint ForwardNextHopAS;
		internal uint ForwardMetric1;
		internal uint ForwardMetric2;
		internal uint ForwardMetric3;
		internal uint ForwardMetric4;
		internal uint ForwardMetric5;
	};

	[DllImport("iphlpapi.dll", ExactSpelling=true)]
	internal static extern int SendARP(
	  int DestIP, int SrcIP, byte[] pMacAddr, ref uint PhyAddrLen);
	[DllImport("iphlpapi.dll", ExactSpelling=true)]
	internal static extern int GetBestRoute(int destAddress, int srcAddress, ref MIB_IPFORWARDROW routeRow);
}

