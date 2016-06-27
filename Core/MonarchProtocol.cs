using System;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;

namespace Aspire.Core
{
	/// <summary>
	/// The Monarch protocol implements the SPA standard known as Monarch. It is available
	/// for compatibility reasons.
	/// </summary>
	public class MonarchProtocol : ControlProtocol, IBlueprint
	{
		const int KBytes = 1024;
		//const int MaxAddressSize = 4;
		const int MaxHeaderSize = 17;
		const int SM_L_PORT = 3500;
		const int segmentLength = 1400;

		enum Ack { Ok=0, Error, InitialRouteReceived, BridgeOntoProcessor, Last };
		enum LocalState { None, Discovery, LogicalAddressWait, RouteInfoWait, LookupSvcProbeWait, Operational};
		enum OperatingMode
		{
			Initializing, FullyOperational=5, DependencyFailure=7, LinkAnomaly=11,
			PacketAnomaly=13, DeliveryAnomaly=15, TemperatureAnomaly=21,
			VoltageAnomaly=23, CurrentAnomaly=27, PressureAnomaly=29,
			TimeDistributionAnomaly=31, InternalSelfTestAnomaly=37,
			ApplicationLevelAnomaly=39, UserDefinedAnomaly=41
		};
		enum XtedsReplyStatus
		{
			Valid, XtedsCancel, AddressNotFound, AddressInvalid,
			SidNotFound, SidInvalid, DeviceNameNotFound, DeviceNameInvalid
		};
		enum op
		{
			Transport=0x41,
			XtedsRequest, XtedsReply,
			RequestCasRoute, ReplyCasRouteUnknown,
			Subscription, SubscriptionReply,
			RegistrationInfoRequest, RegistrationInfoReply,
			RequestAddressBlock=0x4C, AssignAddressBlock,
			VariableInfoRequest, VariableInfoReply,
			NetworkStatusRequest=0x70, NetworkStatusReply,
			DistributeRoute, RequestLsProbe,
			Data, ServiceRequest, ServiceReply,
			TimeAtTone,
			ProbeRequest, ProbeReply,
			Command,
			QueryRequest, QueryReply
		};

		protected Address mLocalManagerAddress;		///< Address of the local manager.
		protected CoreAddress mRegistrar;			///< Active registrar.
		protected MessageHandler mMessageHandler;	///< The Message_Handler context.
		protected bool mLocalManagerIsPresent;		///< Detection state of the local manager

		Address mScratchAddress;
		LocalMessage
			mLocalMessage = new LocalMessage(),
			mLocalParseMessage = new LocalMessage();
		LocalState mLocalState;
		Message mParseMessage, mSendMessage;
		MonarchRegistrationMgr mRegistration;	///< The RegistrationMgr context
		RoutingTable mRoutingTable = new RoutingTable();
		MarshaledString mXtedsString;
		Uuid
			mCompId,
			mXtedsId;
		uint mLogicalAddress;
		bool mEcho, mSmlRouteReceived;

		public MonarchProtocol(IApplication app, Transport transport, MarshaledString xtedsString, Uuid compId) :
			base(app, transport, ProtocolId.Monarch)
		{
			Debug.SetLevel(Debug.Level.Warning);
			mEcho = true;
			mLocalState = LocalState.None;

			mLocalManagerAddress = ProtocolFactory.CreateAddress(Id);
			mLocalManagerAddress.Set(Address.Type.LowLevelLocal,SM_L_PORT);
			mMessageHandler = app.MessageHandler;
			mRegistration = new MonarchRegistrationMgr(xtedsString,mMessageHandler,compId);

			mParseMessage = ProtocolFactory.CreateMessage(Id);
			mParseMessage.Version = (byte)Id;  // FIXME
			mSendMessage = ProtocolFactory.CreateMessage(Id);
			mSendMessage.Version = (byte)Id;  // FIXME
			mSendMessage.Source = transport.ListenAddress;

			mScratchAddress = ProtocolFactory.CreateAddress(ProtocolId.Monarch);
			mLocalMessage.Initialize(transport.ListenAddress,transport);

			mXtedsString = xtedsString;
			mCompId = compId;
			mXtedsId = Uuid.NewUuid();
		}

		public override ControlProtocol Clone()
		{
			var cp = new MonarchProtocol(mApplication, mTransport, mXtedsString, mCompId);
			return cp;
		}

