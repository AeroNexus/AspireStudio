
using Aspire.Core.Messaging;

namespace Aspire.Core
{
	public class LocalMessage
	{
		public enum OpCodeType { NoOp=0, Hello=32, Ack, AssignAddress, RouteRequest, Route };

		public static int HeaderSize = 6;

		Address mLocalToAddress;
		Transport mTransport;
		OpCodeType mOpCode;
		ushort mLength, mSourcePort;
		byte mVersion = 2;

		public void Initialize(Address ownAddress, Transport transport)
		{
			long in_addr;
			mSourcePort = (ushort)ownAddress.GetAddressPort(out in_addr);
			mTransport = transport;
			mLocalToAddress = ownAddress.Clone();
		}

		public OpCodeType OpCode { get { return mOpCode; } }

		public int SendTo(uint destPort, OpCodeType opCode, byte[] datagram, int length)
		{
			datagram[0] = mVersion;
			datagram[1] = (byte)opCode;
			PutNetwork.USHORT(datagram, 2, mSourcePort);
			PutNetwork.USHORT(datagram, 2, (ushort)length);
			mLocalToAddress.Set(Address.Type.Local, destPort);
			return mTransport.Write(datagram, 0, length + HeaderSize, mLocalToAddress);
		}

		public ushort SourcePort { get { return mSourcePort; } }

		public int Unmarshal(byte[] buffer,int length)
		{
			//mVersion = buffer[0];
			mOpCode = (OpCodeType)buffer[1];
			mSourcePort = GetNetwork.USHORT(buffer, 2);
			mLength = GetNetwork.USHORT(buffer, 4);
			return HeaderSize;
		}
	}

}

