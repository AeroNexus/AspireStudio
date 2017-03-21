using System;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;

namespace Aspire.Core
{
	public class NativeProtocol : ControlProtocol
	{
		const int KBytes = 1024;
		const int MaxAddressSize = 8;
		const int MaxHeaderSize = 24;
		const int shiftWidth = 4; // Note: This presumes that the largest interfaceId or messageId will be 15

		protected enum LogLevel { Info, Error, Warning, Trace }

		protected enum msg // Assume shiftWidth = 4
		{
			Control_Probe=0x11, Control_ProbeReply, Control_Heartbeat, Control_HeartbeatReply,
			Control_Ima, Control_ImaAck, Control_AddressRequest, Control_AddressAck,
			Control_TimeAtTheTone, Control_GetCoreComponentList,
			Control_GetCoreComponentListReply, Control_CoreComponentListEntry,
            Control_SoftPps, // SoftPps=29, max of 32 (16+16), else have to change shiftWidth = 5 or default of 8

			Registration_Hello=0x21, Registration_HelloAck, Registration_Goodbye, Registration_GoodbyeAck,

			Xteds_GetxTEDS=0x31, Xteds_GetxTEDSReply, Xteds_WritexTEDS, Xteds_WritexTEDSReply,

			Subscription_Request=0x41, Subscription_RequestReply, Subscription_Cancel, Subscription_CancelReply,

			FileHandling_ReadFile=0x51, FileHandling_ReadFileReply, FileHandling_WriteFile, FileHandling_WriteFileReply,

			Query_Query=0x61, Query_QueryReply, Query_ProviderChanged, Query_ProviderChangedAck,

			Process_GetProperty=0x71, Process_GetPropertyReply, Process_Ping, Process_Pong, Process_SetProperty,
					Process_SetPropertyReply
		}

		public enum Ack { OK = 1, ERROR }
		public enum CoreComponentType { NETWORK_MANAGER = 1, CENTRAL_ID_SERVICE, TASK_MANAGER,
			SENSOR_MANAGER, NETWORK_DATA_STORE, DIRECTORY, GATEWAY, PROCESSOR_MANAGER }
		public enum FileReply { FILE_NOT_FOUND = 1, INSUFFICIENT_SPACE, OK }
		public enum Mode { REMOVE=1, ADD, START }
		public enum ProcessProperty
		{
			/// <summary>
			/// Component unique id
			/// </summary>
			CompUid,
			/// <summary>
			/// Log incoming messages
			/// </summary>
			LogIncomingMsgs,
			/// <summary>
			/// Log outgoing messages
			/// </summary>
			LogOutgoingMsgs,
			/// <summary>
			/// Address to log messages to
			/// </summary>
			MsgLogAddress,
			/// <summary>
			/// OS PID
			/// </summary>
			Pid,
			/// <summary>
			/// Component's xTEDS unique id
			/// </summary>
			XtedsUid
		}
		public enum ProcessResult { OK, Unsupported, Error }
		public enum ProviderState { Active=1, Dormant }
		public enum QueryType { Structured = 1, SpaStd, Regex, User };
		public enum Reply { Unknown, OK = 1, ERROR, WAIT, RESUMED, InProcess }
		public enum ScopeEnum
		{
			Local=1,		///< Use only the local domain
			Remote,			///< Use the remote domain
			LocalBackup,	///< Use the backup local domain
			RemoteBackup
		};

		protected Address mLocalManagerAddress;		///< Address of the local manager.
		protected CoreAddress mRegistrar;			///< Obsolete: remove next release.
		protected IApplicationLite mAppLite;
		protected MarshaledString mXtedsString;
		protected Message mSendMessage;
		protected Uuid mCompUid, mXtedsUid = new Uuid(16);
		protected SecTime mRegistrationTime = new SecTime();				///< Time when registration begins. Used to time the process.

		Address mScratchAddress, mScratchAddress2;
		AppMessage mCoreComponentListEntryMsg = new AppMessage(true);
		MarshaledBuffer
			mParseBuffer1 = new MarshaledBuffer(),
			mParseBuffer2 = new MarshaledBuffer(),
			mSendBuffer = new MarshaledBuffer();
		MarshaledString
			mParseString = new MarshaledString(),
			mSendString = new MarshaledString();
		Message mParseMessage, mSendMessage2;
		int mLogLevel;

		public NativeProtocol(IApplicationLite appLite, Transport transport, MarshaledString xtedsString, Uuid compUid) :
			base(appLite as IApplication, transport, ProtocolId.Aspire)
		{
			mXtedsString = xtedsString;
			mCompUid = compUid.IsEmpty ? Uuid.NewUuid() : compUid;
			mAppLite = appLite;

			GenerateXtedsUid(mXtedsString, mXtedsUid);
			SourceAddress = mTransport.ListenAddress;
			//mCoreComponentListEntryMsg.setSubscriptionCallback(_OnCoreComponentListEntrySubscribedTo);

			mLogLevel = (int)LogLevel.Trace;

			mParseMessage = ProtocolFactory.CreateMessage(Id);
			mParseMessage.Version = (byte)Id;
			mParseMessage.ShiftWidth = shiftWidth;

			mSendMessage = ProtocolFactory.CreateMessage(Id);
			mSendMessage.Version = (byte)Id;
			mSendMessage.ShiftWidth = shiftWidth;

			mSendMessage2 = ProtocolFactory.CreateMessage(Id);
			mSendMessage2.Version = (byte)Id;
			mSendMessage2.ShiftWidth = shiftWidth;
			mScratchAddress = ProtocolFactory.CreateAddress(Id);
			mScratchAddress2 = mScratchAddress.Clone();
		}

		public void CancelCoreComponentListEntry(Address destination, Address client=null)
		{
			MessageId messageId = new MessageId();
			messageId.SetHash((int)msg.Control_CoreComponentListEntry, shiftWidth);
			SendSubscriptionCancel(destination, messageId,client);
		}

		public override ControlProtocol Clone()
		{
			var cp = new NativeProtocol(mApplication, mTransport, mXtedsString, mCompUid);
			return cp;
		}

		public Uuid CompUid { get { return mCompUid; } set { mCompUid = value; } }

		public override IPublisher FindMessage(MessageId messageId)
		{
			switch ( (msg)messageId.Hash(shiftWidth) )
			{
				case msg.Control_CoreComponentListEntry:
					return mCoreComponentListEntryMsg;
			}
			return null;
		}

		public void GenerateXtedsUid(string xtedsText, Uuid uid)
		{
			byte[] md5 = new byte[16];
			MD5.Generate(xtedsText, md5);
			uid.SetFromMd5(md5);
		}

		bool HandleUnknownMessage(Message message)
		{
			return false;
		}

