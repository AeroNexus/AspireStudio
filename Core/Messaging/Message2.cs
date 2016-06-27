//using System;

namespace Aspire.Core.Messaging
{
	public class Message2 : Message
	{
		public const int MarshalSize = 24;
		public const int iSequence = 5;

		protected MessageId mMessageId = new MessageId();
		protected byte mSequence;
		int mDestinationOffset;

		// shift width is an optimization that compresses MSG_SELECTORs in case statements.
		// See NativeProtocol. It should always default to 8, but can vbe overridden for specific protocols
		//if they know the maximum bits occupied my the maximum messageId byte
		protected byte mShiftWidth;

		public Message2():base(2,6)
		{
			mSequence = 0;
			mShiftWidth = 8;
			mSource = new Address2();
			mDestinationOffset = 8 + mSource.Length;
			mDestination = new Address2();
		}

		public override Message Clone()
		{
			var msg = new Message2();
			msg.Assign(this);
			return msg;
		}

		public override bool HaveSecondaryHeaders { get { return mHeaderSize > 6; } }

		public override int Marshal(byte[] buffer, int offset, int length, int payloadLength, byte[] payload)
		{
			int start = offset;
			buffer[offset+0] = mVersion;
			buffer[offset+1] = mHeaderSize;
			mLength = (ushort)payloadLength;
			PutNetwork.USHORT(buffer, offset+2, mLength);
			buffer[offset+4] = mPriority;
			buffer[offset+5] = mSequence;
			offset += 6;
			offset += mMessageId.Marshal(buffer, offset);
			offset += mSource.Marshal(buffer, offset);
			offset += mDestination.Marshal(buffer, offset);
			if (payload != null)
				System.Buffer.BlockCopy(payload,0,buffer,offset,payloadLength);
			return offset-start+payloadLength;
		}

		public override void MarshalDestination(Address destination, byte[] buffer, int offset)
		{
			destination.Marshal(buffer, offset+mDestinationOffset);
		}

		public override int NextSequence() { return ++mSequence; }

		public override int Selector
		{
			get { return mMessageId.Hash(mShiftWidth); }
			set { mMessageId.SetHash(value,mShiftWidth); }
		}

		public override int Sequence
		{
			get { return mSequence; }
			set { mSequence = (byte)value; }
		}

		public override int ShiftWidth
		{
			get { return mShiftWidth; }
			set { mShiftWidth = (byte)value; }
		}

		public override int Size { get { return MarshalSize; } }

		public override int Unmarshal(byte[] buffer, int offset, int length)
		{
			mBuffer = buffer;
			mOffset = offset;
			int start = offset;
			mVersion = buffer[offset+0];
			mHeaderSize = buffer[offset+1];
			mLength = GetNetwork.USHORT(buffer, offset+2);
			mPriority = buffer[offset+4];
			mSequence = buffer[offset+5];
			offset += 6;
			offset += mMessageId.Unmarshal(buffer, offset);
			offset += mSource.Unmarshal(buffer, offset);
			offset += mDestination.Unmarshal(buffer, offset);
			return offset-start;
		}

	}
}
