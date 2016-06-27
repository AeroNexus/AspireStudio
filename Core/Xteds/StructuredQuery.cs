using System;
using System.Collections.Generic;
using System.Text;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;

namespace Aspire.Core.xTEDS
{
	public partial class StructuredQuery
	{
		Address destination, requestorId = new Address2();
		List<Byte> byteList = new List<Byte>();
		uint sensorId;
		byte[] match, deliver;
		int sequence = 1;
		bool futures;

		public StructuredQuery()
		{
		}

		public StructuredQuery(StructuredQuery src)
		{
			destination = src.destination;
			futures = src.futures;
			sensorId = src.sensorId;
			sequence = src.sequence;
			match = src.match.Clone() as byte[];
			deliver = src.deliver.Clone() as byte[];
		}

		//~StructuredQuery()
		//{
		//}

		public bool Deliver( params object[] ops )
		{
			byteList.Clear() ;
			var asciiEncoding = new ASCIIEncoding();
			foreach ( var os in ops )
			{
				if (os.GetType() == typeof(sq))
				{
					if (((sq)os) == sq.End)
						break;
					byteList.Add((byte)((sq)os));
				}
				else if (os.GetType() == typeof(string))
				{
					Byte[] bytes = asciiEncoding.GetBytes(os as string);
					foreach (var _byte in bytes)
						byteList.Add(_byte);
					byteList.Add((byte)0);
				}
				else
					break;
			}
			byteList.Add((byte)sq.End);
			deliver = byteList.ToArray();
			return true;
		}

		public bool Futures
		{
			get { return futures; }
			set { futures = value; }
		}

		public Address RequestorId
		{
			get { return requestorId; }
			//set { value.CopyTo(requestorId); }
		}

		int Marshal(byte[] buf, int size)
		{
			int len = 0;
			PutNetwork.UINT(buf,len,sensorId);
			len += 4; // sizeof sensorId
			PutNetwork.UCHAR(buf,len,(byte)(futures?1:0));
			len ++;
			Buffer.BlockCopy(match, 0, buf, len, match.Length);
			len += match.Length;
			buf[len-1] = (byte)sq.Deliver;
			Buffer.BlockCopy(deliver, 0, buf, len, deliver.Length);
			return len+deliver.Length;
		}

		public bool Match( uint senId, params object[] ops )
		{
			byteList.Clear();

			foreach (var os in ops)
			{
				if (os.GetType() == typeof(sq))
				{
					if (((sq)os) == sq.End)
						break;
					byteList.Add((byte)((sq)os));
				}
				else if (os.GetType() == typeof(string))
				{
					string str = os as string;
					byte[] bytes = new byte[str.Length];
					Encoding.ASCII.GetEncoder().GetBytes(str.ToCharArray(), 0, str.Length, bytes, 0, true);
					foreach (var _byte in bytes)
						byteList.Add(_byte);
					byteList.Add((byte)0);
				}
				else
					break;
			}
			byteList.Add((byte)sq.End);
			match = byteList.ToArray();
			sensorId = senId;
			return true;
		}

		public bool Search(short tag)
		{
			//if ( tag != 0 )
			//    request.seq_num = tag;
			//else
			//{
			//    request.seq_num = (short)sequence++;
			//    if ( sequence == 32768 ) sequence = 1;
			//}
			//request.command_id.Interface = 1;
			//request.command_id.Message = 20;
			//request.source.SensorId = 1;
			//requestorId.CopyTo(request.destination);
			//request.length = (short)Marshal(request.data,BUFSIZE);
			//request.Send();
			return true;
		}
	}
}
