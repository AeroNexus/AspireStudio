using System;
using System.ComponentModel;

using Aspire.Core.Utilities;

namespace Aspire.Core.Messaging
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public abstract class Protocol
	{
		public delegate void UnknownMessageHandler(string label,Message message);

		Address mOwnAddress;
		protected UnknownMessageHandler mHandleUnknownMessage;
		ProtocolId mId;
		protected Transport mTransport;

		public Protocol(ProtocolId id, Transport transport)
		{
			mId = id;
			mTransport = transport;
			mOwnAddress = transport.ListenAddress.Clone();
			mHandleUnknownMessage = new UnknownMessageHandler(HandleUnknownMessage);
		}

		void HandleUnknownMessage(string label,Message message)
		{
			MsgConsole.WriteLine("{0}:: {1}, from {2}: unknown message selector {3}", label,this,message.Source,message.Selector);
		}

		public virtual ProtocolId Id
		{
			get { return mId; }
		}

		//public abstract int MessageSize { get; }

		public Address OwnAddress { get { return mOwnAddress; } }

		public UInt32 OwnAddressHash { set { mOwnAddress.Hash = value; } }

		public abstract bool Parse(byte[] buffer, int length, Message parseHeader);

		protected virtual int Send(Message header, int msgSelector, byte[] buffer, int payloadOffset,
			int payloadLength, Address destination, int sequence=0)
		{
			if (sequence > 0)
				header.Sequence = sequence;
			header.Selector = msgSelector;
			header.Destination = destination;
			return Send(header, buffer, payloadOffset, payloadLength);
		}

		protected virtual int Send(Message header, byte[] buffer, int payloadOffset, int payloadLength, Address destination = null)
		{
			if (destination == null)
				destination = header.Destination;

			int msgSize = header.Size;
			int offset = payloadOffset - msgSize;
			int len = header.Marshal(buffer, offset, payloadLength + msgSize, payloadLength, null);
			return mTransport.Write(buffer, offset, len, destination);
		}

		protected virtual int Send(Message header, byte[] buffer, int payloadOffset, int length, SecTime timeout,
			Address destination=null)
		{
			if ( destination == null )
				destination = header.Destination;

			int msgSize = header.Size;
			int offset = payloadOffset - msgSize;
			int len = header.Marshal(buffer, offset, length + msgSize, length, null);
			return mTransport.Write(buffer, offset, len, destination, timeout);
		}

		//public virtual int SendMultiple(int msgSelector, byte[] buffer, int payload,
		//	int payloadLength, AddressList destinations)
		//{
		//	//mInternalMessage.Sequence = 0;
		//	//mInternalMessage.Selector = msgSelector;
		//	////mInternalMessage.Destination = destination;
		//	//return mTransport.SendMultiple(mInternalMessage,buffer, payload, payloadLength,destinations);
		//	return 0;
		//}

		// This was in UDP transport and needs to be re-written at the Protocol level and merged with above
		//public override int SendMultiple(Message message, byte[] buffer, int payload, int length, AddressList destinations)
		//{
		//	int datagramStart = payload - message.Size;
		//	int len = message.Marshal(buffer, datagramStart, length+message.Size, length, null);
		//	int count=0, n=destinations.Count;
		//	long address;

		//		for (int i=0; i<n; i++)
		//		{
		//			Address2ListItem dest = destinations[i] as Address2ListItem;
		//			if (dest.enabled)
		//			{
		//				if (dest.endpoint == null)
		//				{
		//					int port = dest.address.GetAddressPort(out address);
		//					dest.endpoint = new IPEndPoint(address, port);
		//				}
		//				message.MarshalDestination(dest.address, buffer, datagramStart);
		//				//if (LogOutput != null)
		//				//	LogOutput(message, buffer, datagramStart, len);
		//				count = mTransport.Write(buffer, datagramStart, len, SocketFlags.None, dest.endpoint);
		//			}
		//		}

		//	return count;
		//}

		public virtual int SendVia(Address agent, Message header, int msgSelector, byte[] buffer,
			int payloadStart, int payloadLength, Address destination, int sequence)
		{
			if (sequence > 0) header.Sequence = sequence;
			header.Selector = msgSelector;
			header.Destination = destination;
			return Send(header, buffer, payloadStart, payloadLength, agent);
		}

		public Transport Transport { get { return mTransport; } }

		void WhenUnknownMessage(UnknownMessageHandler handleUnknownMessage)
		{
			mHandleUnknownMessage = handleUnknownMessage;
		}
	}
}
