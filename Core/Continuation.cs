using System;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;

namespace Aspire.Core
{
	public class Continuation : SecondaryHeader
	{
		const int MarshaledSize = 12;	// includes size of base SecondaryHeader(2)
		const int MaxHeader = 1020;		// Maximum header size (255*4)

		enum Role { Transmitter, Receiver };

		Continuation mAck;
		Role mRole;
		byte[] mPayloadBuffer, mAckBuf;
		int mFixedHeaderSize = 24;
		uint mCursor, mOffset, mMaxPacketSize, mPayloadLength, mSize, mTotalLength;
		ushort mNumPackets, mPacketNo, mTransactionId;

		public event EventHandler HasFinished; 
		
		public Continuation() : this(0) { }

		public Continuation(uint payloadLength) :
			base((byte)SecondaryHeader.SecondaryHdrType.Continuation,MarshaledSize-2)
		{
			mMaxPacketSize = (uint)(64*1024 - mFixedHeaderSize - 32); // Initially. Refine in Initialize.
			if (payloadLength == 0)
			{
				mPayloadLength = 0;
				mTotalLength = 0;
				mPayloadBuffer = null;
				mRole = Role.Receiver;
				mAckBuf = new byte[64];
				return;
			}
			else
			{
				mRole = Role.Transmitter;
				mAck = new Continuation();
			}
			Resize(payloadLength);
		}

		~Continuation()
		{
			//if ( mPayloadBuffer != null ) mPayloadBuffer = null;
			//if (mRole)
			//	mAck = null;
			//else
			//	mAckBuf = null;
		}

		/**
		 Adds a packet to the receive buffer.

		 @param packet The packet to add.
		 @param length	The length of the packet to add.
		 @param [in,out] ackBuf If non-null, buffer for acknowledge data.

		 @return the length of the acknowlege payload.
		 */
		public int AddPacket(byte[] packet, int offset, int length, byte[] ackBuf)
		{
			Buffer.BlockCopy(packet,offset,mPayloadBuffer,(int)mOffset,length);
			if (mPacketNo == 1) // use negative to signal first packet
				return -Marshal(ackBuf,0);
			int n = SecondaryHeader.MarshaledLength;	
			PutNetwork.USHORT(ackBuf,n+6 ,mPacketNo);
			return MarshaledSize;
		}

		public uint BytesTransferred { get { return mOffset; } }

		public bool Finished { get { return mPacketNo >= mNumPackets; } }

		public static Continuation FromMessage(XtedsMessage xMsg)
		{
			var sh = xMsg.GetSecondaryHeader((int)SecondaryHeader.SecondaryHdrType.Continuation);
			if ( sh == null )
			{
				sh = new Continuation();
				xMsg.AddSecondaryHeader(sh);
			}
			return sh as Continuation;
		}

		/**
		 Initializes a Continuation secondary header with the length of the payload.
		 Setup for the first transfer.

		 @param fixedHeaderSize Size of the fixed message header.

		 @return The start of information in the write buffer. Room is reserved
		 for the fixed message header.
		 */
		public uint Initialize(int fixedHeaderSize, Address destination)
		{
			//if (destination != mDestination)
			//{
			//	//MsgConsole.WriteLine("Multiple sends of large payloads unsupported at this moment");
			//}

			mFixedHeaderSize = fixedHeaderSize;

			// Initialize total length and num packets assuming that this secondary header
			// is the only SH. Refine below in SendLength
			mTotalLength = mPayloadLength + (uint)MarshaledSize + (uint)mFixedHeaderSize;
			uint packetSize = mMaxPacketSize - MarshaledSize;
			mNumPackets = (ushort)((mPayloadLength+packetSize-1)/packetSize);

			mTransactionId++;
			mOffset = mPayloadLength; // Overload mOffset on the first SH
			mPacketNo = 1;
			mCursor = (uint)mFixedHeaderSize;
			return mCursor;
		}

		public bool IsTransmitter { get { return mRole == Role.Transmitter; } }

		/**
		 Marshals the Continuation secondary header to the network buffer.

		 @param buffer The network buffer that will contain marshaled data.

		 @return the number of bytes marshaled.
		 */
		public override int Marshal(byte[] buffer,int offset)
		{
			int n = base.Marshal(buffer,offset);
			PutNetwork.USHORT(buffer,n,mTransactionId);
			PutNetwork.UINT(buffer,n+2,mOffset);
			PutNetwork.USHORT(buffer,n+6,mPacketNo);
			PutNetwork.USHORT(buffer,n+8,mNumPackets);
			return MarshaledSize;
		}

