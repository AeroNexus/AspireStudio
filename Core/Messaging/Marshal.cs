using System;

namespace Aspire.Core.Messaging
{
	// Get values from network byte order into little endian for PC
	public class GetNetwork
	{
		public static byte CHAR(byte[] buf, int index) { return buf[index]; }
		public static byte UCHAR(byte[] buf, int index) { return buf[index]; }
		public static short SHORT(byte[] buf, int index) { return (short)(buf[index+1] + (buf[index]<<8)); }
		public static ushort USHORT(byte[] buf, int index) { return (ushort)(buf[index+1] + (buf[index]<<8)); }
		public static int INT(byte[] buf, int index, byte[] swapBuf)
		{
			swapBuf[0] = buf[index+3];
			swapBuf[1] = buf[index+2];
			swapBuf[2] = buf[index+1];
			swapBuf[3] = buf[index+0];
			return BitConverter.ToInt32(swapBuf, 0);
		}
		public static uint UINT(byte[] buf, int index, byte[] swapBuf)
		{
			swapBuf[0] = buf[index+3];
			swapBuf[1] = buf[index+2];
			swapBuf[2] = buf[index+1];
			swapBuf[3] = buf[index+0];
			return BitConverter.ToUInt32(swapBuf, 0);
		}
		public static float FLOAT(byte[] buf, int index, byte[] swapBuf)
		{
			swapBuf[0] = buf[index+3];
			swapBuf[1] = buf[index+2];
			swapBuf[2] = buf[index+1];
			swapBuf[3] = buf[index+0];
			return BitConverter.ToSingle(swapBuf, 0);
		}
		public static double DOUBLE(byte[] buf, int index, byte[] swapBuf)
		{
			swapBuf[0] = buf[index+7];
			swapBuf[1] = buf[index+6];
			swapBuf[2] = buf[index+5];
			swapBuf[3] = buf[index+4];
			swapBuf[4] = buf[index+3];
			swapBuf[5] = buf[index+2];
			swapBuf[6] = buf[index+1];
			swapBuf[7] = buf[index+0];
			return BitConverter.ToDouble(swapBuf, 0);
		}
		public static Int64 INT64(byte[] buf, int index, byte[] swapBuf)
		{
			swapBuf[0] = buf[index+7];
			swapBuf[1] = buf[index+6];
			swapBuf[2] = buf[index+5];
			swapBuf[3] = buf[index+4];
			swapBuf[4] = buf[index+3];
			swapBuf[5] = buf[index+2];
			swapBuf[6] = buf[index+1];
			swapBuf[7] = buf[index+0];
			return BitConverter.ToInt64(swapBuf, 0);
		}
		public static UInt64 UINT64(byte[] buf, int index, byte[] swapBuf)
		{
			swapBuf[0] = buf[index+7];
			swapBuf[1] = buf[index+6];
			swapBuf[2] = buf[index+5];
			swapBuf[3] = buf[index+4];
			swapBuf[4] = buf[index+3];
			swapBuf[5] = buf[index+2];
			swapBuf[6] = buf[index+1];
			swapBuf[7] = buf[index+0];
			return BitConverter.ToUInt64(swapBuf, 0);
		}
	}