		public override bool IsOperational { get { return true; } }

		public override Address LocalManagerAddress { get { return mLocalManagerAddress; } }

		//public override int MessageSize { get { return mSendMessage.Size; } }

		protected void OnAddressAck(Message message, Ack AckStatus)
		{
			int level = AckStatus == Ack.OK ? mLogLevel : (int)LogLevel.Error;
			Logger.Log(level,"{1}:AddressAck({0})", AckStatus, mAppLite.Name);
		}

		protected void OnAddressRequest(Message message, Mode Mode, CoreComponentType Type,
			Address address, string Domain)
		{
			Logger.Log(mLogLevel, "{4}:AddressRequest({0}, {1}, {2}, {3})",
				Mode, Type, address.ToString(), Domain, mAppLite.Name);
		}

		protected virtual void OnCoreComponentListEntry(Message message, Mode Mode,
			CoreComponentType Type, Address address, ScopeEnum Scope, string Domain)
		{
			Logger.Log(mLogLevel, "{5}:CoreComponentListEntry({0}, {1}, {2}, {3}, {4})",
				Mode, Type, address.ToString(), Scope, Domain, mAppLite.Name);
		}

		protected virtual void OnCoreComponentListEntrySubscribedTo(IDataMessage message, Subscription subscription, bool cancel)
		{
			Logger.Log(mLogLevel, "{3}:NativeProtocol::On CoreComponentListEntry SubscribedTo({0},{1},{2})",
			message.Name,subscription.Address.ToString(),cancel?"cancel":"add", mAppLite.Name);
		}

		protected virtual void OnGetCoreComponentList(Message message)
		{
			Logger.Log(mLogLevel, "GetCoreComponentList");
		}

		protected virtual void OnGetCoreComponentListReply(Message message, Mode Mode,
			CoreComponentType Type, Address address, ScopeEnum Scope, string Domain)
		{
			Logger.Log(mLogLevel, "{5}:GetCoreComponentListReply({0}, {1}, {2}, {3}, {4})",
				Mode, Type, address.ToString(), Scope, Domain, mAppLite.Name);
		}

		protected virtual void OnGetProperty(Message message, ProcessProperty property)
		{
			Logger.Log(mLogLevel, "GetProperty({0})", property);
		}

		protected virtual void OnGetxTEDS(Message message)
		{
			Logger.Log(mLogLevel, "GetxTEDS()");
		}

		protected void OnGetxTEDSReply(Message message, MarshaledBuffer XtedsID, MarshaledString xTEDS)
		{
			if (Logger.LogLevel >= (int)LogLevel.Trace)
			{
				var xtedsId = new Uuid(XtedsID.Bytes,16);
				Logger.Log(mLogLevel, "GetxTEDSReply({0}\n{1})", xtedsId.ToString(), xTEDS.Chars);
			}
		}

		protected virtual void OnGoodbye(Message message, Address address)
		{
			Logger.Log(mLogLevel, "Goodbye({0})", address.ToString());
		}

		protected virtual void OnGoodbyeAck(Message message, Reply ReplyStatus)
		{
			Logger.Log(mLogLevel, "GoodbyeAck({0})", ReplyStatus);
		}

		protected void OnHeartbeat(Message message, UInt16 ReplyCount, UInt16 ReplyRate, UInt16 ReplyDetail)
		{
			Logger.Log(mLogLevel, "Heartbeat({0},{1},{2})", ReplyCount, ReplyRate, ReplyDetail);
		}

		protected void OnHeartbeatReply(Message message, UInt32 Fault)
		{
			Logger.Log(mLogLevel, "HeartbeatReply({0})", Fault);
		}

		protected void OnHello(Message message, MarshaledBuffer CompID, MarshaledBuffer XtedsID)
		{
			if (Logger.LogLevel >= (int)LogLevel.Trace)
			{
				var compId = new Uuid(CompID.Bytes,16);
				var xtedsId = new Uuid(XtedsID.Bytes,16);
				Logger.Log(mLogLevel, "Hello[{0}]({1},{2})", message.Source.ToString(),compId.ToString(),
					xtedsId.ToString());
			}
		}

		protected virtual void OnHelloAck(Message message, Reply ReplyStatus, Address previousAddress=null)
		{
			Logger.Log(mLogLevel, "{3}:HelloAck[{0}]({1},{2})", message.Source.ToString(),ReplyStatus,
				previousAddress != null ? previousAddress.ToString() : string.Empty,
				mAppLite.Name);
		}

		protected void OnIma(Message message, CoreComponentType Type, string Domain)
		{
			Logger.Log(mLogLevel, "Ima({0}) from {1}, Domain {2}",
				Type, message.Source.ToString(), Domain);
		}

		protected void OnImaAck(Message message, Ack AckStatus)
		{
			Logger.Log(mLogLevel, "ImaAck({0})", AckStatus);
		}

		protected virtual void OnPing(Message message)
		{
			Logger.Log(mLogLevel, "Ping()");
		}

		protected virtual void OnProbe(Message message, UInt16 ReplyCount, UInt16 ReplyRate)
		{
			Logger.Log(mLogLevel, "Probe({0},{1})", ReplyCount, ReplyRate);
		}

		protected void OnProbeReply(Message message, UInt32 Uptime, MarshaledBuffer CompID,
			MarshaledBuffer XtedsID, UInt32 Fault, MarshaledString Capabilities)
		{
			if (Logger.LogLevel >= (int)LogLevel.Trace)
			{
				var compId = new Uuid(CompID.Bytes,16);
				var xtedsId = new Uuid(XtedsID.Bytes,16);
				Logger.Log(mLogLevel, "ProbeReply({0},{1},{2},{3},{4})", Uptime, compId.ToString(),
					xtedsId.ToString(), Fault, Capabilities.Chars);
			}
		}

		protected virtual void OnProviderChanged(Message message, ProviderState ProviderState, Address address, Address prevAddress)
		{
			Logger.Log(mLogLevel, "ProviderChanged({0},{1},{2})", ProviderState, address.ToString(), prevAddress.ToString());
		}

		protected virtual void OnProviderChangedAck(Message message)
		{
			Logger.Log(mLogLevel, "ProviderChangedAck()");
		}

		protected void OnQuery(Message message, QueryType QueryType, MarshaledBuffer Specification)
		{
			if (Logger.LogLevel >= (int)LogLevel.Trace)
			{
				MsgConsole.WriteLine("Query({0})", QueryType);
				if ( QueryType == NativeProtocol.QueryType.Structured )
				{
					byte[] spec = Specification.Bytes;
					int offset = Specification.Offset;
					UInt32 addressHash = spec[offset];
					bool futures = spec[offset+4]==1;
					var sbuf = new MarshaledBuffer(Specification.Length-5,spec,offset+5);
					MsgConsole.WriteLine("  Address {0}, futures:{1}, ",addressHash, futures?"true":"false");
					StructuredResponse.PrintOpStr(sbuf);
				}
			}
		}

