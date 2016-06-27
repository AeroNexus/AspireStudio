//using System;

using Aspire.Core.Utilities;

namespace Aspire.Core.Messaging
{
	/// <summary>
	/// Summary description for MessageId.
	/// </summary>
	public class MessageId
	{
		public MessageId(byte mInterfaceId, byte mMessageId)
		{
			Message = mMessageId;
			Interface = mInterfaceId;
		}

		public MessageId()
			: this(0, 0)
		{
		}

		public MessageId(MessageId id)
			: this(id.mInterfaceId, id.mMessageId)
		{
		}

		public void CopyTo(MessageId dest)
		{
			dest.mInterfaceId = mInterfaceId;
			dest.mMessageId = mMessageId;
		}

		public static MessageId Empty;

		public int Hash()
		{
			return (mInterfaceId<<8)+mMessageId;
		}

		public int Hash(int shiftWidth)
		{
			return (mInterfaceId<<shiftWidth)+mMessageId;
		}

		public void Set(byte interfaceId, byte messageId)
		{
			mInterfaceId = interfaceId;
			mMessageId = messageId;
		}

		public void SetHash(int value, int shiftWidth)
		{
			mInterfaceId = (byte)((value>>shiftWidth)&0xff);
			mMessageId = (byte)(value&((1<<shiftWidth)-1));
		}

		public int Size { get { return Aspire.sizeofCHAR + Aspire.sizeofCHAR; } }

		public byte Interface
		{
			get { return mInterfaceId; }
			set { mInterfaceId = value; }
		}

		public byte Message
		{
			get { return mMessageId; }
			set { mMessageId = value; }
		}

		public short InterfaceMessagePair
		{
			get { return (short)(mMessageId + (mInterfaceId << 8)); }
		}

		public int Marshal(byte[] buffer, int start)
		{
			buffer[start] = mInterfaceId;
			buffer[start+1] =  mMessageId;
			return 2;
		}

		public int Unmarshal(byte[] buffer, int start)
		{
			mInterfaceId = buffer[start];
			mMessageId = buffer[start+1];
			return 2;
		}

		public override bool Equals(System.Object obj)
		{
			if (!(obj is MessageId))
			{
				return false;
			}
			return this == (MessageId)obj;
		}

		public override int GetHashCode()
		{
			return (int)((mInterfaceId<<8) | mMessageId);
		}

		public static bool operator==(MessageId lhs, MessageId rhs)
		{
			if ((object)lhs == null || (object)rhs == null)
			{
				return ((object)lhs == null && (object)rhs == null);
			}
			else
			{
				return ((lhs.mMessageId == rhs.mMessageId) &&
					(lhs.mInterfaceId == rhs.mInterfaceId));
			}
		}

		public static bool operator!=(MessageId lhs, MessageId rhs)
		{
			return !(lhs==rhs);
		}

		public override string ToString()
		{
			return string.Format("{0}.{1}", mInterfaceId, mMessageId);
		}

		private byte mInterfaceId;
		private byte mMessageId;
	}
}
