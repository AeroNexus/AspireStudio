using System;
using System.Text;

namespace Aspire.Core.Utilities
{
	public class MarshaledString : ICountedBlock
	{
		public MarshaledString()
		{
			mLength.Length = 1;
			mChars = null;
			mOffset = 0;
		}
		public MarshaledString(int len, byte[] buffer, int offset)
		{
			mLength.Length = len;
			mChars = buffer;
			mOffset = offset;
		}
		public MarshaledString(string value)
		{
			mLength.Length = value.Length+1;
			mChars = Encoding.ASCII.GetBytes(value);
			mOffset = 0;
		}
		public static implicit operator string(MarshaledString marshaledString)
		{
			return marshaledString.String;
		}
		public static implicit operator MarshaledString(string sysString)
		{
			return new MarshaledString(sysString);
		}
		public int Marshal(byte[] buffer, int offset)
		{
			int newOffset = offset+mLength.Marshal(buffer,offset);
			int len = mLength.Length;
			if ( mChars != null )
				Buffer.BlockCopy(mChars, mOffset, buffer, newOffset, len-1);
			buffer[newOffset+len-1] = 0;
			return newOffset-offset+len;
		}
		public void Set(string value)
		{
			Set(value,value != null ? value.Length :0);
		}
		public void Set(string value, int len)
		{
			mLength.Length = len+1;
			mChars = value==null?null:Encoding.ASCII.GetBytes(value);
			mOffset = 0;
		}
		public void Set(int len, byte[] buffer, int offset)
		{
			mLength.Length = len;
			mChars = buffer;
			mOffset = offset;
		}
		public override string ToString()
		{
			return String;
		}
		public int Unmarshal(byte[] buffer, int offset)
		{
			mOffset = offset + mLength.Unmarshal(buffer, offset);
			mChars = buffer;
			return mOffset-offset+mLength.Length;
		}
		public byte[] Bytes { get { return mChars; } }
		public byte[] Chars { get { return mChars; } }
		public int Length { get { return mLength.Length; } }
		public int Offset { get { return mOffset; } }
		public string String
		{
			get
			{
				if ( mLength <= 1 )
					return string.Empty;
				return new string(Encoding.ASCII.GetChars(mChars, mOffset, mLength-1));
			}
			set { Set(value); }
		}
		public VariableLength VarLength { get { return mLength; } }
		VariableLength mLength;
		byte[] mChars;
		int mOffset;
	}
}