		protected virtual void OnQueryReply(Message message, QueryType QueryType, MarshaledBuffer Specification)
		{
			Logger.Log(mLogLevel, "{1}: QueryReply({0})", QueryType, mAppLite.Name);
		}

		protected void OnReadFile(Message message, string FileName)
		{
			Logger.Log(mLogLevel, "ReadFile({0})", FileName);
		}

		protected void OnReadFileReply(Message message, FileReply ReplyStatus, MarshaledBuffer Data)
		{
			if (Logger.LogLevel >= (int)LogLevel.Trace)
			{
				var hex = BitConverter.ToString(Data.Bytes, Data.Offset, Data.Length);
				MsgConsole.WriteLine("ReadFileReply({0},{1})", ReplyStatus, hex);
			}
		}

		protected virtual void OnSetProperty(Message message, ProcessProperty property, int value, string strValue)
		{
			Logger.Log(mLogLevel, "SetProperty({0},{1},{2})", property, value, strValue);
		}

		protected virtual void OnSubscriptionCancel(Message message, MessageId messageId,
			Address client=null)
		{
			if (Logger.LogLevel >= (int)LogLevel.Trace)
			{
				if ( client != null )
					MsgConsole.WriteLine("Subscription Cancel({0},{1}) from {2}",
						messageId.ToString(),client.ToString(), message.Source.ToString());
				else
					MsgConsole.WriteLine("Subscription Cancel({0}) from {1}",
						messageId.ToString(), message.Source.ToString());
			}
		}

		protected virtual void OnSubscriptionCancelReply(Message message, Ack AckStatus)
		{
			Logger.Log(mLogLevel, "Subscription Cancel Reply({0})", AckStatus);
		}

		protected virtual void OnSubscriptionRequest(Message message, MessageId messageId,
			byte Ith, byte Priority, int requestedLeasePeriod, Address client=null)
		{
			if (Logger.LogLevel >= (int)LogLevel.Trace)
			{
				if ( client != null )
					MsgConsole.WriteLine("Subscription Request From {5}, ({0},{1},{2},{3},{4})",
						messageId.ToString(), Ith, Priority, requestedLeasePeriod, client.ToString(),
						message.Source.ToString());
				else
					MsgConsole.WriteLine("Subscription Request From {4}, ({0},{1},{2},{3})",
						messageId.ToString(), Ith, Priority, requestedLeasePeriod,
						message.Source.ToString());
			}
		}

		protected virtual void OnSubscriptionRequestReply(Message message, Ack AckStatus,
			int grantedLeasePeriod, MessageId messageId, Address client=null)
		{
			if (Logger.LogLevel >= (int)LogLevel.Trace)
			{
				if ( client != null )
					MsgConsole.WriteLine("{5}: SubscriptionRequestReply From {4}, ({0},{1},{2},{3})",
						AckStatus, grantedLeasePeriod, messageId.ToString(),client.ToString(),
						message.Source.ToString(), mAppLite.Name);
				else
					MsgConsole.WriteLine("{4}:SubscriptionRequestReply({0},{1},{2})", AckStatus,
						grantedLeasePeriod, messageId.ToString(),
						message.Source.ToString(), mAppLite.Name);
			}
		}

		protected virtual void OnTimeAtTheTone(Message message, UInt32 ToneSec, UInt32 ToneSubSec, float TimeRatio)
		{
			Logger.Log(mLogLevel, "TimeAtTheTone({0}.{1:D6},{2})", ToneSec, ToneSubSec, TimeRatio);
		}

        protected virtual void OnSoftPps(Message message)
        {
			Logger.Log(mLogLevel, "SoftPps()");
        }

		protected void OnWriteFile(Message message, string FileName, MarshaledBuffer Data)
		{
			if (Logger.LogLevel >= (int)LogLevel.Trace)
			{
				var hex = BitConverter.ToString(Data.Bytes, Data.Offset, Data.Length);
				MsgConsole.WriteLine("WriteFile(%s,%s)", FileName, hex);
			}
		}

		protected void OnWriteFileReply(Message message, FileReply ReplyStatus)
		{
			Logger.Log(mLogLevel, "WriteFileReply({0})", ReplyStatus);
		}

		protected void OnWritexTEDS(Message message, MarshaledString xTEDS)
		{
			Logger.Log(mLogLevel, "WritexTEDS({0})", xTEDS.Chars);
		}

		protected void OnWritexTEDSReply(Message message, Ack AckStatus, MarshaledBuffer XtedsID)
		{
			if (Logger.LogLevel >= (int)LogLevel.Trace)
			{
				var xtedsUid = new Uuid(XtedsID.Bytes,XtedsID.Offset);
				MsgConsole.WriteLine("WritexTEDSReply(%s,%s)", AckStatus, xtedsUid.ToString());
			}
		}

		MessageId parseMessageId = new MessageId();
		byte[] swap = new byte[8];

        /// <summary>
        /// Serialize access to NativeProtocol::Parse
        /// 
        /// Unfortunately there is static data that is modified here that makes the code non-thread safe.
        /// See ie Xteds.Interface.currentInterface for an example
        /// </summary>
        private readonly static object parseMutex = new object();
        public override bool Parse(byte[] buffer, int len, Message parseMessage)
        {
            lock(parseMutex)
            {
                return ParseInternal(buffer, len, parseMessage);
            }
        }