		public ControlProtocol ContactLocalManager()
		{
			if ( !mLocalManagerIsPresent )
			{
			Again:
				switch ( mLocalState )
				{
				case LocalState.None:
					Debug.Printf(Debug.Level.Info, "Monarch Uid {0}\n", mCompId.ToString());
					SetState(LocalState.Discovery);
					goto Again;
				case LocalState.Discovery:
					SendLocalHello(mLocalManagerAddress, mCompId, MonarchTypes.ComponentType.Other);
					break;
				}
			}
			return null;
		}

		public override IPublisher FindMessage(MessageId messageId)
		{
			return null;
		}

		public void GenerateXtedsUid(string xtedsText, Uuid uid)
		{
			byte[] sha1 = new byte[20];
			MD5.Generate(xtedsText, sha1); // someday, switch to SHA-1
			uid.SetFromSha1(sha1);
		}

		public override bool IsOperational
		{
			get
			{
				return false;
					//mPmStateMachine.State == PmState.Operational &&
					//mDirectoryStateMachine.State == DirectoryState.Operational;
			}
		}

		public override Address LocalManagerAddress { get { return mLocalManagerAddress; } }

		//public override int MessageSize { get { return mSendMessage.Size; } }

		void OnDistributeRoute(Message message, uint compAddress, Uuid compUid, MonarchTypes.ComponentType compType)
		{
			if ( mEcho ) Debug.Printf(Debug.Level.Debug,"Processing MonarchDistributeRoute");
			Debug.Printf(Debug.Level.Trace,"I talk to {0} to talk to {1}",message.Source.Hash,compAddress);
			mScratchAddress.Set(Address.Type.Logical,message.Source.Hash);
			Address addr = mRoutingTable.Add(compAddress,compUid,mScratchAddress,compType);
			
			if (compType == MonarchTypes.ComponentType.LS)
			{
				//SetRegistrarAddress(addr);
				Debug.Printf(Debug.Level.Debug,"Received Lookup Service address: {0}",addr.ToString());
				SetState(LocalState.LookupSvcProbeWait);
			}
		}

		void OnLocalAck(LocalMessage message, Ack ack)
		{
			if ( mEcho ) Debug.Printf(Debug.Level.Debug,"LocalAck({0}) received",ack);
			if ( ack != Ack.Ok ) return;
			
			SetState(LocalState.LogicalAddressWait);
			if ( !mLocalManagerIsPresent )
				mLocalManagerIsPresent = true;
		}

		void OnLocalAssignAddress(LocalMessage message, uint address)
		{
			mLogicalAddress = address;
			mSendMessage.Source.Set(Address.Type.Local,address);
			//mSendMessage2.Source.Set(Address.Type.Local,address);
			long addr;
			int port = OwnAddress.GetAddressPort(out addr);
			mScratchAddress.Set(Address.Type.Self,(uint)port);
			mRoutingTable.Add(address, mCompId, mScratchAddress,
				MonarchTypes.ComponentType.Other);
			if ( mEcho ) Debug.Printf(Debug.Level.Debug,"Assigned logical address {0}",address);
			
			SetState(LocalState.RouteInfoWait);
		}

		void OnLocalRoute(LocalMessage message, uint address, ushort port,
			Uuid compUid, MonarchTypes.ComponentType type, bool ackRequired)
		{
			if ( port == 0 ) return;
			mScratchAddress.Set(Address.Type.Local,port);
			mRoutingTable.Add(address,compUid,mScratchAddress,type);
			
			if ( ackRequired )
			{
				mScratchAddress.Set(Address.Type.LowLevelLocal,message.SourcePort);
				SendLocalAck(mScratchAddress,Ack.InitialRouteReceived);
			}

			if ( type == MonarchTypes.ComponentType.SML )
			{
				Debug.Printf(Debug.Level.Debug,"Received resident SM-L address: {0}.{1}",address,port);
				mSmlRouteReceived = true;
				SetState(LocalState.LookupSvcProbeWait);
			}
		}

		void OnProbeRequest(Message message, ushort dialog, ushort replyCount, ushort replyPeriod)
		{
			if ( mEcho ) Debug.Printf(Debug.Level.Debug,"Processing ProbeRequest({0},{1},{2})", dialog,replyCount,replyPeriod);
			SendProbeReply(message.Source, dialog, 0, mCompId,
				mRegistration.XtedsId,0,OperatingMode.FullyOperational);
			SetState(LocalState.Operational);
		}

