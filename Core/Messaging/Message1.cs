using System;

namespace Aspire.Core.Messaging
{
	public class Message1 : Message
	{
		const int marshalSize = 17;
		const int checksumSize = 2;

		int mDestinationOffset;
		ushort mFlags;
		byte mOpCode, mProtocolId = (byte)ProtocolId.Monarch;

		public Message1():base(1,0)
		{
			mSource = new Address1();
			mDestinationOffset = 4;
			mDestination = new Address1();
		}

		private ushort CalculateChecksum(byte[] buffer, int offset, int length)
		{
			uint sum = 0;
			int len = length&(~1); // truncate to even

			for (int i=0; i<len; i += 2)
				//Sum 16-bit words
				sum += BitConverter.ToUInt16(buffer, offset+i);

			// Add left-over byte, if any
			if(length > len)
				sum += buffer[offset+length-1];

			// Fold 32-bit sum to 16 bits
			while((sum >> 16) != 0)
				sum = (sum & 0xFFFF) + (sum >> 16);

			// Ones complement
			return (ushort)~sum;
		}

		public override Message Clone()
		{
			var msg = new Message1();
			msg.Assign(this);
			return msg;
		}

		public override bool HaveSecondaryHeaders { get { return mHeaderSize > 4; } }

		public override int Marshal(byte[] buffer, int offset, int length, int payloadLength, byte[] payload)
		{
			int start = offset;
			buffer[offset+0] = mProtocolId;
			buffer[offset+1] = mVersion;
			buffer[offset+2] = mPriority;
			mLength = (ushort)payloadLength;
			PutNetwork.USHORT(buffer, offset+3, mLength);
			offset += 5;
			offset += mDestination.Marshal(buffer, offset);
			offset += mSource.Marshal(buffer, offset);
			PutNetwork.USHORT(buffer, offset, mFlags);
			buffer[offset+2] = mOpCode;
			buffer[offset+3] = mHeaderSize;
			if (payload != null)
				System.Buffer.BlockCopy(payload,0,buffer,offset+4,payloadLength);
			PutNetwork.USHORT(buffer, offset+length, CalculateChecksum(buffer,offset+1,length-1));
			return length+2;
		}

		public override void MarshalDestination(Address destination, byte[] buffer, int offset)
		{
			destination.Marshal(buffer, offset+mDestinationOffset);
		}

		public override int Selector
		{
			get { return mOpCode; }
			set { mOpCode = (byte)value; }
		}

		public override int Sequence
		{
			get { return 0; }
			set {  }
		}

		public override int ShiftWidth
		{
			get { return 8; }
			set {  }
		}

		public override int Size { get { return marshalSize; } }

		public override int Unmarshal(byte[] buffer, int offset, int length)
		{
			int start = offset;
			//mProtocolId = buffer[offset+0];
			mVersion = buffer[offset+1];
			mPriority = buffer[offset+2];
			mLength = GetNetwork.USHORT(buffer, offset+3);
			offset += 5;
			offset += mDestination.Unmarshal(buffer, offset);
			offset += mSource.Unmarshal(buffer, offset);
			mFlags = GetNetwork.USHORT(buffer, offset);
			mOpCode = buffer[offset+2];
			mHeaderSize = buffer[offset+3];
			return offset+4-start;
		}

	}
}
