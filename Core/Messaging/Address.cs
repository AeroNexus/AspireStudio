using System;
using System.Collections.Generic;

using Aspire.Core.Utilities;

namespace Aspire.Core.Messaging
{
	public abstract class Address
	{
		public enum Type { Unknown, Local, LowLevelLocal, Spacewire, LowLevelSpaceWire, I2C, Logical, Self };

		public virtual Type AddressType { get { return Type.Unknown; } }

		public abstract Address Clone();

		public abstract void CopyTo(Address destination);

		public abstract AddressList CreateList();

		public abstract AddressListItem CreateListItem();
		
		//Equals is overridden in derived classes

		public abstract bool EqualsNetwork(Address rhs);

		public static Address Empty;
		
		public abstract int GetAddressPort(out long address);

		public abstract UInt32 Hash { get; set; }

		public abstract bool IsLocal { get; }

		public abstract bool IsLoopback { get; }

		public abstract int Length { get; }

		public abstract int Marshal(byte[] buffer, int offset);

		public abstract int Marshal(MarshaledBuffer marshaledBuffer);

		public virtual uint NetworkDependentAddress { get { return 0; } }

		public abstract void Parse(string text);

		public abstract ProtocolId ProtocolId { get; }

		public abstract void SetAddressPort(long address, int port);

		public virtual void Set(Type type, UInt32 value) { }

		public override string ToString()
		{
			return "0";
		}

		public abstract int Unmarshal(byte[] buffer, int offset);

		public abstract int Unmarshal(MarshaledBuffer marshaledBuffer);

		//Address(const Address& src);
		//virtual ~Address(void);
		//virtual int MarshalSize() = 0;
		//virtual uint32 NetworkDependentAddress();
		//virtual byte ProtocolId() = 0;
		//virtual int Size() = 0;
		//virtual bool operator==(const Address& rhs) = 0;

		public abstract Address WithRespectTo(Address rhs);

	}

	public class AddressListItem
	{
		public Address address;
		public bool enabled;
	}
	public class AddressList
	{
		List<AddressListItem> list = new List<AddressListItem>();

		public AddressListItem this[int index]
		{
			get { return list[index]; }
		}

		public void Add(Address address)
		{
			var item = address.CreateListItem();
			list.Add(item);
		}

		public void Clear()
		{
			list.Clear();
		}

		public int Count { get { return list.Count; } }

		public void Remove(Address address)
		{
			for (int i=0; i<list.Count; i++)
				if (list[i].address == address)
				{
					list.RemoveAt(i);
					return;
				}
		}
	}

}