        bool ParseInternal(byte[] buffer, int len, Message parseMessage)
		{
			if (parseMessage==null) parseMessage = mParseMessage;
			int offset = parseMessage.Unmarshal(buffer, 0, len), value;
			Ack ack;
			FileReply fileReply;
			CoreComponentType type;
			Mode mode;
			ProcessProperty property;
			ProviderState providerState;
			QueryType queryType;
			Reply reply;
			ScopeEnum scope;

			switch ( (msg)parseMessage.Selector )
			{
				case msg.Control_AddressAck:
					ack = (Ack)buffer[offset];
					OnAddressAck(parseMessage, ack);
					break;
				case msg.Control_AddressRequest:
					mode = (Mode)buffer[offset];
					type = (CoreComponentType)buffer[offset+1];
					offset += mParseBuffer1.Unmarshal(buffer, offset+2)+2;
					mScratchAddress.Unmarshal(mParseBuffer1);
					mParseString.Unmarshal(buffer, offset);
					OnAddressRequest(parseMessage, mode, type, mScratchAddress, mParseString.String);
					break;
				case msg.Control_CoreComponentListEntry:
					mode = (Mode)buffer[offset];
					type = (CoreComponentType)buffer[offset+1];
					offset += mParseBuffer1.Unmarshal(buffer, offset+2)+2;
					mScratchAddress.Unmarshal(mParseBuffer1);
					scope = (ScopeEnum)buffer[offset];
					mParseString.Unmarshal(buffer, offset+1);
					OnCoreComponentListEntry(parseMessage, mode, type, mScratchAddress,
						scope, mParseString.String);
					break;
				case msg.Control_GetCoreComponentList:
					OnGetCoreComponentList(parseMessage);
					break;
				case msg.Control_GetCoreComponentListReply:
					mode = (Mode)buffer[offset];
					type = (CoreComponentType)buffer[offset+1];
					offset += mParseBuffer1.Unmarshal(buffer, offset+2)+2;
					mScratchAddress.Unmarshal(mParseBuffer1);
					scope = (ScopeEnum)buffer[offset];
					mParseString.Unmarshal(buffer, offset+1);
					OnGetCoreComponentListReply(parseMessage, mode, type, mScratchAddress,
						scope, mParseString.String);
					break;
				case msg.Control_Heartbeat:
					OnHeartbeat(parseMessage, GetNetwork.USHORT(buffer,offset), GetNetwork.USHORT(buffer,offset+2), GetNetwork.USHORT(buffer,offset+4));
					break;
				case msg.Control_HeartbeatReply:
					OnHeartbeatReply(parseMessage, GetNetwork.UINT(buffer,offset,swap));
					break;
				case msg.Control_Ima:
					type = (CoreComponentType)buffer[offset];
					mParseString.Unmarshal(buffer, offset+1);
					OnIma(parseMessage, type, mParseString.String);
					break;
				case msg.Control_ImaAck:
					ack = (Ack)buffer[offset];
					OnImaAck(parseMessage, ack);
					break;
				case msg.Control_Probe:
					OnProbe(parseMessage, GetNetwork.USHORT(buffer, offset), GetNetwork.USHORT(buffer,offset+2));
					break;
				case msg.Control_ProbeReply:
					{
					UInt32 Uptime = GetNetwork.UINT(buffer, offset, swap);
				    offset += mParseBuffer1.Unmarshal(buffer,offset+4)+4;
					offset += mParseBuffer2.Unmarshal(buffer, offset);
					UInt32 Fault = GetNetwork.UINT(buffer, offset, swap);
				    mParseString.Unmarshal(buffer,offset+4);
				    OnProbeReply(parseMessage,Uptime,mParseBuffer1,mParseBuffer2,Fault,mParseString);
					}
					break;
				case msg.Control_TimeAtTheTone:
					float timeRatio;
					if (parseMessage.Length > 8)
						timeRatio = GetNetwork.FLOAT(buffer, offset+8, swap);
					else
						timeRatio = 1.0f;
					OnTimeAtTheTone(parseMessage, GetNetwork.UINT(buffer, offset, swap), GetNetwork.UINT(buffer, offset+4, swap), timeRatio);
					break;
                case msg.Control_SoftPps:
                    OnSoftPps(parseMessage);
                    break;
				case msg.Registration_Goodbye:
					mParseBuffer1.Unmarshal(buffer, offset);
					mScratchAddress.Unmarshal(mParseBuffer1);
					OnGoodbye(parseMessage, mScratchAddress);
					break;
				case msg.Registration_GoodbyeAck:
					reply = (Reply)buffer[offset];
					OnGoodbyeAck(parseMessage, reply);
					break;
				case msg.Registration_Hello:
				    offset += mParseBuffer1.Unmarshal(buffer,offset);
					mParseBuffer2.Unmarshal(buffer, offset);
				    OnHello(parseMessage,mParseBuffer1,mParseBuffer2);
					break;
				case msg.Registration_HelloAck:
					reply = (Reply)buffer[offset++];
					if (parseMessage.Length > 1)
					{
						mParseBuffer1.Unmarshal(buffer,offset);
						mScratchAddress.Unmarshal(mParseBuffer1);
						OnHelloAck(parseMessage, reply, mScratchAddress);
					}
					else
						OnHelloAck(parseMessage, reply);
					break;
				case msg.Xteds_GetxTEDS:
					OnGetxTEDS(parseMessage);
					break;
				case msg.Xteds_GetxTEDSReply:
					offset += mParseBuffer1.Unmarshal(buffer, offset);
					mParseString.Unmarshal(buffer, offset);
				    OnGetxTEDSReply(parseMessage,mParseBuffer1,mParseString);
					break;
				case msg.Xteds_WritexTEDS:
					mParseString.Unmarshal(buffer, offset);
				    OnWritexTEDS(parseMessage,mParseString);
					break;
				case msg.Xteds_WritexTEDSReply:
					ack = (Ack)buffer[offset];
					mParseBuffer1.Unmarshal(buffer, offset+1);
				    OnWritexTEDSReply(parseMessage,ack,mParseBuffer1);
					break;
				case msg.Subscription_Cancel:
					offset += parseMessageId.Unmarshal(buffer, offset);
					if (parseMessage.Length > 2)
					{
						mParseBuffer1.Unmarshal(buffer, offset+2);
						mScratchAddress.Unmarshal(mParseBuffer1);
						OnSubscriptionCancel(parseMessage, parseMessageId, mScratchAddress);
					}
					else
						OnSubscriptionCancel(parseMessage, parseMessageId,null);
					break;
				case msg.Subscription_CancelReply:
				    ack = (Ack)buffer[offset];
				    OnSubscriptionCancelReply(parseMessage,ack);
					break;
				case msg.Subscription_Request:
				    offset += parseMessageId.Unmarshal(buffer,offset);
					if (parseMessage.Length > 5)
					{
						mParseBuffer1.Unmarshal(buffer, offset+3);
						mScratchAddress.Unmarshal(mParseBuffer1);
						OnSubscriptionRequest(parseMessage, parseMessageId,
							buffer[offset], buffer[offset+1], buffer[offset+2], mScratchAddress);
					}
					else
					    OnSubscriptionRequest(parseMessage,parseMessageId,
							buffer[offset],buffer[offset+1],buffer[offset+2],null);
					break;
				case msg.Subscription_RequestReply:
					ack = (Ack)buffer[offset];
					parseMessageId.Unmarshal(buffer,offset+2);
					if (parseMessage.Length > 4)
					{
						mParseBuffer1.Unmarshal(buffer, offset+4);
						mScratchAddress.Unmarshal(mParseBuffer1);
						OnSubscriptionRequestReply(parseMessage, ack, buffer[offset+1], parseMessageId, mScratchAddress);
					}
					else
						OnSubscriptionRequestReply(parseMessage, ack, buffer[offset+1], parseMessageId,null);
					break;
				case msg.FileHandling_ReadFile:
					mParseString.Unmarshal(buffer, offset);
				    OnReadFile(parseMessage,mParseString.String);
					break;
				case msg.FileHandling_ReadFileReply:
					fileReply = (FileReply)buffer[offset];
					mParseBuffer1.Unmarshal(buffer, offset+1);
				    OnReadFileReply(parseMessage,fileReply,mParseBuffer1);
					break;
				case msg.FileHandling_WriteFile:
					offset += mParseString.Unmarshal(buffer, offset);
					mParseBuffer1.Unmarshal(buffer, offset);
				    OnWriteFile(parseMessage,mParseString.String,mParseBuffer1);
					break;
				case msg.FileHandling_WriteFileReply:
					fileReply = (FileReply)buffer[offset];
				    OnWriteFileReply(parseMessage,fileReply);
					break;
				case msg.Query_ProviderChanged:
					providerState = (ProviderState)buffer[offset];
					offset += mParseBuffer1.Unmarshal(buffer, offset+1) + 1;
					mScratchAddress.Unmarshal(mParseBuffer1.Bytes, mParseBuffer1.Offset);
					mParseBuffer2.Unmarshal(buffer, offset);
					mScratchAddress2.Unmarshal(mParseBuffer2.Bytes, mParseBuffer2.Offset);
					OnProviderChanged(parseMessage, providerState, mScratchAddress, mScratchAddress2);
					break;
				case msg.Query_ProviderChangedAck:
				    OnProviderChangedAck(parseMessage);
					break;
				case msg.Query_Query:
					queryType = (QueryType)buffer[offset];
					mParseBuffer1.Unmarshal(buffer, offset+1);
				    OnQuery(parseMessage,queryType,mParseBuffer1);
					break;
				case msg.Query_QueryReply:
					queryType = (QueryType)buffer[offset];
					mParseBuffer1.Unmarshal(buffer, offset + 1);
					OnQueryReply(parseMessage, queryType, mParseBuffer1);
					break;
				case msg.Process_GetProperty:
					property = (ProcessProperty)buffer[offset];
					OnGetProperty(parseMessage, property);
					break;
				case msg.Process_Ping:
					OnPing(parseMessage);
					break;
				case msg.Process_SetProperty:
					property = (ProcessProperty)buffer[offset];
					value = GetNetwork.INT(buffer, offset+1, swap);
					mParseString.Unmarshal(buffer, offset+5);
					OnSetProperty(parseMessage, property, value, mParseString);
					break;
				case 0:
					break;
				default:
					if ( !HandleUnknownMessage(parseMessage) )
						Logger.Log(mLogLevel,"{0}: Bogus msg id x{1:X}", mAppLite,parseMessage.Selector);
					return false;
			}
			return true;
		}

