using Aspire.Core;
using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;

namespace Aspire.CoreModels
{
	class BrowserFactoryProtocol : NativeProtocol, IPmStateMachineListener
	{
		PmStateMachine mPmStateMachine;

		public BrowserFactoryProtocol(IApplicationLite appLite, Transport transport, MarshaledString xtedsString, Uuid compUid) :
			base(appLite, transport, xtedsString, compUid)
		{
			mPmStateMachine = new PmStateMachine(this);

			mLocalManagerAddress = ProtocolFactory.CreateAddress(ProtocolId.Aspire);
			mLocalManagerAddress.Parse("127.0.0.1:2000");
		}

		protected override void OnCoreComponentListEntry(Message message, Mode Mode,
			CoreComponentType Type, Address address, ScopeEnum Scope, string Domain)
		{
			base.OnCoreComponentListEntry(message, Mode, Type, address, Scope, Domain);
			if (Mode == Mode.ADD)
			{
				var ca = AddressServer.AddAddress(TypeAsString(Type), address, ScopeAsString(Scope), Domain);
				switch (Type)
				{
					case CoreComponentType.DIRECTORY:
						mAppLite.OnDirectory(ca,true);
						break;
					case CoreComponentType.PROCESSOR_MANAGER:
						if (ca.Scope == "Local")
							mAppLite.OnLocalManager(ca, true);
						break;
				}
			}
			else if (Mode == Mode.REMOVE)
			{
				var ca = AddressServer.FindCoreAddress(address);
				switch (Type)
				{
					case CoreComponentType.DIRECTORY:
						if (mRegistrar != null && address.EqualsNetwork(mRegistrar.Address))
						{
							mAppLite.OnDirectory(ca,false);
							mRegistrar = null;
						}
						break;
					case CoreComponentType.PROCESSOR_MANAGER:
						if (Scope == ScopeEnum.Local)
						{
							mPmStateMachine.PmRemoved();
							mAppLite.OnLocalManager(ca,false);
						}
						break;
				}
				AddressServer.RemoveAddress(TypeAsString(Type), address);
			}
		}

		protected override void OnSubscriptionRequestReply(Message message, Ack AckStatus,
			int grantedLeasePeriod, MessageId messageId, Address client)
		{
			base.OnSubscriptionRequestReply(message, AckStatus, grantedLeasePeriod, messageId);

			long srcAddr = 0, localAddr, ownAddr;
			int localPort = mLocalManagerAddress.GetAddressPort(out localAddr);
			if (message.Source.GetAddressPort(out srcAddr) == localPort ||
				message.Source.EqualsNetwork(mLocalManagerAddress))
			{
				//if (message.Destination.GetAddressPort(out ownAddr) == localPort)
				//{
				//	// We are PM
				//	uint id = mLocalManagerAddress.Hash;
				//	message.Source.Hash = id;
				//	SetAssignedId(id); // Set as if registered so registration is optional
				//}
				OwnAddress.GetAddressPort(out ownAddr);
				// Test for locality.
				if (srcAddr == 0 || srcAddr == ownAddr)
				{
					mPmStateMachine.SubscriptionReplyRecieved();
					mLocalManagerAddress = message.Source;
				}
			}
			if (AckStatus != Ack.ERROR && mXtedsProtocol != null)
			{
				XtedsMessage xmsg = mXtedsProtocol.FindMessage(message.Source, messageId.Hash());
				if (xmsg != null)
				{
					//Create a Subscription under it and start the renew timer
					var dmsg = xmsg as IDataMessage;
					if (dmsg.Publication == null)
						dmsg.Publication = new Publication(dmsg as IPublisher, client);
					Publication pub = dmsg.Publication;
					if (grantedLeasePeriod > 0)
						pub.AutomaticallyRenewLease(grantedLeasePeriod, this);
					mApplication.OnSubscriptionReply(xmsg);//,grantedLeasePeriod);
				}
			}
		}

		public override void Start()
		{
			mPmStateMachine.Start();
			//mDirectoryStateMachine.Start();
		}

		public override void Stop()
		{
			mPmStateMachine.Stop();
			//mDirectoryStateMachine.Stop();
		}


		#region IPmStateMachineListener Members

		public void OnStateChanged(PmState state)
		{
			switch (state)
			{
				case PmState.Broken:
					mPmStateMachine.Start();
					break;
				case PmState.Operational:
					break;
				case PmState.Shutdown:
					break;
				case PmState.Subscribing:
					break;
				case PmState.Unsubscribing:
					break;
			}
		}

		public void Subscribe()
		{
			base.SubscribeToCoreComponentListEntry(mLocalManagerAddress);
		}

		public void Unsubscribe()
		{
			base.UnsubscribeFromCoreComponentListEntry(mLocalManagerAddress);
		}

		#endregion
	}
}