		/**
		 Gets the number of bytes that are marshaled to/from a network buffer.

		 @return The number of marshaled bytes.
		 */
		public override int MarshalSize()
		{
			return MarshaledSize;
		}

		public int NeedToTransmit(SecondaryHeaderIterator iterator, out int startNextPacket)
		{
			startNextPacket = 0;
			if ( iterator.GetSecondaryHeader(mAck) != null )
			{
				if ( mAck.mNumPackets == mNumPackets &&
					mAck.mPacketNo == mPacketNo &&
					mAck.mTransactionId == mTransactionId )
				{
					if (mAck.mPacketNo == mNumPackets)
					{
						if (HasFinished != null)
							HasFinished(this, EventArgs.Empty);
						return 0;
					}

					mPacketNo++;
					mCursor += mMaxPacketSize - MarshaledSize;
					mOffset += mMaxPacketSize - MarshaledSize;
					startNextPacket = (int)(mCursor-MarshaledSize);
					uint remaining = mTotalLength - mCursor;
					int length;
					if ( remaining > mMaxPacketSize )
						length = (int)mMaxPacketSize;
					else
						length = (int)remaining + MarshaledSize;

					Marshal(mPayloadBuffer,startNextPacket);
					//MsgConsole.WriteLine("Sending {0}/{1}",mPacketNo,mNumPackets);
					return length;
				}
				return 0;
			}
			return 0;
		}

		public byte[] PayloadBuffer { get { return mPayloadBuffer; } }

		public uint PayloadLength { get { return mPayloadLength; } }

		/**
		 Resizes the read buffer once the writer's Continuation secondary header
		 is received and marshaled.

		 @param payloadLength Length of the payload.
		 */
		public void Resize(uint payloadLength)
		{
			if ( mSize > 0 ) mPayloadBuffer = null;
			mPayloadLength = payloadLength;
			mTotalLength = mPayloadLength + (uint)MarshaledSize + (uint)mFixedHeaderSize;
			mSize = payloadLength + MaxHeader;	// Allow for header + SHs.
			mPayloadBuffer = new byte[mSize];
		}

		/**
		 Calculates the number of bytes to send, after the total
		 secondary header length is known.

		 @param secHdrLength Length of all the secondary headers.

		 @return the number of bytes to send in the first packet.
		 */
		public uint SendLength(int secHdrLength)
		{
			if ( mPacketNo == 1 )
			{
				// Refine total length and num packets now that we know the size
				// of all the secondary headers
				mTotalLength = mPayloadLength + (uint)secHdrLength + (uint)mFixedHeaderSize;
				uint m = mPayloadLength + (uint)secHdrLength - (uint)mMaxPacketSize;
				uint n = mMaxPacketSize - MarshaledSize;
				mNumPackets = (ushort)((m+n-1)/n + 1);
				mOffset = 0;
				mCursor += (uint)secHdrLength;
				return mMaxPacketSize;
			}
			else
				MsgConsole.WriteLine("SendLength error");
			return 0;
		}

		public void SetPayloadLength(uint payloadLength)
		{
			mPayloadLength = payloadLength;
			mTotalLength = mPayloadLength + (uint)MarshaledSize + (uint)mFixedHeaderSize;
			uint packetSize = mMaxPacketSize - MarshaledSize;
			mNumPackets = (ushort)((mPayloadLength+packetSize-1)/packetSize);
		}

		public uint Size { get { return mSize; } }

		/**
		 Unmarshals a network buffer into a Continuation secondary header.

		 @param buffer The buffer to unmarshal from.

		 @return the number of bytes unmarshaled.
		 */
		public override int Unmarshal(byte[] buffer, int offset)
		{
			int n = base.Unmarshal(buffer,offset);
			mTransactionId = GetNetwork.USHORT(buffer,n);
			mOffset = GetNetwork.UINT(buffer,n+2,swap);
			mPacketNo = GetNetwork.USHORT(buffer,n+6);
			mNumPackets = GetNetwork.USHORT(buffer,n+8);
			if ( mPacketNo == 1 )
			{
				mPayloadLength = mOffset;
				mOffset = 0;
				if ( mSize == 0 )
					Resize(mPayloadLength);
			}
			return MarshaledSize;
		}	byte[] swap = new byte[4];

	}
}