		void PublishCoreComponentListEntry(Mode Mode, CoreComponentType Type, Address address,
			ScopeEnum Scope, string Domain)
		{
			var pub = mCoreComponentListEntryMsg.Publication;
			if (pub != null)
				foreach (var sub in pub.Subscriptions )
					if ( sub.ShouldPublish(1) )
						SendCoreComponentListEntry(sub.Address, Mode, Type, address, Scope, Domain);
		}

		public override int RegisterQueryClient(IQueryClient client)
		{
			return -1;
		}

		public virtual bool Resumed() { return false; }

		public string ScopeAsString(ScopeEnum scope)
		{
			return Enum.GetName(typeof(ScopeEnum), scope);
		}

		/// <summary>
		/// Send a message using an external message header
		/// </summary>
		/// <param name="header">The external message header.</param>
		/// <param name="msgSelector">The message selector identifying the specific message.</param>
		/// <param name="buffer">The message buffer, already marshaled in network byte order.</param>
		/// <param name="payloadOffset">The offset into the messagge buffer where the payload starts, already marshaled in network byte order.</param>
		/// <param name="payloadLength">Length of the payload buffer.</param>
		/// <param name="destination">Address of the Destination component.</param>
		/// <param name="sequence">(optional) The sequence number.</param>
		/// <returns>The number of bytes sent</returns>
		public virtual int Send(int msgSelector, byte[] payload, int offset, int payloadLength,
			Address destination=null, SecTime? timeout=null, int sequence=0)
		{
			Message header = mSendMessage;

			header.Sequence = sequence;
			header.Selector = msgSelector;
			header.Source = SourceAddress;
			header.Destination = destination;
			header.Source.WithRespectTo(destination);

			if ( timeout == null )
				return base.Send(header, payload, offset, payloadLength);
			else
				return base.Send(header, payload, offset, payloadLength, (SecTime)timeout, null);
		}

		public void SendAddressAck(Address destination, Ack AckStatus)
		{
			byte[] buffer = new byte[MaxHeaderSize+1];
			int payload = mSendMessage.Size;
			buffer[payload] = (byte)AckStatus;
			Send((int)msg.Control_AddressAck,buffer,payload,1,destination);
		}

		public void SendAddressRequest(Address destination, Mode Mode, CoreComponentType Type,
			Address address, string Domain)
		{
			Address compAddress = address;
			if (compAddress.IsLocal)
				compAddress = compAddress.Clone().WithRespectTo(destination);

			byte[] buffer = new byte[1*KBytes];
			int payload = mSendMessage.Size, offset = payload;
			buffer[offset] = (byte)Mode;
			buffer[offset+1] = (byte)Type;
			var vlen = new VariableLength(compAddress.Length);
			offset += vlen.Marshal(buffer,offset+2)+2;
			offset += compAddress.Marshal(buffer,offset);
			mSendString.Set(Domain);
			offset += mSendString.Marshal(buffer,offset);
			Send((int)msg.Control_AddressRequest, buffer, payload, offset-payload, destination);

			if ( compAddress != address )
				compAddress = null; // C++ deletes
		}

		public void SendCoreComponentListEntry(Address destination, Mode mode,
			CoreComponentType Type, Address address, ScopeEnum Scope, string Domain)
		{
			Address compAddress = address;
			if (compAddress.IsLocal)
				compAddress = compAddress.Clone().WithRespectTo(destination);

			byte[] buffer = new byte[1 * KBytes];
			int payload = mSendMessage.Size, offset = payload;
			buffer[offset] = (byte)mode;
			buffer[offset+1] = (byte)Type;
			var vlen = new VariableLength(compAddress.Length);
			offset += vlen.Marshal(buffer,offset+2)+2;
			offset += compAddress.Marshal(buffer,offset);
			buffer[offset] = (byte)(Scope);
			mSendString.Set(Domain);
			offset += mSendString.Marshal(buffer,offset+1)+1;
			Send((int)msg.Control_CoreComponentListEntry, buffer, payload, offset-payload, destination);

			if (compAddress != address)
				compAddress = null; // C++ deletes
		}

