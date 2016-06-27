using System;
using System.Collections.Generic;
using System.Text;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;

namespace Aspire.Core.xTEDS
{
	/// <summary>
	/// Manage an array of responses, each with an arrray of string items
	/// </summary>
	public class StructuredResponse //: IMarshal
	{
		Address[] mComponents;
		byte[] mData;
		ICountedBlock[] mResponses;
		ushort[] mComponentFirstResp;
		int mComponentsSize, mResponsesSize, mStringDataSize;
		ushort mNumItems, mNumResponses;
		short mNumComponents;

		public StructuredResponse()
		{
			Clear();
		}

		//~StructuredResponse()
		//{
		//    if ( stringData ) free(stringData);
		//    if ( items ) free(items);
		//    if ( responses ) free(responses);
		//    if (sensors ) free(sensors);
		//    if ( sensorFirstResp ) free(sensorFirstResp);
		//}

		public string Error { get; private set; }

		public bool ItemIsMessageSpec(int response,int item)
		{
			int i = response*mNumItems+item;
			if ( i < mNumResponses*mNumItems )
				return (byte)mResponses[i].Bytes[0] >= (byte)StructuredQuery.sq.Elements;
			else
				return false;
		}

		public ushort NumItems { get { return mNumItems; } }
		public ushort NumResponses { get { return mNumResponses; } }
		public short NumSensors { get { return mNumComponents; } }

		public ICountedBlock this[int response, int item ]
		{
			get
			{
				int i = response*mNumItems+item;
				if ( i < mNumResponses*mNumItems )
					return mResponses[i];
				else
					return null;
			}
		}

		void Clear()
		{
			mNumComponents = 0;
			mNumResponses = 0;
			mNumItems = 0;
			mData = null;
			mResponses = null;
		}

		public Address ComponentAddress( int responseId )
		{
			for (int sen=mNumComponents-1; sen>=0; sen--)
				if (mComponentFirstResp[sen] <= responseId)
					return mComponents[sen];
			return null;
		}

		static string[] PrimitiveTypeName = new string[]{
			"unknown","","","","","","","","","","","","","","","","",
			"UINT8","INT8","UINT16","INT16","UINT32","INT32","FLOAT32","FLOAT64",
			"UINT64","INT64","STRING","BUFFER","ENUM32","BOOL32","FLOAT128","OBJECT","CSTRING"};

		public static void PrintOpStr(MarshaledBuffer opCodes)
		{
			PrintOpStr(opCodes.Bytes, opCodes.Offset, opCodes.Length);
		}

		public static void PrintOpStr(byte[] bytes, int offset, int length)
		{
			byte op = 0;
			MsgConsole.WriteLine("(");
			for(int i=offset; i<length; i++)
			{
				byte c = bytes[i];
				if (c >= (byte)StructuredQuery.sq.Elements || (op==0 && c <= (byte)StructuredQuery.sq.ccFirst))
				{
					uint j;
					for(j=0;j<StructuredQuery.Tokens.Length;j++)
						if (c == (byte)StructuredQuery.Tokens[j].op)
						{
							MsgConsole.WriteLine("{0},", StructuredQuery.Tokens[j].name);
							op = c;
							break;
						}
					if (j==StructuredQuery.Tokens.Length)
						MsgConsole.WriteLine("<unkown op {0}>,",c);
				}
				else if (op!= 0)
				{
					if (c >= (byte)StructuredQuery.sq.ccFirst)
					{
						int j;
						for(j=i; bytes[j]!=0;j++)
							;
						int len = j-i;
						var text = ASCIIEncoding.ASCII.GetChars(bytes,i,len);
						MsgConsole.WriteLine("{0},",new string(text));
						 i += len;
					}
					else
					{
						if (op == (byte)StructuredQuery.sq.Format)
							MsgConsole.WriteLine("{0},",PrimitiveTypeName[c]);
						else
							MsgConsole.WriteLine("{0},",c);
					}
					op = 0;
				}
			}
			MsgConsole.WriteLine("),");
		}

		public void PrintResponses(int respId=-1)
		{
			Address prevAddress=null;
			var sb = new StringBuilder();
			if ( mNumResponses == 0 )
			{
				MsgConsole.WriteLine("No responses for SID {0}", ComponentAddress(0).ToString());
				return;
			}
			for( int resp=0; resp<mNumResponses; resp++)
			{
			  if (respId >= 0 && resp != respId) continue;
				sb.Length = 0;
				sb.Append(string.Format("SS resp {0}: ", resp));
				Address address = ComponentAddress(resp);
				if (address != prevAddress)
				{
					sb.Append(string.Format("SID {0}: ", address.ToString()));
					prevAddress = address;
				}
				for( int item=0; item < mNumItems; item++ )
				{
					sb.Append(mResponses[resp*mNumItems+item]);
					sb.Append(", ");
				}
				MsgConsole.WriteLine(sb.ToString());
			}
		}

		public int Unmarshal(byte[] buffer, int offset, int length)
		{
			int start = offset;
			MarshaledBuffer buf = new MarshaledBuffer();
			mNumResponses = GetNetwork.USHORT(buffer, offset+0);
			mNumItems = GetNetwork.USHORT(buffer, offset+2);
			mNumComponents = GetNetwork.SHORT(buffer, offset+4);
			offset += 6;
			if ( mNumComponents > 0 )
			{
				if ( mNumComponents > mComponentsSize )
				{
					mComponentsSize = mNumComponents;
					mComponents = new Address[mComponentsSize];
					mComponentFirstResp = new ushort[mComponentsSize];
				}
				for ( int sen=0; sen<mNumComponents; sen++ )
				{
					offset += buf.Unmarshal(buffer,offset);
					mComponents[sen] = ProtocolFactory.CreateAddress(buf);
				}
				for ( int sen=0; sen<mNumComponents; sen++ )
					mComponentFirstResp[sen] = GetNetwork.USHORT(buffer,offset+sen*2);
				offset += mNumComponents*2;
			}

			int stringSize = length - (offset-start);
			if (stringSize > mStringDataSize)
			{
				mStringDataSize = stringSize;
				mData = new byte[mStringDataSize];
			}
			if ( mNumResponses*mNumItems > mResponsesSize )
			{
				mResponsesSize = mNumResponses*mNumItems;
				mResponses = new ICountedBlock[mResponsesSize];
			}
			if ( stringSize > 0 )
			{
				Buffer.BlockCopy(buffer,offset,mData,0,stringSize);
				if (mData[0] == 0)
					mData[1] = 0;

				var vlen = new VariableLength();
				int index = 0;
				for (int resp=0; resp<mNumResponses; resp++)
					for (int item=0; item<mNumItems; item++)
					{
						index += vlen.Unmarshal(mData, index);
						if (mData[index] >= (byte)StructuredQuery.sq.Elements)
						{
							mResponses[resp*mNumItems+item] = new MarshaledBuffer(vlen.Length, mData, index);
							index += vlen.Length;
						}
						else
						{
							mResponses[resp*mNumItems+item] = new MarshaledString(vlen.Length-1, mData, index);
							index += vlen.Length;
						}
					}
				offset += index;

				if ( offset < length )
				{
					MarshaledString str = new MarshaledString();
					str.Unmarshal(buffer,offset);
					Error = str.String;
				}
			}

			return offset-start;
		}

	}
}
