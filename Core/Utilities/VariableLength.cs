using System;

namespace Aspire.Core.Utilities
{
	public struct VariableLength
	{
		public VariableLength(int length)
		{
			this.length = length;
		}
		public int Marshal(byte[] buffer, int offset)
		{
			if ( length < 128 )
			{
				buffer[offset] = (byte)length;
				return 1;
			}
			int todo = length;
			int i=0;
			for(; i<offset+6; i++)
			{
				buffer[offset+i] = (byte)((length>>(i*7))&0x7f);
				if ( (todo >>= 7) == 0 ) break;
				buffer[offset+i] |= 0x80;
			}
			return i+1;
		}
		public int Unmarshal(byte[] buffer, int offset)
		{
			if ( buffer[offset] < 128 )
			{
				length = buffer[offset];
				return 1;
			}
			int i=0;
			length = 0;
			
			do
				length = (buffer[i+offset]<<(i*7)) | (length&((1<<(i*7))-1));
			while ( (buffer[offset+i++]&0x80) != 0 );
			return i;
		}
		public int Length
		{
			get { return length; }
			set { length = value; }
		}
		public static implicit operator int(VariableLength vlen)
		{
			return vlen.length;
		}
		private	int length;
	}

	public interface ICountedBlock
	{
		byte[] Bytes { get; }
		int Length { get; }
		int Offset { get; }
	}
}