		public void SendGetCoreComponentList(Address destination)
		{
			byte[] buffer = new byte[MaxHeaderSize];
			int payload = mSendMessage.Size;
			Send((int)msg.Control_GetCoreComponentList, buffer, payload, 0, destination);
		}

		public void SendGetCoreComponentListReply(Address destination, Mode mode,
			CoreComponentType Type, Address address, ScopeEnum Scope, string Domain)
		{
			Address compAddress = address;
			if (compAddress.IsLocal)
				compAddress = compAddress.Clone().WithRespectTo(destination);

			byte[] buffer = new byte[1 * KBytes];
			int payload = mSendMessage.Size, offset = payload;
			buffer[offset] = (byte)mode;
			buffer[offset+1] = (byte)Type;
			var vlen = new VariableLength(compAddress.Length);
			offset += vlen.Marshal(buffer,offset+2)+2;
			offset += compAddress.Marshal(buffer,offset);
			buffer[offset] = (byte)(Scope);
			mSendString.Set(Domain);
			offset += mSendString.Marshal(buffer, offset+1)+1;
			Send((int)msg.Control_GetCoreComponentListReply, buffer, payload, offset-payload, destination);

			if (compAddress != address)
				compAddress = null; // C++ deletes
		}

		public void SendGetPropertyReply(Address destination, ProcessResult result,
			ProcessProperty property, int value, string strValue)
		{
			byte[] buffer = new byte[128];
			int payload = mSendMessage.Size, offset = payload;
			buffer[offset] = (byte)result;
			buffer[offset + 1] = (byte)property;
			PutNetwork.INT(buffer, 2, value);
			mSendString.Set(strValue);
			offset += mSendString.Marshal(buffer, offset + 6) + 6;
			Send((int)msg.Process_GetPropertyReply, buffer, payload, offset-payload, destination);
		}

		public void SendGetxTEDS(Address destination)
		{
			byte[] buffer = new byte[MaxHeaderSize];
			int payload = mSendMessage.Size;
			Send((int)msg.Xteds_GetxTEDS, buffer, payload, 0, destination);
		}

	  private const uint bufSize = 64*KBytes;
		byte[] xtedsReply = new byte[bufSize]; // datagram limit
		public void SendGetxTEDSReply(Address destination, Uuid XtedsID, MarshaledString xTEDS)
		{
			int payload = mSendMessage.Size, offset = payload;
			mSendBuffer.Set(Uuid.size,XtedsID.ToByteArray(),0);
			offset += mSendBuffer.Marshal(xtedsReply,offset);
			// Need to handle segmented xTEDS, probably from above. At least let everyone know
		  uint bytesAvail = (uint)(bufSize - offset);
		  if (xTEDS.Length > bytesAvail)
		  {
        Logger.Log(mLogLevel, "\nxTEDS too large; {0} > {1}\n\n", xTEDS.Length, bufSize);
		    var ms = new MarshaledString(XtedsHelper.ConstructXteds(string.Format("xTEDS[{0}]", xTEDS.Length)));
        offset += ms.Marshal(xtedsReply, offset);
		  }
      else
		    offset += xTEDS.Marshal(xtedsReply,offset);
			Send((int)msg.Xteds_GetxTEDSReply,xtedsReply,payload,offset-payload,destination);
		}

		public void SendGoodbye(Address destination, Address address, SecTime? timeout=null)
		{
			Address compAddress = address;
			if (compAddress.IsLocal)
				compAddress = compAddress.Clone().WithRespectTo(destination);

			Logger.Log(mLogLevel, "Sending GoodBye {0}, {1}", destination.ToString(), compAddress.ToString());

			byte[] buffer = new byte[1 * KBytes];
			int payload = mSendMessage.Size, offset = payload;
			var vlen = new VariableLength(compAddress.Length);
			offset += vlen.Marshal(buffer, offset);
			offset += compAddress.Marshal(buffer, offset);
			if ( timeout == null )
				Send((int)msg.Registration_Goodbye, buffer, payload, offset - payload, destination);
			else
				Send((int)msg.Registration_Goodbye, buffer, payload, offset - payload, destination, (SecTime)timeout);

			if (compAddress != address)
				compAddress = null; // C++ deletes
		}

		public void SendGoodbye(Address destination, Address address)
		{
			SendGoodbye(destination,address,SecTime.Infinite);
		}

		public void SendGoodbyeAck(Address destination, Reply replyStatus)
		{
			byte[] buffer = new byte[MaxHeaderSize+1];
			int payload = mSendMessage.Size;
			buffer[payload] = (byte)replyStatus;
			Send((int)msg.Registration_GoodbyeAck, buffer, payload, 1, destination);
		}

		public void SendHeartbeat(Address destination, UInt16 replyCount, UInt16 replyRate, UInt16 replyDetail)
		{
			byte[] buffer = new byte[MaxHeaderSize+6];
			int payload = mSendMessage.Size;
			PutNetwork.USHORT(buffer,payload,replyCount);
			PutNetwork.USHORT(buffer,payload+2,replyRate);
			PutNetwork.USHORT(buffer,payload+4,replyDetail);
			Send((int)msg.Control_Heartbeat, buffer, payload, 6, destination);
		}

		public void SendHeartbeatReply(Address destination, UInt32 fault)
		{
			byte[] buffer = new byte[MaxHeaderSize+4];
			int payload = mSendMessage.Size;
			PutNetwork.UINT(buffer, payload, fault);
			Send((int)msg.Control_HeartbeatReply, buffer, payload, 4, destination);
		}

		public void SendHello(Address destination, Uuid CompID, Uuid XtedsID)
		{
			byte[] buffer = new byte[256];
			int payload = mSendMessage.Size, offset = payload;
			mSendBuffer.Set(Uuid.size,CompID.ToByteArray(),0);
			offset += mSendBuffer.Marshal(buffer,offset);
			mSendBuffer.Bytes = XtedsID.ToByteArray();
			offset += mSendBuffer.Marshal(buffer,offset);
			Send((int)msg.Registration_Hello,buffer,payload,offset-payload,destination);
		}

		public void SendHelloAck(Address destination, Reply replyStatus, Address previousAddress=null)
		{
			byte[] buffer = new byte[256];
			int payload = mSendMessage.Size, offset = payload;
			buffer[offset++] = (byte)replyStatus;
			if (replyStatus == Reply.RESUMED)
			{
				var vlen = new VariableLength(previousAddress.Length);
				offset += vlen.Marshal(buffer, offset);
				offset += previousAddress.Marshal(buffer, offset);
			}
			Send((int)msg.Registration_HelloAck, buffer, payload, offset-payload, destination);
		}

