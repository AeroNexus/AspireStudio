using System;
using System.Net;
////using System.Net.Sockets;

using Aspire.Core.Utilities;

namespace Aspire.Core.Messaging
{
	public class Address1 : Address
	{
		byte[] swapBuf = new byte[8];
		UInt32 mAddress;
		Type mType = Type.Unknown;
		
		const int marshalSize = 4;
		static long localInetAddress;
		static string[] typeLabel = new string[]{ "???", "Local", "LL Local", "Spacewire", "LL SpaceWire", "I2C", "Logical", "Self" };

		static Address1()
		{
			var bytes = IPAddress.Loopback.GetAddressBytes();
			if ( bytes.Length == 4 )
				localInetAddress = BitConverter.ToUInt32(bytes, 0);
			else
				localInetAddress = BitConverter.ToInt64(bytes, 0);
		}

		public Address1()
		{
		}

		public Address1(Address1 src)
		{
			mAddress = src.mAddress;
			mType = src.mType;
		}

		public override Type AddressType { get { return mType; } }

		public override Address Clone()
		{
			return new Address1(this);
		}

		public override void CopyTo(Address destination)
		{
			var dest = destination as Address1;
			dest.mAddress = mAddress;
			dest.mType = mType;
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
			Address1 rhs = obj as Address1;
			return rhs != null && mType == rhs.mType && mAddress == rhs.mAddress;
		}

		public override bool EqualsNetwork(Address address)
		{
			Address1 rhs = address as Address1;
			return rhs != null && mType == rhs.mType && mAddress == rhs.mAddress;
		}

		public override int GetAddressPort(out long address)
		{
			switch (mType)
			{
				case Type.LowLevelLocal:
				case Type.Local:
				case Type.Self:
					address = localInetAddress; // for now. Other transports need something different here
					return (int)mAddress;
				default:
					address = 0;
					return 0;
			}
		}

		public override void SetAddressPort(long address, int port)
		{
			mAddress = (uint)port;
		}

		public override int GetHashCode()
		{
			return (int)mAddress;
		}

		public override UInt32 Hash
		{
			get { return mAddress; }
			set { mAddress = value; }
		}

		public override bool IsLocal
		{
			get { return true; }
		}

		public override bool IsLoopback
		{
			get
			{
				switch (mType)
				{
					case Type.Self:
					case Type.Local:
					case Type.LowLevelLocal:
						return true;
				}

				return false;
			}
		}

		public override int Length
		{
			get { return 0; }
		}

		public override int Marshal(byte[] buffer, int offset)
		{
			PutNetwork.UINT(buffer, offset, mAddress);
			return marshalSize;
		}

		public override int Marshal(MarshaledBuffer marshaledBuffer)
		{
			PutNetwork.UINT(marshaledBuffer.Bytes, marshaledBuffer.Offset, mAddress);
			return marshalSize;
		}

		public int MarshalSize { get { return marshalSize; } }

		public override void Parse(string text)
		{
			mAddress = 0;
			if ( text == "???" )
				mType = Type.Unknown;
			else
			{
				string[] str = text.Split(':', ' ');
				try
				{
					mType = (Type)Enum.Parse(typeof(Type), str[0]);
				}
				catch (Exception)
				{
					mType = Type.Unknown;
				}
				finally
				{
					if ( str.Length > 1 )
						mAddress = uint.Parse(str[1]);
				}
			}
		}

		public override ProtocolId ProtocolId { get { return ProtocolId.Monarch; } }

		public override string ToString()
		{
			switch (mType)
			{
				case Type.Unknown: return "???";
				case Type.Self: return typeLabel[(int)mType];
				case Type.Local:
				case Type.LowLevelLocal:
				case Type.Logical:
					return string.Format("{0}: {1}", typeLabel[(int)mType], mAddress);
				default:
					return string.Format("{0}: {1} {2}.{3}", typeLabel[(int)mType], mAddress,
						mAddress>>16,mAddress&0xffff);
			}
		}

		public override void Set(Type type, uint value)
		{
			 mType = type;
			 mAddress = value;
		}

		public override int Unmarshal(byte[] buffer, int offset)
		{
			mAddress = GetNetwork.UINT(buffer, offset, swapBuf);
			return marshalSize;
		}

		public override int Unmarshal(MarshaledBuffer marshaledBuffer)
		{
			mAddress = GetNetwork.UINT(marshaledBuffer.Bytes, marshaledBuffer.Offset, swapBuf);
			return marshalSize;
		}

		public override Address WithRespectTo(Address rhs) {return this;}

	}

	public class Address1ListItem : AddressListItem
	{
		public IPEndPoint endpoint;
	}
}
