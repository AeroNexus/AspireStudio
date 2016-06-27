using System;
using System.Net;
using System.Net.Sockets;

using Aspire.Core.Utilities;

namespace Aspire.Core.Messaging
{
	public class Address2 : Address
	{
		UInt32 mNda;
		UInt16 mNdi, mId;
		
		const int marshalSize = 8;

		public Address2()
		{
		}

		public Address2(Address2 src)
		{
			mNda = src.mNda;
			mNdi = src.mNdi;
			mId = src.mId;
		}

		public Address2(string nda) : this(nda, 0, 0) { }
		public Address2(string nda, UInt16 ndi) : this(nda, ndi, 0) { }
		public Address2(string nda, UInt16 ndi, UInt16 id)
		{
			mNdi = ndi;
			mId = id;
			var ipAddress = IPAddress.Parse(nda);
			mNda = BitConverter.ToUInt32(ipAddress.GetAddressBytes(),0);
		}

		public Address2(UInt32 nda) : this(nda, 0, 0) { }
		public Address2(UInt32 nda, UInt16 ndi) : this(nda, ndi, 0) { }
		public Address2(UInt32 nda, UInt16 ndi, UInt16 id)
		{
			mNda = nda;
			mNdi = ndi;
			mId = id;
		}

		public override Address Clone()
		{
			return new Address2(this);
		}

		public override void CopyTo(Address destination)
		{
			var dest = destination as Address2;
			dest.mNdi = mNdi;
			dest.mId = mId;
			dest.mNda = mNda;
		}

		public override AddressList CreateList()
		{
			return new AddressList();
		}

		public override AddressListItem CreateListItem()
		{
			var item = new Address2ListItem();
			item.address = this;
			return item;
		}

		public override bool Equals(object obj)
		{
			Address2 rhs = obj as Address2;
			return rhs != null && mNda == rhs.mNda && mNdi == rhs.mNdi && mId == rhs.mId;
		}

		public override bool EqualsNetwork(Address address)
		{
			Address2 rhs = address as Address2;
			return rhs != null && mNda == rhs.mNda && mNdi == rhs.mNdi;
		}

		public override int GetAddressPort(out long address)
		{
			address = mNda;
			return mNdi;
		}

		public override int GetHashCode()
		{
			return mId;
		}

		public override UInt32 Hash
		{
			get { return mId; }
			set { mId = (UInt16)value; }
		}

		private static uint IpWithRespectTo(uint nda)
		{
			var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

			var endPoint = new IPEndPoint(nda, 0);
			sock.Connect(endPoint);
			var ip = BitConverter.ToUInt32((sock.LocalEndPoint as IPEndPoint).Address.GetAddressBytes(), 0);
			sock.Close();
			return ip;
		}

		public override bool IsLocal
		{
			get { return (IpWithRespectTo(mNda) == mNda); }
		}

		public override bool IsLoopback
		{
			get { return mNda == 0x0100007f; }
		}

		public override int Length
		{
			get { return marshalSize; }
		}

		public override int Marshal(byte[] buffer, int offset)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(mNda), 0, buffer, offset, 4);
			PutNetwork.USHORT(buffer, offset+4, mNdi);
			PutNetwork.USHORT(buffer, offset+6, mId);
			return marshalSize;
		}

		public override int Marshal(MarshaledBuffer marshaledBuffer)
		{
			byte[] buffer = marshaledBuffer.Bytes;
			int offset = marshaledBuffer.Offset;
			Buffer.BlockCopy(BitConverter.GetBytes(mNda), 0, buffer, offset, 4);
			PutNetwork.USHORT(buffer, offset+4, mNdi);
			PutNetwork.USHORT(buffer, offset+6, mId);
			return marshalSize;
		}

		public ushort Ndi { get { return mNdi; } }

		public override uint NetworkDependentAddress { get { return mNda; } }

		public override void Parse(string text)
		{
			int start=0, state = 0;
			for(int i=0; i<text.Length; i++)
			{
				if ( text[i] == ':' )
				{
					switch ( state )
					{
					case 0:
						if (i > start)
						{
							var ipAddress = IPAddress.Parse(text.Substring(start, i-start));
							mNda = BitConverter.ToUInt32(ipAddress.GetAddressBytes(), 0);
						}
						start = i+1;
						state = 1;
						break;
					case 1:
						mNdi = UInt16.Parse(text.Substring(start,i-start));
						start = i+1;
						state = 2;
						break;
					}
				}
			}
			if ( state == 1 )
				mNdi = UInt16.Parse(text.Substring(start));
			else if ( state == 2 )
				mId = UInt16.Parse(text.Substring(start));
		}

		public override ProtocolId ProtocolId { get { return ProtocolId.Aspire; } }

		public override void SetAddressPort(long address, int port)
		{
			if (address >= 0)
				mNda = (UInt32)address;
			mNdi = (UInt16)port;
		}

		public override string ToString()
		{
			var ipAddress = new IPAddress((long)mNda);
			return String.Format("{0}:{1}:{2}", ipAddress.ToString(), mNdi, mId);
		}

		public override int Unmarshal(byte[] buffer, int offset)
		{
			mNda = BitConverter.ToUInt32(buffer, offset); // Keep in network byte order
			mNdi = GetNetwork.USHORT(buffer, offset+4);
			mId = GetNetwork.USHORT(buffer, offset+6);
			return marshalSize;
		}

		public override int Unmarshal(MarshaledBuffer marshaledBuffer)
		{
			byte[] buffer = marshaledBuffer.Bytes;
			int offset = marshaledBuffer.Offset;
			mNda = BitConverter.ToUInt32(buffer, offset); // Keep in network byte order
			mNdi = GetNetwork.USHORT(buffer, offset+4);
			mId = GetNetwork.USHORT(buffer, offset+6);
			return marshalSize;
		}

		public override Address WithRespectTo(Address rhs) 
		{
			mNda = IpWithRespectTo(rhs.NetworkDependentAddress);

			return this;
		}

	}

	public class Address2ListItem : AddressListItem
	{
		public IPEndPoint endpoint;
	}
}