		public void SendIma(Address destination, CoreComponentType Type, string Domain)
		{
			byte[] buffer = new byte[256];
			int payload = mSendMessage.Size, offset = payload;
			buffer[offset] = (byte)Type;
			mSendString.Set(Domain);
			offset += mSendString.Marshal(buffer,offset+1)+1;
			Send((int)msg.Control_Ima, buffer, payload, offset-payload, destination);
		}

		public void SendImaAck(Address destination, Ack AckStatus)
		{
			byte[] buffer = new byte[MaxHeaderSize+1+2];
			int payload = mSendMessage.Size;
			buffer[payload] = (byte)AckStatus;
			Send((int)msg.Control_ImaAck, buffer, payload, 1, destination);
		}

		//public override void SendNoOp(Address destination)
		//{
		//	byte[] buffer = new byte[MaxHeaderSize+2];
		//	Send(0,buffer,MaxHeaderSize,0,destination,0);
		//}

		public void SendProbe(Address destination, UInt16 replyCount, UInt16 replyRate)
		{
			byte[] buffer = new byte[MaxHeaderSize+4];
			int payload = mSendMessage.Size;
			PutNetwork.USHORT(buffer, payload, replyCount);
			PutNetwork.USHORT(buffer, payload+2, replyRate);
			Send((int)msg.Control_Probe, buffer, payload, 4, destination);
		}

		public void SendProbeReply(Address destination, UInt32 Uptime, Uuid CompID, Uuid XtedsID, UInt32 Fault, MarshaledString Capabilities)
		{
			byte[] buffer = new byte[1*KBytes];
			int payload = mSendMessage.Size, offset = payload;
			PutNetwork.UINT(buffer, offset, Uptime);
			mSendBuffer.Set(Uuid.size, CompID.ToByteArray(), 0);
			offset += mSendBuffer.Marshal(buffer, offset+4)+4;
			mSendBuffer.Set(Uuid.size, XtedsID.ToByteArray(), 0);
			offset += mSendBuffer.Marshal(buffer, offset);
			PutNetwork.UINT(buffer, offset, Fault);
			offset += Capabilities.Marshal(buffer, offset+4)+4;
			Send((int)msg.Control_ProbeReply, buffer, payload, offset-payload, destination);
		}

		public void SendProviderChanged(Address destination, ProviderState providerState,
			Address address, Address prevAddress)
		{
			Address compAddress = address;
			if (compAddress.IsLocal)
				compAddress = compAddress.Clone().WithRespectTo(destination);

			Address compPrevAddress = prevAddress;
			if (compPrevAddress.IsLocal)
				compPrevAddress = compPrevAddress.Clone().WithRespectTo(destination);

			byte[] buffer = new byte[1 * KBytes];
			int payload = mSendMessage.Size, offset = payload;
			buffer[offset] = (byte)providerState;
			var vlen = new VariableLength(compAddress.Length);
			offset += vlen.Marshal(buffer, offset);
			offset += compAddress.Marshal(buffer, offset);
			offset += vlen.Marshal(buffer, offset);
			offset += compPrevAddress.Marshal(buffer, offset);
			Send((int)msg.Query_ProviderChanged, buffer, payload, offset - payload, destination);

			if (compAddress != address)
				compAddress = null; // C++ deletes

			if (compPrevAddress != prevAddress)
				compPrevAddress = null; // C++ deletes
		}

		public void SendProviderChangedAck(Address destination)
		{
			byte[] buffer = new byte[MaxHeaderSize];
			int payload = mSendMessage.Size;
			Send((int)msg.Query_ProviderChangedAck,buffer,payload,0,destination);
		}

		public void SendQuery(int tag, QueryType QueryType,
			MarshaledBuffer Specification, Address registrar)
		{
			byte[] buffer = new byte[4*KBytes];
			int payload = mSendMessage.Size, offset = payload;
			buffer[offset] = (byte)QueryType;
			offset += Specification.Marshal(buffer,offset+1)+1;
			Send((int)msg.Query_Query,buffer,payload,offset-payload,registrar,null,tag);
		}

		void SendQueryReply(Address destination, int tag, QueryType QueryType, MarshaledBuffer Specification)
		{
			byte[] buffer = new byte[64*KBytes];
			int payload = mSendMessage.Size, offset = payload;
			buffer[offset] = (byte)QueryType;
			offset += Specification.Marshal(buffer,1)+1;
			Send((int)msg.Query_QueryReply, buffer, payload, offset-payload, destination, null, tag);
		}

		public void SendReadFile(Address destination, string FileName)
		{
			byte[] buffer = new byte[1*KBytes];
			int payload = mSendMessage.Size, offset = payload;
			mSendString.Set(FileName);
			offset += mSendString.Marshal(buffer, offset);
			Send((int)msg.FileHandling_ReadFile, buffer, payload, offset-payload, destination);
		}

		public void SendReadFileReply(Address destination, FileReply ReplyStatus, MarshaledBuffer Data)
		{
			int payload = mSendMessage.Size, offset = payload;
			xtedsReply[offset] = (byte)ReplyStatus;
			offset += Data.Marshal(xtedsReply, offset + 1) + 1;
			Send((int)msg.FileHandling_ReadFileReply, xtedsReply, payload, offset - payload, destination);
		}

		public void SendSetPropertyReply(Address destination, ProcessResult result, ProcessProperty property)
		{
			byte[] buffer = new byte[MaxHeaderSize+22];
			int payload = mSendMessage.Size;
			buffer[payload] = (byte)result;
			buffer[payload+1] = (byte)property;
			Send((int)msg.Process_SetPropertyReply, buffer, payload, 2, destination);
		}

		public override void SendSubscriptionCancel(Address destination, MessageId messageId,
			Address client = null, int dialog = 0)
		{
			byte[] buffer = new byte[MaxHeaderSize + 32];
			int payload = mSendMessage.Size, offset = payload;
			offset += messageId.Marshal(buffer, offset);
			if (client != null)
			{
				var vlen = new VariableLength(client.Length);
				offset += vlen.Marshal(buffer, offset);
				offset += client.Marshal(buffer, offset);
			}
			Send((int)msg.Subscription_Cancel, buffer, payload, offset - payload, destination);
		}

		public void SendSubscriptionCancel(Address destination, MessageId messageId,
			SecTime timeout, Address client = null, int sequence=0, int dialog = 0)
		{
			byte[] buffer = new byte[MaxHeaderSize + 32];
			int payload = mSendMessage.Size, offset = payload;
			offset += messageId.Marshal(buffer, offset);
			if (client != null)
			{
				var vlen = new VariableLength(client.Length);
				offset += vlen.Marshal(buffer, offset);
				offset += client.Marshal(buffer, offset);
			}
			Send((int)msg.Subscription_Cancel, buffer, payload, offset - payload, destination);
		}