	// Put values in little endian out as network byte order
	public class PutNetwork
	{
		//// Marshalling macros - generic
		static void PUT_TYPE2(byte[] buf, int index, ushort val)
		{
			buf[index+1] = (byte)(val&0xff); buf[index] = (byte)((val>>8)&0xff);
		}
		static void PUT_TYPE4(byte[] buf, int index, uint val)
		{
			buf[index+3] = (byte)(val&0xff); buf[index+2] = (byte)((val>>8)&0xff);
			buf[index+1] = (byte)((val>>16)&0xff); buf[index+0] = (byte)((val>>24)&0xff);
		}
		public static void CHAR(byte[] buf, int index, byte val) { buf[index] = val; }
		public static void UCHAR(byte[] buf, int index, byte val) { buf[index] = val; }
		public static void SHORT(byte[] buf, int index, short val) { PUT_TYPE2(buf, index, (ushort)val); }
		public static void USHORT(byte[] buf, int index, ushort val) { PUT_TYPE2(buf, index, val); }
		public static void INT(byte[] buf, int index, int val) { PUT_TYPE4(buf, index, (uint)val); }
		public static void UINT(byte[] buf, int index, uint val) { PUT_TYPE4(buf, index, val); }
		public static void LONG(byte[] buf, int index, int val) { PUT_TYPE4(buf, index, (uint)val); }
		public static void ULONG(byte[] buf, int index, uint val) { PUT_TYPE4(buf, index, val); }
		public static void FLOAT(byte[] buf, int index, float val, byte[] swapBuf)
		{
			BitConverter.GetBytes(val).CopyTo(swapBuf, 0);
			buf[index+0] = swapBuf[3];
			buf[index+1] = swapBuf[2];
			buf[index+2] = swapBuf[1];
			buf[index+3] = swapBuf[0];
		}
		public static void DOUBLE(byte[] buf, int index, double val, byte[] swapBuf)
		{
			BitConverter.GetBytes(val).CopyTo(swapBuf, 0);
			buf[index+0] = swapBuf[7];
			buf[index+1] = swapBuf[6];
			buf[index+2] = swapBuf[5];
			buf[index+3] = swapBuf[4];
			buf[index+4] = swapBuf[3];
			buf[index+5] = swapBuf[2];
			buf[index+6] = swapBuf[1];
			buf[index+7] = swapBuf[0];
		}
		public static void INT64(byte[] buf, int index, Int64 val, byte[] swapBuf)
		{
			BitConverter.GetBytes(val).CopyTo(swapBuf, 0);
			buf[index+0] = swapBuf[7];
			buf[index+1] = swapBuf[6];
			buf[index+2] = swapBuf[5];
			buf[index+3] = swapBuf[4];
			buf[index+4] = swapBuf[3];
			buf[index+5] = swapBuf[2];
			buf[index+6] = swapBuf[1];
			buf[index+7] = swapBuf[0];
		}
		public static void UINT64(byte[] buf, int index, UInt64 val, byte[] swapBuf)
		{
			BitConverter.GetBytes(val).CopyTo(swapBuf, 0);
			buf[index+0] = swapBuf[7];
			buf[index+1] = swapBuf[6];
			buf[index+2] = swapBuf[5];
			buf[index+3] = swapBuf[4];
			buf[index+4] = swapBuf[3];
			buf[index+5] = swapBuf[2];
			buf[index+6] = swapBuf[1];
			buf[index+7] = swapBuf[0];
		}
	}

	// Get values from native byte order
	public class GetNative
	{
		public static byte CHAR(byte[] buf, int index) { return buf[index]; }
		public static byte UCHAR(byte[] buf, int index) { return buf[index]; }
		public static short SHORT(byte[] buf, int index) { return (short)(buf[index] + (buf[index+1]<<8)); }
		public static ushort USHORT(byte[] buf, int index) { return (ushort)(buf[index] + (buf[index+1]<<8)); }
		public static int INT(byte[] buf, int index)
		{
			return BitConverter.ToInt32(buf, index);
		}
		public static uint UINT(byte[] buf, int index)
		{
			return BitConverter.ToUInt32(buf, index);
		}
		public static float FLOAT(byte[] buf, int index)
		{
			return BitConverter.ToSingle(buf, index);
		}
		public static double DOUBLE(byte[] buf, int index)
		{
			return BitConverter.ToDouble(buf, index);
		}
		public static UInt64 UINT64(byte[] buf, int index)
		{
			return BitConverter.ToUInt64(buf, index);
		}
	}

	// Put values out in native byte order
	public class PutNative
	{
		//// Marshalling macros - generic
		static void PUT_TYPE2(byte[] buf, int index, ushort val)
		{
			buf[index] = (byte)(val&0xff); buf[index+1] = (byte)((val>>8)&0xff);
		}
		static void PUT_TYPE4(byte[] buf, int index, uint val)
		{
			buf[index+0] = (byte)(val&0xff); buf[index+1] = (byte)((val>>8)&0xff);
			buf[index+2] = (byte)((val>>16)&0xff); buf[index+3] = (byte)((val>>24)&0xff);
		}
		public static void CHAR(byte[] buf, int index, byte val) { buf[index] = val; }
		public static void UCHAR(byte[] buf, int index, byte val) { buf[index] = val; }
		public static void SHORT(byte[] buf, int index, short val) { PUT_TYPE2(buf, index, (ushort)val); }
		public static void USHORT(byte[] buf, int index, ushort val) { PUT_TYPE2(buf, index, val); }
		public static void INT(byte[] buf, int index, int val) { PUT_TYPE4(buf, index, (uint)val); }
		public static void UINT(byte[] buf, int index, uint val) { PUT_TYPE4(buf, index, val); }
		public static void LONG(byte[] buf, int index, int val) { PUT_TYPE4(buf, index, (uint)val); }
		public static void ULONG(byte[] buf, int index, uint val) { PUT_TYPE4(buf, index, val); }
		public static void FLOAT(byte[] buf, int index, float val)
		{
			BitConverter.GetBytes(val).CopyTo(buf, index);
		}
		public static void DOUBLE(byte[] buf, int index, double val)
		{
			BitConverter.GetBytes(val).CopyTo(buf, index);
		}
		public static void UINT64(byte[] buf, int index, UInt64 val)
		{
			BitConverter.GetBytes(val).CopyTo(buf, index);
		}
	}
}