		void OnProbeReply(Message message, uint Uptime, MarshaledBuffer CompId, MarshaledBuffer XtedsId, uint fault, MarshaledString capabilities)
		{
			if ( mEcho )
			{
			//	Uuid compId(CompID.bytes,16);
			//	Uuid xtedsId(XtedsID.bytes,16);
			//	char buf1[40], buf2[40];
			//	printf("ProbeReply(%d,%s,%s,%d,%s)\n", Uptime, compId.ToString(buf1,sizeof(buf1)),
			//		xtedsId.ToString(buf1,sizeof(buf2)), Fault, Capabilities.chars);
				Debug.Printf(Debug.Level.Debug,"Processing ProbeReply");
			}
		}

		void OnXtedsRequest(Message message, ushort dialog, ushort requestType, uint componentAddress, Uuid compUid)
		{
			if ( mEcho ) Debug.Printf(Debug.Level.Debug,"Processing XtedsRequest");
			SendXtedsReply(message.Source, dialog, XtedsReplyStatus.Valid, componentAddress,
				mRegistration.XtedsId, mRegistration.XtedsString );
		}

		byte[] swapBuf = new byte[8];

		public override bool Parse(byte[] buffer, int len, Message parseMessage)
		{
			MonarchTypes.ComponentType componentType;
			if (parseMessage==null) parseMessage = mParseMessage;
			
			if (buffer[0] == (byte)ProtocolId.Monarch)
			{
				int offset = parseMessage.Unmarshal(buffer, 0, len);
				//Ack ack;

				switch ((op)parseMessage.Selector)
				{
					//case op.AssignAddressBlock: break;
					//case op.Command:
					//    break;
					//case op.Data:
					//    break;
					case op.DistributeRoute:
						componentType = (MonarchTypes.ComponentType)buffer[offset+20];
						OnDistributeRoute(parseMessage, GetNetwork.UINT(buffer, offset, swapBuf), new Uuid(buffer,offset+4), componentType);
						break;
					//case op.NetworkStatusRequest: break;
					//case op.NetworkStatusReply:
					//    break;
					case op.ProbeRequest:
						OnProbeRequest(parseMessage, GetNetwork.USHORT(buffer, offset), GetNetwork.USHORT(buffer, offset+4),
							GetNetwork.USHORT(buffer, offset+6));
						break;
					case op.ProbeReply: break;
					//case op.RegistrationInfoRequest:
					//	break;
					//case op.RegistrationInfoReply:
					//	break;
					//case op.RequestAddressBlock: break;
					//case op.RequestLsProbe: break;
					//case op.RequestCasRoute:
					//	break;
					//case op.ReplyCasRouteUnknown:
					//	break;
					//case op.ServiceRequest:
					//	break;
					//case op.ServiceReply:
					//	break;
					//case op.Subscription:
					//	break;
					//case op.SubscriptionReply:
					//	break;
					//case op.TimeAtTone:
					//	break;
					//case op.Transport: break;
					//case op.VariableInfoRequest:
					//	break;
					//case op.VariableInfoReply:
					//	break;
					case op.XtedsRequest:
						OnXtedsRequest(parseMessage, GetNetwork.USHORT(buffer, offset), GetNetwork.USHORT(buffer, offset+2),
							GetNetwork.UINT(buffer, offset+4, swapBuf), new Uuid(buffer, offset+8));
						break;
					//case op.XtedsReply:
					//    ptr.b += mParseBuffer1.Unmarshal(ptr.b);
					//    mParseString.Unmarshal(ptr.b);
					//    OnGetxTEDSReply(*parseMessage,mParseBuffer1,mParseString);
					//	break;
					case 0:
						break;
					default:
						MsgConsole.WriteLine("Bogus Monarch msg id {0}\n", parseMessage.Selector);
						return false;
				}
			}
			else
			{
				Ack ack;

				int offset = mLocalParseMessage.Unmarshal(buffer, len);
				switch ( mLocalParseMessage.OpCode )
				{
				case LocalMessage.OpCodeType.Ack:
						if (buffer[offset] < (byte)Ack.Last) ack = (Ack)buffer[offset];
						else ack = Ack.Error;
					OnLocalAck(mLocalParseMessage,ack);
					break;
				case LocalMessage.OpCodeType.AssignAddress:
					OnLocalAssignAddress(mLocalParseMessage,GetNetwork.UINT(buffer,offset,swapBuf));
					break;
				case LocalMessage.OpCodeType.Hello:
					Debug.Printf(Debug.Level.Debug, "rcv local Hello");
					break;
				case LocalMessage.OpCodeType.Route:
					componentType = (MonarchTypes.ComponentType)buffer[offset+22];
					OnLocalRoute(mLocalParseMessage,GetNetwork.UINT(buffer,offset,swapBuf),GetNetwork.USHORT(buffer,offset+4),new Uuid(buffer,offset+6),
						componentType,buffer[offset+23] != 0);
					break;
				case LocalMessage.OpCodeType.RouteRequest:
					Debug.Printf(Debug.Level.Debug,"rcv local RouteRequest");
					break;
				case 0:
					break;
				default:
					MsgConsole.WriteLine("Bogus Monarch local msg id {0}\n", mLocalParseMessage.OpCode);
					return false;
				}
			}
			return true;
		}

