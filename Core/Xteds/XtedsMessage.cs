//using System;
using System.ComponentModel;
//using System.IO;
using System.Xml.Serialization;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;

namespace Aspire.Core.xTEDS
{
	public enum MessageTypeCode { NonSpecific, Command, Notification, Request, Reply, Fault }

	[TypeConverter(typeof(ExpandableObjectConverter))]
	public abstract class XtedsMessage : SecondaryHeaderHost
	{
		public delegate void Handler(XtedsMessage msg);
		public delegate void SubscriptionHandler(Subscription subscription, bool cancel);
		Message mHeader;
		IApplication mApplication;
		byte[] mZero = new byte[64];
		bool mVerified;

		public enum DispositionType { Deliver, Defer, Discard };

		protected int received, sent;

		public IApplication Application
		{
			set
			{
				mApplication = value;
				var reply = ReplyMessage;
				if (reply != null)
					reply.Application = value;
			}
		}

		public XtedsMessage Clear()
		{
			mVerified = false;
			mVarMap = null;
			return this;
		}

		public DispositionType Disposition
		{
			get;
			set;
		}

		[XmlIgnore]
		public Message Header
		{
			get { return mHeader; }
			set { mHeader = value; }
		}

		public bool HeaderIsSet { get { return mHeader != null; } }

		[XmlIgnore]
		public abstract string InterfaceName { get; }

		public bool IsSynchronous { get; set; }
		
		public abstract void LeaseHasExpired();

		public int Marshal(byte[] buffer, int offset, uint size)
		{
			//lock(mVarMap)
				if ( mVarMap == null )
					MapVariables(IVariables);
			int start = offset;
			//marshaling = true;
			foreach (var marshaler in mVarMap)
				if ( marshaler != null )
					offset += marshaler.Serialize(buffer, offset,(int)(size-offset));
			//marshaling = false;
			sent++;
			return offset-start;
		}

		int mVarMapLength;
		protected VariableMarshaler[] mVarMap;

		void AllocateVarMap(bool dynamic=false)
		{
			mVarMapLength = 0;
			mFixedLength = Size;
			mVarMap = new VariableMarshaler[NumVariables];
			string appName = mApplication != null ? mApplication.Name : appName = "?app";
			if (mVarMap.Length == 0 && NumVariables > 0) MsgConsole.WriteLine("{0} {1}:varMap[0]", appName, Name);
			if (!dynamic && WhenMessageArrives != null && MessageType.ToString().StartsWith("Notification"))
				MsgConsole.WriteLine("A potential race exists when specifying WhenMessageArrives() prior to MapVariable() for {0} {1}",
					appName, Name);
		}

		public bool IsMapped
		{
			get
			{
				if (mVarMap != null)
				{
					if (mVarMap.Length == 0) return true;
					foreach (var vm in mVarMap)
						if (vm != null) return true;
				}
				return false;
			}
		}

		public void MapVariables(IVariable[] variables)
		{
			if (mVerified || mVarMap != null) return;
			AllocateVarMap(true);
			int i = 0;
			foreach (var ivar in variables)
			{
				var marshaler = XtedsVariableMarshaler(ivar) as VariableMarshaler;
				if ( marshaler == null )
					marshaler = new VariableMarshaler(Name, ivar);
				mVarMap[i++] = marshaler;
			}
		}

		public XtedsMessage MapVariable(string name, string localName)
		{ return MapVariable(name, localName, PrimitiveType.unknownType, 1); }
		public XtedsMessage MapVariable(string name, string localName, PrimitiveType type)
		{ return MapVariable(name, localName, type, 1); }
		public XtedsMessage MapVariable(string name, string localName, PrimitiveType type, int length)
		{
			foreach (var ivar in IVariables)
				if (ivar.Name == name)
					return mapVariable(ivar, mApplication, localName, type, length);

			MsgConsole.WriteLine("Xteds Message {0} doesn't contain {1}", Name, name);
			return this;
		}

		public XtedsMessage MapVariable(string name, object container, string localName)
		{ return MapVariable(name, container, localName, PrimitiveType.unknownType, 0); }
		public XtedsMessage MapVariable(string name, object container, string localName, PrimitiveType type)
		{ return MapVariable(name, container, localName, type, 1); }
		public XtedsMessage MapVariable(string name, object container, string localName, PrimitiveType type, int length)
		{
			foreach (var ivar in IVariables)
				if (ivar.Name == name)
					return mapVariable(ivar, container, localName, type, length);

			MsgConsole.WriteLine("Xteds Message {0} doesn't contain {1}", Name, name);
			return this;
		}

		public XtedsMessage MapVariable(string name)
		{
			if (mVerified) return this;
			foreach (var ivar in IVariables)
				if (ivar.Name == name)
				{
					if (mVarMap == null) AllocateVarMap();
					mVarMap[mVarMapLength++] = mApplication.IKnownMarshaler.KnownMarshaler(ivar);
					return this;
				}

			MsgConsole.WriteLine("Xteds Message {0} doesn't contain {1}", Name, name);
			return this;
		}

