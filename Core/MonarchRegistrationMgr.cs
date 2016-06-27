using System;
using System.ComponentModel;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;

namespace Aspire.Core
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class MonarchRegistrationMgr
	{
		IApplication mApplication;
		CoreAddressList mRegistrars;
		CoreAddress mLocalRegistrar;
		MessageHandler mMessageHandler;
		ControlProtocol mControlProtocol;
		Timer mHelloTimer, mGoodbyeTimer;
		Uuid mCompId = new Uuid(), mXtedsId = new Uuid(0);
		bool mRegistered;
		string mXtedsText;

		public event EventHandler Registered;

		public MonarchRegistrationMgr(string xtedsText, MessageHandler messageHandler, Uuid compId)
		{
			XtedsText = xtedsText;
			mMessageHandler = messageHandler;
			//mControlProtocol = messageHandler.Protocols(ProtocolId.Aspire) as ControlProtocol;
			if ( compId.IsEmpty )
				mCompId = Uuid.NewUuid();
			else
				mCompId = compId;
			// Remove the trailing \0 by reducing length
		}

		public void CancelHelloTimer()
		{
			if (mHelloTimer != null)
			{
				mHelloTimer.Cancel();
				mHelloTimer = null;
			}
		}

		public Uuid CompId
		{
			get { return mCompId; }
			set { mCompId = value; }
		}

		void ReHello(){Hello();}

		public void Hello()
		{
			//if (mLocalRegistrar == null)
			//    return;
			if (mControlProtocol == null)
			{
				if (mApplication!=null)
					MsgConsole.WriteLine("{0} can't send Hello: controlProto=null", mApplication);
				return;
			}
			//timing.Print("Hello");
			if (mHelloTimer == null)
				mHelloTimer = Timer.Set(5, 0, Timer.Trigger.Periodic, ReHello);
			foreach (var reg in mRegistrars)
			{
				MsgConsole.WriteLine("Registering {0} with {1}", mApplication.Name, reg.ToString());
				//mControlProtocol.SendHello(reg.Address, mCompId, mXtedsId);
			}
			//mControlProtocol.SendHello(mLocalRegistrar.Address, mCompId, mXtedsId);
		}

		public void HelloAcknowledged(UInt32 assignedId)
		{
			//mMessageHandler.OwnAddressHash = assignedId;
			//timing.TimeStamp("Hello acked");
			//timing.Dump();

			CancelHelloTimer();

			mRegistered = true;
			if (Registered != null)
				Registered(this, EventArgs.Empty);
			//mMessageHandler.Break();
		}

		void ReGoodbye() { Goodbye(); }

		public void Goodbye()
		{
			CancelHelloTimer();
			//if ( mGoodbyeTimer == null )
			//	mGoodbyeTimer = Timer.Set(5,0, Timer.Trigger.Periodic, ReGoodbye);
			//if ( mRegistrars != null )
			//	for(int i=0; i<mRegistrars.Length; i++)
			//		mControlProtocol.SendGoodbye(mRegistrars[i].Address, mMessageHandler.OwnAddress);
		}

		public void GoodbyeAcknowledged(int ackStatus)
		{
			if (mGoodbyeTimer != null)
			{
				mGoodbyeTimer.Cancel();
				mGoodbyeTimer = null;
			}
		}

		public bool IsRegistered { get { return mRegistered; } }

		public void SetControlProtocol(IApplication application)
		{
			mControlProtocol = application.ControlProtocol;
			// Now that we know the control protocil, use it to generate the xteds UID
			//mControlProtocol.GenerateXtedsUid(mXtedsText, mXtedsId);
		}

		public CoreAddressList GetRegistrars(IApplication application)
		{
			return GetRegistrars(application, DomainScope.Locally, null);
		}
		public CoreAddressList GetRegistrars(IApplication application, DomainScope domainScope)
		{
			return GetRegistrars(application, domainScope, null);
		}
		public CoreAddressList GetRegistrars(IApplication application, DomainScope domainScope, string domainName)
		{
			mApplication = application;
			mControlProtocol = application.ControlProtocol;
			// Now that we know the control protocil, use it to generate the xteds UID
			//mControlProtocol.GenerateXtedsUid(mXtedsText, mXtedsId);

			string scope;
			switch (domainScope)
			{
				case DomainScope.Locally:
					scope = "Local";
					break;
				case DomainScope.Remotely:
					scope = "Remote";
					break;
				default:
					scope = null;
					break;
			}

			if (mRegistrars == null)
			{
				//mControlProtocol.AddressServer.DirectoryAdded += new EventHandler(OnNewDirectory);
				mRegistrars = new CoreAddressList();
			}

			do
			{
				var registrars = new CoreAddress[0];
					//mControlProtocol.AddressServer.GetAddresses("DIRECTORY", application.DomainName, scope);
				mRegistrars.Clear();

				// Scan the Registrars list and return the one that matches the Scope/Domain criteria
				// Policy is that an app should register with the Local Directory in its own Domain
				// We match on the most recent Local domain
				SecTime maxTime = new SecTime();
				string appDomainName = application.DomainName;
				for (int i=0; i<registrars.Length; i++)
				{
					var registrar = registrars[i];

					//printf("Directory @ %s, %s %s domain\n",registrar.getAddress().ToString(),
					//	registrar.Scope(),registrar.DomainName());

					if (scope != null && registrar.Scope != scope) continue;

					if (appDomainName != null) // Everyone except PM
					{
						if (registrar.DomainName != null) // Only if we have a non-NULL Domain Name
						{
							if (registrar.DomainName == appDomainName)
							{
								if (registrar.Scope == "Local")
									mLocalRegistrar = registrar;
								mRegistrars.Add(registrar);
							}
						}
					}
					else // PM
					{
						if (registrar.Time > maxTime)
						{
							mRegistrars.Add(registrar);
							mLocalRegistrar = registrar;
							maxTime = registrar.Time;
						}
					}
				}
				if (mRegistrars.Length > 0 )
					return mRegistrars;
				mMessageHandler.Poll(10);
			} while (mLocalRegistrar == null);

			return null;
		}

		void OnNewDirectory(object sender, EventArgs e)
		{
			//Migrate Registration here someday
			//var ca = sender as CoreAddress;
			//MsgConsole.WriteLine("Registration.OnNewDictionary:{0}",ca);
			//if (mLocalRegistrar == null)
			//	mMessageHandler.Break();
		}

		public void SetRegistrarAddress(Address address, ControlProtocol controlProtocol,string domainName)
		{
			if ( mControlProtocol == null )
				mControlProtocol = controlProtocol;

			mRegistrars = new CoreAddressList();
			// ASIMs only ever register with local Default domains
			var item = new CoreAddress(address, "DIRECTORY", "Local", domainName);
			mRegistrars.Add(item);
		}

		public MarshaledString XtedsString
		{
			get { return mXtedsString; }
			set
			{
				mXtedsString = value;
				//if ( mControlProtocol != null )
				//	mControlProtocol.GenerateXtedsUid(mXtedsText, mXtedsId);
			}
		} MarshaledString mXtedsString;

		public string XtedsText
		{
			get { return mXtedsText; }
			set
			{
				mXtedsText = value;
				mXtedsString = new MarshaledString(value);
				//if ( mControlProtocol != null )
				//	mControlProtocol.GenerateXtedsUid(mXtedsText, mXtedsId);
			}
		}

		public Uuid XtedsId
		{
			get { return mXtedsId; }
			set { mXtedsId = value; }
		}
	}
}