		public void SendSubscriptionCancelReply(Address destination, Ack AckStatus)
		{
			byte[] buffer = new byte[MaxHeaderSize+1];
			int payload = mSendMessage.Size;
			buffer[payload] = (byte)AckStatus;
			Send((int)msg.Subscription_CancelReply, buffer, payload, 1, destination);
		}

		public override bool SendSubscriptionRequest(Address destination, MessageId messageId,
			byte Ith, byte Priority, int requestedLeasePeriod, Address client=null, bool resumeCanceled=false)
		{
			byte[] buffer = new byte[MaxHeaderSize + 32];
			int payload = mSendMessage.Size, offset = payload;
			offset += messageId.Marshal(buffer, offset);
			buffer[offset] = Ith;
			buffer[offset + 1] = Priority;
			buffer[offset + 2] = (byte)requestedLeasePeriod;
			offset += 3;
			if (client != null)
			{
				var vlen = new VariableLength(client.Length);
				offset += vlen.Marshal(buffer, offset);
				offset += client.Marshal(buffer, offset);
			}
			Send((int)msg.Subscription_Request, buffer, payload, offset - payload, destination);
			return false;
		}

		public void SendSubscriptionRequest(Address destination, MessageId messageId,
			byte Ith, byte Priority, int requestedLeasePeriod, SecTime timeout,
			Address client=null, int sequence=0)
		{
			byte[] buffer = new byte[MaxHeaderSize + 32];
			int payload = mSendMessage.Size, offset = payload;
			offset += messageId.Marshal(buffer, offset);
			buffer[offset] = Ith;
			buffer[offset + 1] = Priority;
			buffer[offset + 2] = (byte)requestedLeasePeriod;
			offset += 3;
			if (client != null)
			{
				var vlen = new VariableLength(client.Length);
				offset += vlen.Marshal(buffer, offset);
				offset += client.Marshal(buffer, offset);
			}
			Send((int)msg.Subscription_Request, buffer, payload, offset - payload, destination, timeout, sequence);
		}

		public override void SendSubscriptionRequestReply(Address destination, bool ack,
			int grantedLeasePeriod, MessageId messageId, Address client)
		{
			SendSubscriptionRequestReply(destination,
				ack ? NativeProtocol.Ack.OK : NativeProtocol.Ack.ERROR,
				grantedLeasePeriod, messageId, client);
		}

		public void SendSubscriptionRequestReply(Address destination, Ack AckStatus,
			int grantedLeasePeriod, MessageId messageId, Address client=null)
		{
			byte[] buffer = new byte[MaxHeaderSize + 32]; // leave room for 28 bytes of address+length
			int payload = mSendMessage.Size, offset = payload;

			buffer[offset] = (byte)AckStatus;
			buffer[offset + 1] = (byte)(grantedLeasePeriod & 0xff);
			offset += messageId.Marshal(buffer, offset + 2) + 2;
			if (client != null)
			{
				var vlen = new VariableLength(client.Length);
				offset += vlen.Marshal(buffer, offset);
				offset += client.Marshal(buffer, offset);
			}
			Send((int)msg.Subscription_RequestReply, buffer, payload, offset - payload, destination);
		}

		public override void SendTimeAtTheTone(Address destination, UInt32 toneSec, UInt32 toneSubSec,
			float timeRatio)
		{
			byte[] buffer = new byte[MaxHeaderSize+12];
			int payload = mSendMessage.Size, offset = payload;
			PutNetwork.UINT(buffer,payload,toneSec);
			PutNetwork.UINT(buffer, payload+4, toneSubSec);
			PutNetwork.FLOAT(buffer, payload+8, timeRatio, swap);
			Send((int)msg.Control_TimeAtTheTone, buffer, payload, 12, destination);
		}

        public void SendSoftPps(Address destination)
        {
            byte[] buffer = new byte[MaxHeaderSize];
	        int payload = mSendMessage.Size, offset = payload;
            Send((int)msg.Control_SoftPps, buffer, payload, 0, destination);
        }

		public void SendWriteFile(Address destination, string FileName, MarshaledBuffer Data)
		{
			int payload = mSendMessage.Size, offset = payload;
			mSendString.Set(FileName);
			offset += mSendString.Marshal(xtedsReply, offset);
			offset += Data.Marshal(xtedsReply, offset);
			Send((int)msg.FileHandling_WriteFile, xtedsReply, payload, offset-payload, destination);
		}

		public void SendWriteFileReply(Address destination, FileReply ReplyStatus)
		{
			byte[] buffer = new byte[MaxHeaderSize+1];
			int payload = mSendMessage.Size;
			buffer[payload] = (byte)ReplyStatus;
			Send((int)msg.FileHandling_WriteFileReply, buffer, payload, 1, destination);
		}

		public void SendWritexTEDS(Address destination, MarshaledString xTEDS)
		{
			int payload = mSendMessage.Size, offset = payload;
			offset += xTEDS.Marshal(xtedsReply, offset);
			Send((int)msg.Xteds_WritexTEDS, xtedsReply, payload, offset-payload, destination);
		}

		public void SendWritexTEDSReply(Address destination, Ack AckStatus, Uuid XtedsID)
		{
			byte[] buffer = new byte[256];
			int payload = mSendMessage.Size, offset = payload;
			buffer[offset] = (byte)AckStatus;
			mSendBuffer.Set(Uuid.size, XtedsID.ToByteArray(), 0);
			offset += mSendBuffer.Marshal(buffer, offset+1)+1;
			Send((int)msg.Xteds_WritexTEDSReply, buffer, payload, offset-payload, destination);
		}

		protected Address SourceAddress { get;  private set; }

		public override void Start()
		{
		}

		public override void Stop()
		{
		}

		public void SubscribeToCoreComponentListEntry(Address destination)
		{
			MessageId messageId = new MessageId();
			messageId.SetHash((int)msg.Control_CoreComponentListEntry, shiftWidth);
			SendSubscriptionRequest(destination, messageId, 1, 0, 0, SecTime.Milliseconds(10), null, 255);
		}

		public void UnsubscribeFromCoreComponentListEntry(Address destination)
		{
			MessageId messageId = new MessageId();
			messageId.SetHash((int)msg.Control_CoreComponentListEntry, shiftWidth);
			SendSubscriptionCancel(destination, messageId, SecTime.Milliseconds(10), null, 0, 255);
		}

		public string TypeAsString(CoreComponentType ccType)
		{
			return Enum.GetName(typeof(CoreComponentType), ccType);
		}

		public override Uuid XtedsUid { get { return mXtedsUid; } }

		protected MarshaledString XtedsString { get { return mXtedsString; } }

	}
}
