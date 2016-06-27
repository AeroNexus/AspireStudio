using System;

namespace Aspire.Core
{
	public class Aspire
	{
		public const int sizeofCHAR = 1;
		public const int sizeofSHORT = 2;
		public const int sizeofLONG = 4;
		public const int MaxDatagram = 65536;
	}
	public enum PrimitiveType
	{
		unknownType=0,
		UINT8=17,
		INT8=18,
		UINT16,
		INT16,
		UINT32,
		INT32,
		FLOAT32,
		FLOAT64,
		UINT64,
		INT64,
		STRING,
		BUFFER,
		// not currently serialized
		ENUM32,
		FLOAT128,
		OBJECT,
		CSTRING
	};

}
