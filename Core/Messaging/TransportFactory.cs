/******************************************************************************
*
* (c) Copyright 2010-2011 Aero Nexus Inc.
* All rights reserved.
*     
* http://www/AeroNexus.com, (505) 503-1563
*
******************************************************************************/
using System;

using Aspire.Core.Utilities;

namespace Aspire.Core.Messaging
{
	public class TransportFactory
	{
		public static Transport Create(string name, int port, bool readOnly=false)
		{
			var lowerName = name.ToLower();
			Transport transport = null;

			switch (name.ToLower())
			{
				case "tcp":
					transport = new TcpTransport(port);
					break;
				case "udp":
					transport = new UdpTransport(port,readOnly);
					break;
				default:
					MsgConsole.WriteLine("TransportFactory.Create unknown transport: {0}", name);
					break;
			}

			return transport;
		}

		public static void Destroy(Transport transport)
		{
		}
	}
}
