using System;

namespace Aspire.Core.Messaging
{
	public abstract class Message
	{
		protected Address mSource;
		protected Address mDestination;
		protected Transport mTransport;

		protected byte[] mBuffer;
		protected int mOffset;
		protected UInt16 mLength;
		protected byte mVersion;
		protected byte mHeaderSize, mPrevHeaderSize;
		protected byte mPriority;

		public Message(byte version, byte headerSize)
		{
			mVersion = version;
			mHeaderSize = headerSize;
		}

		protected void Assign(Message src)
		{
			mSource = src.mSource.Clone();
			mDestination = src.mDestination.Clone();
			mTransport = src.mTransport;
		}
		public byte[] Buffer { get { return mBuffer; } }

		public abstract Message Clone();

		public Address Destination
		{
			get { return mDestination; }
			set { mDestination = value; }
		}
		public abstract bool HaveSecondaryHeaders{get;}

		/// <summary>
		/// Get the header length in bytes
		/// </summary>
		public int HeaderLength
		{
			get { return mHeaderSize<<2; }
			set { mHeaderSize = (byte)((Size+value+3)>>2); }
		}

		/// <summary>
		/// Get the header size in uint32s
		/// </summary>
		public byte HeaderSize { get { return mHeaderSize; } }
		public void PopHeaderSize(){mHeaderSize = mPrevHeaderSize;}
		public void PushHeaderSize(byte headerSize){mPrevHeaderSize = mHeaderSize; mHeaderSize = headerSize;}

		/// <summary>
		/// Get the payload length
		/// </summary>
		public UInt16 Length { get { return mLength; } }
		public void Lock() { }
		public abstract int Marshal(byte[] buffer, int offset, int length, int payloadLength, byte[] payload);
		public abstract void MarshalDestination(Address destination, byte[] buffer, int offset);
		public int Offset { get { return mOffset; } }
		public virtual int NextSequence() { return 0; }
		public int PayloadOffset() { return (mHeaderSize<<2)+mOffset; }
		public void Release() { }
		public abstract int Selector { get; set; }
		//public int Send(byte[] buffer, int payload, int length)
		//{
		//	return mTransport.Send(this,buffer,payload,length,null);
		//}
		//public int Send(byte[] buffer, int payload, int length, Transport transport, Address destination)
		//{
		//	return transport.Send(this,buffer,payload,length,destination);
		//}

		//public int SendMultiple(byte[] buffer, int payload, int length, AddressList destinations)
		//{
		//	return mTransport.SendMultiple(this, buffer, payload, length, destinations);
		//}

		public abstract int Sequence { get; set; }

		public abstract int ShiftWidth { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public abstract int Size { get; }

		public Address Source
		{
			get { return mSource; }
			set { mSource = value; }
		}
		public Transport Transport
		{
			get { return mTransport; }
			set { mTransport = value; }
		}

		public abstract int Unmarshal(byte[] buffer, int offset, int length);

		public byte Version
		{
			get { return mVersion; }
			set { mVersion = value; }
		}
	}
}
