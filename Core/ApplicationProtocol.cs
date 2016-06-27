using System;
using System.Collections.Generic;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;
using Aspire.Utilities;

namespace Aspire.Core
{
	public class ApplicationProtocol : NativeProtocol, IBlueprint,
		IDirectoryStateMachineListener, IPmStateMachineListener
	{
		static int MaxClientQueryId = 254;
		static int queryId = MaxClientQueryId;

		Address mPongDestination, mPreviousAddress;
		Address2 scratchAddress = new Address2();
		CoreAddress mLocalManager;
		DirectoryStateMachine mDirectoryStateMachine;
		List<CoreAddress> mPendingDirectories = new List<CoreAddress>();
		List<Query> mQueries = new List<Query>();
		List<IQueryClient> queryClients = new List<IQueryClient>();
		PmStateMachine mPmStateMachine;
		QueryMgr mQueryMgr;
		Reply mHelloReply;
		StructuredResponse mResponse;

		bool mLogIncomingMsgs, mLogOutgoingMsgs, mResumed;
		Address mLogMsgDestination;

		byte[] mPongBuf;
		byte[] queryBuffer = new byte[8 * 1024];

		public static void Blueprint()
		{
			new ProtocolBlueprint("Aspire", ProtocolId.Aspire, typeof(ApplicationProtocol));
		}

		//from ControlProtocol
		//public event EventHandler RegistrarChanged;
		//public abstract bool Resumed();

		public ApplicationProtocol(IApplication application, Transport transport,
			MarshaledString xtedsString, Uuid compId, QueryMgr queryMgr=null) :
			base(application, transport, xtedsString, compId)
		{
			mResponse = new StructuredResponse();
			mQueryMgr = queryMgr;

			mLocalManagerAddress = ProtocolFactory.CreateAddress(ProtocolId.Aspire);
			mLocalManagerAddress.Parse("127.0.0.1:2000");

			// Note: not using the Ports used in C++
			mDirectoryStateMachine = new DirectoryStateMachine(this);
			mPmStateMachine = new PmStateMachine(this);
		}

		bool CancelSubscription(IApplication application, Message message,
			MessageId messageId, Address client=null)
		{
			IPublisher publisher;
			//object subscriptionContext;
			if (message.Sequence == 255)
				publisher = FindMessage(messageId);
			else
				publisher = application.IXteds.FindMessage(messageId.Hash()) as xTEDS.IPublisher;

			if (publisher != null)
			{
				Publication pub = publisher.Publication;
				Address recipient = client != null ? client : message.Source;
				Subscription sub;
				//lock pub
				if (pub != null)
					if (pub.Cancel(recipient, out sub) == 0)
					{
						publisher.RaiseSubscription(sub, true);
						application.QueuePublisher(publisher, true);
					}
				//unlock pub
				return true;
			}
			Logger.Log(1,"Cancel: can't find message {0} in {1}\n", messageId.ToString(), application.IXteds.XtedsComponent.Name);
			return false;
		}

		public override ControlProtocol Clone()
		{
			var cp = new ApplicationProtocol(mApplication, mTransport, mXtedsString, mCompUid, mQueryMgr);
			return cp;
		}

		private void DispatchQueries(CoreAddress directory, bool registered)
		{
			foreach (var query in mQueries)
				if (query.Matches(directory, registered))
					SendQuery(query, directory);
		}

		public override bool IsOperational
		{
			get
			{
				return
					mPmStateMachine.State == PmState.Operational &&
					mDirectoryStateMachine.State == DirectoryState.Operational;
			}
			// Someday, set from events
		}

		void OnChangePublisherState(IPublisher publisher, bool active)
		{
			Publication pub = publisher.Publication;
			Logger.Log(1, "provider {0} of {1} {2}",pub.Provider.ToString(),
				pub.MessageName,active?"active":"dormant");
			if ( active )
				SendSubscriptionRequest(pub.Provider,pub.MessageId,1,0,0);
			else
				pub.CancelLeaseRenewal();
		}

		protected override void OnCoreComponentListEntry(Message message, Mode Mode,
			CoreComponentType Type, Address address, ScopeEnum Scope, string Domain)
		{
			//base.OnCoreComponentListEntry(message, Mode, Type, address, Scope, Domain);
			if ( Mode == Mode.ADD )
			{
				var ca = AddressServer.AddAddress(TypeAsString(Type), address, ScopeAsString(Scope), Domain);
				if (ca.Tag != null)
				{
					RegistrarState rs = ca.Tag as RegistrarState;
					rs.State = RegistrarState.Values.JustAdded;
				}
				else
					ca.Tag = new RegistrarState();

				switch ( Type )
				{
					case CoreComponentType.DIRECTORY:
						FindDirectory();
						OnDirectoryAdded(ca);
						break;
					case CoreComponentType.PROCESSOR_MANAGER:
						if ( Scope == ScopeEnum.Local )
							mLocalManager = ca;
						break;
				}
			}
			else if ( Mode == Mode.REMOVE )
			{
				AddressServer.RemoveAddress(TypeAsString(Type), address);
				switch(Type)
				{
					case CoreComponentType.DIRECTORY:
						if (mRegistrar != null && address.EqualsNetwork(mRegistrar.Address))
						{
							mDirectoryStateMachine.DirectoryRemoved();
							mRegistrar = null;
						}
						break;
					case CoreComponentType.PROCESSOR_MANAGER:
						if (Scope == ScopeEnum.Local)
						{
							mPmStateMachine.PmRemoved();
							mDirectoryStateMachine.PmRemoved();
							mLocalManager = null;
						}
					break;
				}
			}
		}

		protected void OnDirectoryAdded(CoreAddress directory)
		{
			Logger.Log(4,"{0}: on directory {1} ({2:X}) added\n",
				mAppLite.Name,directory.DomainName,(directory.Address.Hash&0xFF00)>>8);

			if (!IsOperational)
				mPendingDirectories.Add(directory);
			else
				DispatchQueries(directory, false);
		}

		public void OnDirectoryRegistered(CoreAddress directory)
		{
			Logger.Log(4, "{0}: on directory {1} registered\n",
				mAppLite.Name, directory.DomainName);

			DispatchQueries(directory, true);

			foreach (var dir in mPendingDirectories)
				if (dir != directory)
					DispatchQueries(dir, false);
			mPendingDirectories.Clear();
		}

		/// <summary>
		/// This app is a resumption of an earlier instance of itself as determined by the Directory.
		/// Remove the previous subscription to PM's CoreComponentList and let the Application know.
		/// </summary>
		void OnDirectoryResumed()
		{
			mResumed = true;

			// Cancel the subscription held by the previous instance of this app
			CancelCoreComponentListEntry(mLocalManagerAddress, mPreviousAddress);

			// Let the application know
			mApplication.Resume();
		}

		protected override void OnGetProperty(Message message, ProcessProperty property)
		{
			int value = 0;
			string strValue = string.Empty;
			ProcessResult result = ProcessResult.OK;

			switch ( property )
			{
			case ProcessProperty.CompUid:
				strValue = mApplication.CompUid.ToString();
				break;
			case ProcessProperty.LogIncomingMsgs:
				value = mLogIncomingMsgs ? 1 : 0;
				break;
			case ProcessProperty.LogOutgoingMsgs:
				value = mLogOutgoingMsgs ? 1 : 0;
				break;
			case ProcessProperty.MsgLogAddress:
				strValue = mLogMsgDestination.ToString();
				break;
			case ProcessProperty.Pid:
				value = System.Diagnostics.Process.GetCurrentProcess().Id;
				break;
			case ProcessProperty.XtedsUid:
				strValue = mApplication.XtedsUid.ToString();
				break;
			default:
				result = ProcessResult.Unsupported;
				break;
			}
			SendGetPropertyReply(message.Source,result,property,value,strValue);
		}

		protected override void OnGetxTEDS(Message message)
		{
			mDirectoryStateMachine.GetXtedsReceived();

			SecTime et = new SecTime();
			Clock.GetTime(ref et);
			et -= mRegistrationTime;
			Logger.Log(2, "{0}ms\n\n", et.ToMilliSeconds);
		}

		protected override void OnGoodbyeAck(Message message, Reply ReplyStatus)
		{
			base.OnGoodbyeAck(message, ReplyStatus);
		}

		protected override void OnHelloAck(Message message, Reply ReplyStatus, Address previousAddress=null)
		{
			base.OnHelloAck(message,ReplyStatus,previousAddress);

			mHelloReply = ReplyStatus;
			bool affirm;
			switch ( ReplyStatus )
			{
			case Reply.RESUMED:
        if ( previousAddress != null)
				  mPreviousAddress = previousAddress.Clone();
				affirm = true;
				break;
			case Reply.OK:
				affirm = true;
				break;
			case Reply.InProcess:
				Logger.Log(1,"-{0}.Hello in-process", mApplication.Name);
				affirm = false;
				return;
			default:
				affirm = false;
				break;
			}
			UInt32 myAssignedId = message.Destination.Hash;
			if (affirm)
			{
				SetAssignedId(myAssignedId);
				mDirectoryStateMachine.HelloAckReceived(ReplyStatus==Reply.RESUMED);
			}
			if ( ReplyStatus == Reply.OK )
				Logger.Log(2, " Aspire ID {0}; ",myAssignedId);
			else
			{
				SecTime et = new SecTime();
				Clock.GetTime(ref et);
				et -= mRegistrationTime;
				Logger.Log(2, " Aspire ID, {0}, {1}ms\n\n", myAssignedId, et.ToMilliSeconds);
			}
		}

		protected override void OnPing(Message message)
		{
			if ( mPongBuf == null )
			{
				mPongBuf = new byte[Message2.MarshalSize];
				mPongDestination = message.Source.Clone();
				mPongDestination.WithRespectTo(message.Source);
				message.Source = message.Destination;
				message.Destination = mPongDestination;
				message.Selector = (int)msg.Process_Pong;
				message.Marshal(mPongBuf,0,Message2.MarshalSize,0,null);
			}
			mPongBuf[Message2.iSequence] = (byte)message.Sequence;
			mTransport.Write(message.Buffer, 0, Message2.MarshalSize, mPongDestination);
		}

		protected virtual void OnPmStateChanged() { }

		// Receipt of Probe message
		protected override void OnProbe(Message message, UInt16 ReplyCount, UInt16 ReplyRate)
		{
			UInt32 Fault = 0;
			MarshaledString Capabilities = new MarshaledString(mApplication.Name);
			// Reply back to the originator
			SendProbeReply(message.Source,mApplication.ElapsedSeconds, CompUid, XtedsUid,
				Fault, Capabilities);
		}

		/// <summary>
		/// When the provider changed message arrives, it is the registrar notifying
		/// consumers that had positive returns to their queries whenever that provider
		/// found for the query has changed its run state.
		/// </summary>
		/// <param name="message">The provider changed notification message.</param>
		/// <param name="ProviderState">State of the provider application: Active or Dormant.</param>
		/// <param name="address">The current address of the provider.</param>
		/// <param name="prevAddress">The previous address of the provider if active.</param>
		protected override void OnProviderChanged(Message message, ProviderState ProviderState,
			Address address, Address prevAddress)
		{
			//base.OnProviderChanged(message, ProviderState, address);
			mXtedsProtocol.ChangeMessageProviders(ProviderState == ProviderState.Active,
				address, prevAddress, OnChangePublisherState);
			SendProviderChangedAck(message.Source);
		}

		void OnQueryAdded(Query query)
		{
			Logger.Log(4,"{0}: on query {1} added\n",mAppLite.Name,query.Id);

			CoreAddressList registrars = AddressServer.GetAddresses("DIRECTORY");
			for (int ix = 0; ix < registrars.Length; ix++)
			{
				CoreAddress directory = registrars[ix];
				bool registered =
					mRegistrar != null &&
					mDirectoryStateMachine.State == DirectoryState.Operational &&
					mRegistrar.Address.EqualsNetwork(directory.Address);
				if (query.Matches(directory, registered))
					SendQuery(query, directory);
			}
		}

		protected override void OnQueryReply(Message message, QueryType QueryType, MarshaledBuffer Specification)
		{
			base.OnQueryReply(message, QueryType, Specification);

			if (QueryType != QueryType.Structured) return;

			mResponse.Unmarshal(Specification.Bytes,Specification.Offset,Specification.Length);
			for(int resp=0; resp<mResponse.NumResponses; resp++)
			{
				if (mResponse[resp,0] is MarshaledBuffer)
				{
					Address provider = mResponse.ComponentAddress(resp);
					int qid = message.Sequence;
					XtedsMessage xmsg = mXtedsProtocol.FindMessage(provider, mResponse[resp, 0] as MarshaledBuffer,
						message.Source);
					if (xmsg != null)
					{
						xmsg.Provider = provider.Clone();
						xmsg.Header = null; // So it gets set on the first send in the appropriate thread
						int clientQid = MaxClientQueryId - qid;
						xmsg.Application = mApplication;
						Logger.Log(2,"{4}: Query {0} found message {1}.{2} provided by {3}",
								qid,xmsg.InterfaceName,xmsg.Name,xmsg.Provider.ToString(),mAppLite.Name);
						if (clientQid >= 0 && clientQid < queryClients.Count)
							queryClients[clientQid].OnQueryForMessage(qid,xmsg, mResponse, resp);
						else
							mApplication.OnQueryForMessage(qid, xmsg);
						xmsg.VerifyVariableMapping();
						var reply = xmsg.ReplyMessage;
						if (reply != null)
							reply.VerifyVariableMapping();
					}
					else
						Logger.Log(1, "Bad message spec in query response[{0}], {1}", qid, provider.ToString());
				}
				else
					mApplication.OnQueryFound(message.Sequence, resp, mResponse);
			}
			if (mResponse.Error != null)
				Logger.Log(1, "\n{0}", mResponse.Error);
		}

		protected override void OnSetProperty(Message message, ProcessProperty property, int value, string strValue)
		{
			ProcessResult result = ProcessResult.Unsupported;

			switch (property)
			{
				case ProcessProperty.LogIncomingMsgs:
					mLogIncomingMsgs = value == 1;
					result = ProcessResult.OK;
					break;
				case ProcessProperty.LogOutgoingMsgs:
					mLogOutgoingMsgs = value == 1;
					result = ProcessResult.OK;
					break;
				case ProcessProperty.MsgLogAddress:
					mLogMsgDestination = mSendMessage.Source.Clone();
					mLogMsgDestination.Parse(strValue);
					result = ProcessResult.OK;
					break;
			}
			SendSetPropertyReply(message.Source, result, property);
		}

		protected override void OnSubscriptionCancel(Message message,
			MessageId messageId, Address client)
		{
			base.OnSubscriptionCancel(message, messageId,client);

			Ack ack;
			if (CancelSubscription(mApplication, message, messageId, client))
				ack = Ack.OK;
			else
				ack = Ack.ERROR;
			SendSubscriptionCancelReply(message.Source, ack);
		}

		protected override void OnSubscriptionCancelReply(Message message, Ack AckStatus)
		{
			base.OnSubscriptionCancelReply(message, AckStatus);
		}

		protected override void OnSubscriptionRequest(Message message, MessageId messageId,
			byte Ith, byte Priority, int requestedLeasePeriod, Address client)
		{
			base.OnSubscriptionRequest(message, messageId, Ith, Priority, requestedLeasePeriod);

			SubscribeTo(mApplication, message, messageId, Ith, Priority, requestedLeasePeriod, client);
		}

		protected override void OnSubscriptionRequestReply(Message message, Ack AckStatus,
			int grantedLeasePeriod, MessageId messageId, Address client)
		{
			base.OnSubscriptionRequestReply(message, AckStatus, grantedLeasePeriod, messageId);

			long srcAddr=0, localAddr, ownAddr;
			int localPort = mLocalManagerAddress.GetAddressPort(out localAddr);
			if ( message.Source.GetAddressPort(out srcAddr) == localPort ||
				message.Source.EqualsNetwork(mLocalManagerAddress) )
			{
				if (message.Destination.GetAddressPort(out ownAddr) == localPort)
				{
					// We are PM
					uint id = mLocalManagerAddress.Hash;
					message.Source.Hash = id;
					SetAssignedId(id); // Set as if registered so registration is optional
				}
				OwnAddress.GetAddressPort(out ownAddr);
				// Test for locality.
				if (srcAddr == 0 || srcAddr == ownAddr)
				{
					mPmStateMachine.SubscriptionReplyRecieved();

					mLocalManagerAddress = message.Source;
				}
			}
			if (AckStatus != Ack.ERROR && mXtedsProtocol != null )
			{
				XtedsMessage xmsg = mXtedsProtocol.FindMessage(message.Source, messageId.Hash());
				if (xmsg != null)
				{
					//Create a Subscription under it and start the renew timer
					var dmsg = xmsg as IDataMessage;
					if (dmsg.Publication == null)
						dmsg.Publication = new Publication(dmsg as IPublisher,client);
					Publication pub = dmsg.Publication;
					if (grantedLeasePeriod > 0)
						pub.AutomaticallyRenewLease(grantedLeasePeriod, this);
					mApplication.OnSubscriptionReply(xmsg);//,grantedLeasePeriod);
				}
			}
			else if ( AckStatus == Ack.ERROR )
				Logger.Log(1,"{0}: Subscription request {1} reply error from {2}",
					mAppLite.Name, messageId,message.Source);
		}

		internal override bool Query(Query query)
		{
			mQueries.Add(query);
			OnQueryAdded(query);

			return true; // archaic
		}

		public override int RegisterQueryClient(IQueryClient client)
		{
			int qid = queryId--;
			queryClients.Insert(MaxClientQueryId-qid,client);
			return qid;
		}

		public override bool Resumed()
		{
			return mHelloReply == Reply.RESUMED;
		}

		void SendQuery(Query query, CoreAddress directory)
		{
			int len = mQueryMgr.Parse(query, queryBuffer);
			if (len > 0)
			{
				MarshaledBuffer spec = new MarshaledBuffer(len, queryBuffer, 0);
				SendQuery(query.Id, QueryType.Structured, spec, directory.Address);
			}
		}

		public override bool SendSubscriptionRequest(Address destination, MessageId messageId, byte Ith,
			byte Priority, int requestedLeasePeriod, Address client = null, bool resumeCanceled = false)
		{
			if ( mResumed && !resumeCanceled && mPreviousAddress != null)
			{
				Logger.Log(2, "{0}: Canceling previous sub to {1}'s {2} message from {3}.\n",
					mAppLite.Name,destination.ToString(), messageId.ToString(), mPreviousAddress.ToString());
				base.SendSubscriptionCancel(destination, messageId, mPreviousAddress);
				resumeCanceled = true;
			}
			base.SendSubscriptionRequest(destination, messageId,
					Ith, Priority, requestedLeasePeriod, client, resumeCanceled);

			return resumeCanceled;
		}

		void SetAssignedId(uint myAssignedId)
		{
			OwnAddress.Hash = myAssignedId;
			SourceAddress.Hash = myAssignedId;
			mXtedsProtocol.Hash = myAssignedId;
		}

		public override void Start()
		{
			mPmStateMachine.Start();
			mDirectoryStateMachine.Start();
		}

		public override void Stop()
		{
			mXtedsProtocol.CancelAllSubscriptions(this);
			mPmStateMachine.Stop();
			mDirectoryStateMachine.Stop();
		}

		int SubscribeTo(IApplication application, Message message, MessageId messageId, byte Ith,
			byte Priority, int requestedLeasePeriod, Address client=null)
		{
			IPublisher publisher;

			if ( message.Sequence == 255 )
				publisher = FindMessage(messageId);
			else
				publisher = application.IXteds.FindMessage(messageId.Hash()) as IPublisher;

			if (publisher != null)
			{
				Publication pub = publisher.Publication;
				if (pub == null)
					publisher.Publication = (pub = new Publication(publisher));
				int leasePeriod = requestedLeasePeriod;

				Address recipient = client != null ? client : message.Source;
				//lock pub
				Subscription newSub = pub.AddIfRequired(recipient, Ith, Priority, ref leasePeriod);

				SendSubscriptionRequestReply(message.Source,
					leasePeriod >= 0, leasePeriod, messageId, client);

				if (newSub != null && newSub.PreviousState != Subscription.OpState.Active)
				{
					if (publisher.IsProvider)
						application.QueuePublisher(publisher, false);
					//unlock pub
					//if (sub.PreviousState == Subscription.State.Initial)
					publisher.RaiseSubscription(newSub, false);
				}
				return leasePeriod;
			}
			else
			{
				SendSubscriptionRequestReply(message.Source, Ack.ERROR, -1, messageId, client);
				Logger.Log(1,"SubscribeTo: can't find message {0} in {1}",
					messageId.ToString(), application.IXteds.XtedsComponent.Name);
				return -1;
			}
		}

		#region IDirectoryStateMachineListener Members

		public void FindDirectory()
		{
			if (mRegistrar != null)
				return;

			string appDomainName = mApplication.DomainName;
			string scope = string.Empty;
			if (appDomainName != null && appDomainName == "*")
				appDomainName = string.Empty;

			var registrars = AddressServer.GetAddresses("DIRECTORY",appDomainName,scope);
			if (registrars.Length > 0)
			{
				mRegistrar = registrars[0]; // This may not always hold, especially on the WAN
				mDirectoryStateMachine.DirectoryAdded();
			}
		}

		public void OnStateChanged(DirectoryState state)
		{
			Log.WriteLine(2, "{1}: DIRECTORY STATE: {0}", state, mAppLite.Name);
			switch(state)
			{
			case DirectoryState.Broken:
				mDirectoryStateMachine.Start();
				break;
			case DirectoryState.Operational:
				mApplication.OnRegistrarChanged();
				OnDirectoryRegistered(mRegistrar);
				break;
			case DirectoryState.Resumed:
				OnDirectoryResumed();
				break;
			}
		}

		public void SendXteds()
		{
			SendGetxTEDSReply(mRegistrar.Address, XtedsUid, XtedsString );

			mApplication.OnXtedsDelivered();
		}

		public void SendGoodbye()
		{
			SendGoodbye(mRegistrar.Address, OwnAddress);
		}

		public void SendHello()
		{
			Log.WriteLine(1, "\nRegistering {0} with {1}\n",
				mApplication.Name, mRegistrar.ToString());
			SecTime now = new SecTime();
			Clock.GetTime(ref now);
			mRegistrationTime = now;

			SendHello( mRegistrar.Address, CompUid, XtedsUid);
		}

		#endregion

		#region IPmStateMachineListener Members

		public void OnStateChanged(PmState state)
		{
			Logger.Log(2, "{0} PM STATE: {1}", mAppLite.Name, state);
			mApplication.OnLocalManager(mLocalManager, state == PmState.Operational);
			OnPmStateChanged();
			switch (state)
			{
				case PmState.Broken:
					mPmStateMachine.Start();
					break;
			}
		}

		public void Subscribe()
		{
			SubscribeToCoreComponentListEntry(mLocalManagerAddress);
		}

		public void Unsubscribe()
		{
			UnsubscribeFromCoreComponentListEntry(mLocalManagerAddress);
		}

		#endregion
	}

	class RegistrarState
	{
		public enum Values { JustAdded, Hello, Registered };
		public RegistrarState()
		{
			State = Values.JustAdded;
		}
		
		public Values State;
	};

}
