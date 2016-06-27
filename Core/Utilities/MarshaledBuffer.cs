using System;

namespace Aspire.Core.Utilities
{
	public class MarshaledBuffer : ICountedBlock
	{
		public MarshaledBuffer()
		{
			mLength.Length = 0;
			mBytes = null;
			mOffset = 0;
		}
		public MarshaledBuffer(int len, byte[] buffer, int offset)
		{
			mLength.Length = len;
			mBytes = buffer;
			mOffset = offset;
		}

		public int Allocate
		{
			get { return mLength; }
			set
			{
				if (value > mLength)
				{
					mBytes = new byte[value];
					mOffset = 0;
				}
				mLength.Length = value;
			}
		}

		public int Marshal(byte[] buffer, int offset)
		{
			int newOffset = offset + mLength.Marshal(buffer,offset);
			int len = mLength.Length;
			if ( len > 0 )
				Buffer.BlockCopy(mBytes,mOffset,buffer,newOffset,len);
			return newOffset-offset+len;
		}

		public void Set(int len, byte[] buffer, int offset)
		{
			mLength.Length = len;
			mBytes = buffer;
			mOffset = offset;
		}

		public override string ToString()
		{
			if (mBytes != null)
				return BitConverter.ToString(mBytes, mOffset, mLength.Length);
			else
				return string.Empty;
		}

		public int Unmarshal(byte[] buffer, int offset)
		{
			mOffset = offset + mLength.Unmarshal(buffer, offset);
			mBytes = buffer;
			return mOffset-offset+mLength.Length;
		}
		public byte[] Bytes
		{
			get { return mBytes; }
			set { mBytes = value; }
		}

		public int Length { get { return mLength.Length; } }
		public VariableLength VarLength { get { return mLength; } }
		public int Offset { get { return mOffset; } }
		VariableLength mLength;
		byte[] mBytes;
		int mOffset;
	}
}