		internal override bool Query(Query query)
		{
			return base.Query(query);
		}

		//public override void Register(DomainScope scope)
		public void Register(DomainScope scope)
		{
			while (mLocalState != LocalState.Operational)
				mMessageHandler.Poll(1);
		}

		public override int RegisterQueryClient(IQueryClient client)
		{
			return 0;
		}

		int Send(int msgSelector, byte[] buffer, int payload, int payloadLength, Address destination, int sequence)
		{
			Address agentAddress = mRoutingTable.RoutedAddress(destination);
			if (agentAddress.AddressType == Address.Type.Local &&
				agentAddress.Hash == SM_L_PORT &&
				(destination.Hash & 0xffff) != 0)
				SendLocalRouteRequest(mLocalManagerAddress, destination);
			if (payloadLength < segmentLength)
				return base.SendVia(agentAddress, mSendMessage, msgSelector, buffer, payload, payloadLength, destination, sequence);
			else
				return 0;
		}

		void SendLocalAck(Address dest, Ack ack)
		{
			int payload = LocalMessage.HeaderSize;
			byte[] buffer = new byte[LocalMessage.HeaderSize+1];
			buffer[payload] = (byte)ack;
			if ( mEcho ) Debug.Printf(Debug.Level.Debug,"Sending LocalAck");
			mLocalMessage.SendTo(dest.Hash,LocalMessage.OpCodeType.Ack,buffer,1);
		}

		void SendLocalHello(Address dest, Uuid compId, MonarchTypes.ComponentType componentType)
		{
			int payload = LocalMessage.HeaderSize, offset = payload;
			byte[] buffer = new byte[LocalMessage.HeaderSize+17];
			offset += compId.Marshal(buffer,offset);
			buffer[offset] = (byte)componentType;
			if ( mEcho ) Debug.Printf(Debug.Level.Debug,"Sending LocalHello");
			mLocalMessage.SendTo(dest.Hash, LocalMessage.OpCodeType.Hello, buffer, offset+1-payload);
		}

		public void SendLocalRouteRequest(Address dest,Address address)
		{
			int payload = LocalMessage.HeaderSize;
			byte[] buffer = new byte[LocalMessage.HeaderSize+4];
			PutNetwork.UINT(buffer, payload, address.Hash);
			if ( mEcho ) Debug.Printf(Debug.Level.Trace,"No route found, sending to SM-L and requesting route to address from SM-L\n");
			mLocalMessage.SendTo(dest.Hash, LocalMessage.OpCodeType.RouteRequest, buffer, 4);
		}

		public void SendNoOp(Address destination)
		{
			byte[] buffer = new byte[MaxHeaderSize];
			Send(0,buffer,MaxHeaderSize,0,destination,0);
		}

		void SendProbeRequest(Address destination, ushort dialog, ushort replyCount, ushort replyPeriod)
		{
			int payload = mSendMessage.Size+3;
			byte[] buffer = new byte[4+mSendMessage.Size+8+2];
			PutNetwork.USHORT(buffer, payload, dialog);
			PutNetwork.USHORT(buffer, payload+4, replyCount);
			PutNetwork.USHORT(buffer, payload+6, replyPeriod);
			Send((int)op.ProbeRequest,buffer,payload,8,destination,0);
		}

		void SendProbeReply(Address destination, ushort dialog, uint uptime, Uuid compUid,
			Uuid xtedsUid, uint fault, OperatingMode mode)
		{
			int payload = mSendMessage.Size+3, offset = payload;
			byte[] buffer = new byte[4+mSendMessage.Size+45+2];
			PutNetwork.USHORT(buffer, offset, dialog);
			PutNetwork.USHORT(buffer, offset+2, 0);
			PutNetwork.UINT(buffer, offset+4, uptime);
			offset += compUid.Marshal(buffer,offset+8)+8;
			offset += xtedsUid.Marshal(buffer, offset);
			PutNetwork.UINT(buffer, offset, fault);
			buffer[offset] = (byte)mode;
			if ( mEcho ) Debug.Printf(Debug.Level.Debug,"Sending ProbeReply()");
			Send((int)op.ProbeReply,buffer,payload,offset+5-payload,destination,0);
		}

