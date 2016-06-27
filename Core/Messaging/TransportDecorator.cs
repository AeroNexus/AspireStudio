﻿using System;

using Aspire.Core.Utilities;

namespace Aspire.Core.Messaging
{
	public abstract class TransportDecorator : Transport
	{
		public TransportDecorator(Transport transport)
			: base(transport.Name)
		{
			mTransport = transport;
		}

		// ITransport
		public override int Close() { return mTransport.Close(); }
		public override int Open() { return mTransport.Open(); }
		public override int Read(byte[] buffer, SecTime timeout)
		{
			return mTransport.Read(buffer, timeout);
		}
		public override int Write(byte[] buffer, int offset, int length, Address destination, SecTime timeout)
		{
			return mTransport.Write(buffer, offset, length, destination, timeout);
		}

		public override string Name { get { return mTransport.Name; } }
		public override Address ListenAddress { get { return mTransport.ListenAddress; } }
		public override string ListenAddressString { get { return mTransport.ListenAddressString; } }

		// Transport
		public override SecTime ReadTimeout
		{
			get { return mTransport.ReadTimeout; }
			set { mTransport.ReadTimeout = value; }
		}
		public override SecTime WriteTimeout
		{
			get { return mTransport.WriteTimeout; }
			set { mTransport.WriteTimeout = value; }
		}

		public override int Read(byte[] buffer) { return mTransport.Read(buffer); }
		public override int Write(byte[] buffer, int offset, int length, Address destination)
		{ return mTransport.Write(buffer, offset, length, destination); }

		public override bool SupportsReliableDelivery { get { return mTransport.SupportsReliableDelivery; } }
		public override bool SupportsBestEffortDelivery { get { return mTransport.SupportsBestEffortDelivery; } }
		public override bool SupportsBroadcast { get { return mTransport.SupportsBroadcast; } }
		public override bool SupportsMulticast { get { return mTransport.SupportsMulticast; } }

		protected Transport mTransport;
		//~TransportDecorator() {}
	}
}