		public XtedsMessage MapVariableKind(string kind, string localName, PrimitiveType type)
		{ return MapVariableKind(kind, localName, type, 1); }
		public XtedsMessage MapVariableKind(string kind, string localName, PrimitiveType type, int length)
		{
			foreach (var ivar in IVariables)
			    if (ivar.Kind == kind)
					return mapVariable(ivar, mApplication, localName, type, length);

			MsgConsole.WriteLine("Xteds Message {0} doesn't contain kind {1}", Name, kind);
			return this;
		}

		private XtedsMessage mapVariable(IVariable ivar, object container, string localName, PrimitiveType type, int length)
		{
			if (mVerified) return this;
			if (mVarMap == null) AllocateVarMap();
			int index = 0;
			foreach (var vm in mVarMap)
			{
				if (vm == null || vm.IVariable == null) break;
				else if (vm.ToString() == ivar.Name)
				{
					//if ( x )
						mVarMap[index] = new VariableMarshaler(Name, container, ivar, localName, type, length);
					//else
					//	MsgConsole.WriteLine("{0}.MapVariable: Won't add {1} a second time",
					//		Name, ivar.Name);
					return this;
				}
				index++;
			}
			if (mVarMapLength < mVarMap.Length)
				mVarMap[mVarMapLength++] = new VariableMarshaler(Name, container, ivar, localName, type, length);
			else
				MsgConsole.WriteLine("{0}.MapVariable: Can't add {1}: map full.",
					Name,ivar.Name);

			return this;
		}

		[XmlIgnore]
		public MessageId MessageId
		{
			get { return mMessageId; }
			set { mMessageId = value; }
		}
		protected MessageId mMessageId = new MessageId();

		public abstract IMessageType MessageType{get; }

		public abstract MessageTypeCode MessageTypeCode { get; }

		[XmlAttribute("name")]
		public abstract string Name { get; set; }

		public abstract int NumVariables { get; }

		[XmlIgnore]
		public Address Provider
		{
			get { return mProvider; }
			set { mProvider = value; }
		} Address mProvider;

		public abstract string ProviderName { get; }

		public XtedsMessage ReplyMessage
		{
			get
			{
				if (MessageType is IXtedsRequest)
				{
					var reply = (MessageType as IXtedsRequest).DataReplyXtedsMsg;
					if (this == reply)
						return null;
					else
						return reply;
				}
				else
					return null;
			}
		}

		public XtedsMessage RequestMessage
		{
			get
			{
				if (MessageType is IXtedsRequest)
				{
					var request = (MessageType as IXtedsRequest).CommandXtedsMsg;
					if (this == request)
						return null;
					else
						return request;
				}
				else
					return null;
			}
		}

		public abstract int Size { get; }

		//bool marshaling;
		int mFixedLength;

		public int Unmarshal(MarshaledBuffer buffer, Message header)
		{
			if (header.Sequence == 255 )
				LeaseHasExpired();
			mHeader = header;
			if (mVarMap == null)
				MapVariables(IVariables);
			int offset = buffer.Offset;
			byte[] bytes = buffer.Bytes;
			//marshaling = true;
			foreach (var marshaler in mVarMap)
			{
				if (marshaler == null) break;
				offset += marshaler.Deserialize(bytes, offset);
			}
			//marshaling = false;
			received++;
			if (WhenMessageArrives != null)
				WhenMessageArrives(this);
			return mFixedLength + offset-buffer.Offset;
		}

		public abstract IVariable IVariable(string name);

		[XmlIgnore]
		public abstract IVariable[] IVariables { get; }

		public bool Verified { get { return mVerified; } set { mVerified = value; } }

		public void VerifyVariableMapping()
		{
			if (NumVariables == 0 ||
				//mVarMap == null ||
				//!(this is IDataMessage) ||
				mVerified ) return;

			if (NumVariables > 0 && mVarMap == null)
				mVarMap = new VariableMarshaler[NumVariables];
			int i = 0;
			foreach (var ivar in IVariables)
			{
				bool found = false;
				foreach (var vm in mVarMap)
					if (vm != null && vm.ToString() == ivar.Name)
					{
						found = true;
						break;
					}
				if (!found)
				{
					var marshaler = XtedsVariableMarshaler(ivar) as VariableMarshaler;
					if (marshaler == null)
						marshaler = new VariableMarshaler(Name, ivar);
					if (mVarMap[i] == null)
					{
						mVarMap[i] = marshaler;
						if (i+1 >= mVarMapLength)
							mVarMapLength = i+1;
					}
					else if (mVarMap[i].IVariable == null) // remove
					{
						for (int j=i; j<mVarMapLength-1; j++)
							mVarMap[j] = mVarMap[j+1];
						mVarMapLength--;
					}
					else
					{
						for (int j=mVarMapLength; j>i; j--)
							mVarMap[j] = mVarMap[j-1];
						mVarMap[i] = marshaler;
						mVarMapLength++;
					}
				}
				i++;
			}
			mVerified = true;
		}

		public event Handler WhenMessageArrives;

		public abstract IVariableMarshaler XtedsVariableMarshaler(IVariable ivar);

		public abstract XtedsMessage Clone();
	}

	interface IMarshal
	{
	}
}