		public override void SendSubscriptionCancel(Address destination, MessageId messageId, Address client=null, int dialog=0)
		{
		}

		public override bool SendSubscriptionRequest(Address destination, MessageId messageId,
			byte Ith, byte Priority, int requestedLeasePeriod, Address client, bool resumeCanceled)
		{
			return false;
		}

		public override void SendSubscriptionRequestReply(Address destination, bool ack,
			int grantedLeasePeriod, MessageId messageId, Address client)
		{
			//SendSubscriptionRequestReply(destination,
			//	ack ? NativeProtocol.Ack.OK : NativeProtocol.Ack.ERROR,
			//	grantedLeasePeriod, messageId, client);
		}

		public override void SendTimeAtTheTone(Address destination, UInt32 toneSec, UInt32 toneSubSec, float timeRatio)
		{
			byte[] buffer = new byte[256];
			int payload = mSendMessage.Size, offset = payload;
			PutNetwork.UINT(buffer,payload,toneSec);
			PutNetwork.UINT(buffer,payload+4,toneSubSec);
			Send((int)op.TimeAtTone,buffer,payload,8,destination,0);
		}

		void SendXtedsReply(Address destination, ushort dialog, XtedsReplyStatus status, uint componentAddress, Uuid xtedsUid, MarshaledString xtedsString)
		{
			int payload = mSendMessage.Size+3, offset = payload;
			byte[] buffer = new byte[64+KBytes];
			PutNetwork.USHORT(buffer, offset, dialog);
			PutNetwork.USHORT(buffer, offset+2, (ushort)status);
			PutNetwork.UINT(buffer, offset+4, componentAddress);
			offset += xtedsUid.Marshal(buffer, offset+8)+8;
			Buffer.BlockCopy(xtedsString.Chars, 0, buffer, offset, xtedsString.Length);
			if (mEcho) Debug.Printf(Debug.Level.Debug, "Sending XtedsReply()");
			Send((int)op.XtedsReply,buffer,payload,offset+xtedsString.Length-payload,destination,0);
		}

		void SetState(LocalState state)
		{
			//exit logic
			switch (mLocalState)
			{
				case LocalState.Discovery:
					if (state != LocalState.LogicalAddressWait) return;
					Debug.Printf(Debug.Level.Debug, "Finished Monarch-Local Discovery Process");
					break;
				case LocalState.LogicalAddressWait:
					if (state != LocalState.RouteInfoWait) return;
					//printf("Assigned logical address: %d\n" + mOwnAddress.Id());
					break;
				case LocalState.RouteInfoWait:
					if (state != LocalState.LookupSvcProbeWait) return;
					if (mRegistrar==null || !mSmlRouteReceived) return;
					Debug.Printf(Debug.Level.Debug, "SM-L route and Lookup Service address received");
					break;
				case LocalState.LookupSvcProbeWait:
					if (state != LocalState.Operational) return;
					Debug.Printf(Debug.Level.Debug, "ProbeReply sent to Lookup Service, Lookup Service will request xTEDS if needed");
					break;
			}

			mLocalState = state;

			//entry logic
			switch (mLocalState)
			{
				case LocalState.Discovery:
					Debug.Printf(Debug.Level.Debug, "Starting Monarch-Local Discovery Process");
					break;
				case LocalState.LogicalAddressWait:
					Debug.Printf(Debug.Level.Debug, "Waiting for logical address assignment");
					break;
				case LocalState.RouteInfoWait:
					Debug.Printf(Debug.Level.Debug, "Waiting for SM-L route info and Lookup Service address notification");
					break;
				case LocalState.LookupSvcProbeWait:
					Debug.Printf(Debug.Level.Debug, "Waiting for Lookup Service probe");
					break;
				case LocalState.Operational:
					Debug.Printf(Debug.Level.Debug, "Init finished, going operational");
					break;
			}
		}

		public void Shutdown()
		{
			mRegistration.Goodbye();
		}

		public override void Start()
		{
		}

		public override void Stop()
		{
		}
	}
}
