using System;
using System.Collections.Generic;
using System.Threading;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;

namespace Aspire.Core
{
	public class ProtocolMux
	{
		IApplicationLite mApplication;
		MessageHandler mMessageHandler;
		List<ControlProtocol> mControlProtocols = new List<ControlProtocol>();
		ProtocolId[] mProtocolIds;

		public ProtocolMux(IApplicationLite application)
		{
			mApplication = application;
			Protocol[] protocols = null;
			ControlProtocol controlProtocol;
			int n = ProtocolFactory.FindControlProtocols(ref protocols);
			for( int i=0; i<n; i++)
			{
				controlProtocol = protocols[i] as ControlProtocol;
				if (controlProtocol != null) mControlProtocols.Add(controlProtocol);
			}

			mMessageHandler = mApplication.MessageHandler;
			mProtocolIds = new ProtocolId[mControlProtocols.Count];
			ProtocolId id;
			for( int i=0; i<mControlProtocols.Count; i++)
			{
				controlProtocol = mControlProtocols[i];
				mProtocolIds[i] = id = (ProtocolId)controlProtocol.Id;
				//controlProtocol.Application = mApplication as IApplication;
				mMessageHandler.AddProtocol((ProtocolId)id, controlProtocol);
				//controlProtocol.AddressServer.DirectoryAdded += new EventHandler(AddressServer_DirectoryAdded);
			}
		}

		//public void CancelLocalManagerSubscriptions()
		//{
		//	foreach (var cp in mControlProtocols)
		//		cp.CancelLocalManagerSubscriptions();
		//}

		//public event EventHandler DirectoryAdded;

		//void AddressServer_DirectoryAdded(object sender, EventArgs e)
		//{
		//	if (DirectoryAdded != null)
		//		DirectoryAdded(sender, e);
		//}

		//~ProtocolMux()
		//{
		//	//if ( mControlProtocols.Length() > 0 )
		//	//    delete[] mProtocolIds;
		//}

		public ControlProtocol FindLocalManager(bool preloadedRegistrar)
		{
			//if ( mControlProtocols.Count == 0 ) return null;
			//bool done = false;

			//if (mStandaloneProtocol != null) return mStandaloneProtocol;

			//mMessageHandler.SetPollPeriod(1,0);

			//ControlProtocol controlProtocol;

			//while (!done)
			//{
			//	for( int i=0; i<mControlProtocols.Count; i++)
			//	{
			//		controlProtocol = (mMessageHandler.Protocols(mProtocolIds[i]) as ControlProtocol).ContactLocalManager();
			//		if ( preloadedRegistrar && controlProtocol!=null )
			//			return controlProtocol;
			//	}

			//	mMessageHandler.Poll(0,100000,true);

			//	for( int i=0; i<mControlProtocols.Count; i++)
			//	{
			//		controlProtocol = mMessageHandler.Protocols(mProtocolIds[i]) as ControlProtocol;
			//		if (controlProtocol.LocalManagerIsPresent)
			//			return controlProtocol;
			//	}

			//	Thread.Sleep(1000);
			//}
			return null;
		}

		//public void PreloadRegistrar(Address address, string domainName)
		//{
		//	for (int i=0; i<mControlProtocols.Count; i++)
		//	{
		//		var cp = mMessageHandler.Protocols(mProtocolIds[i]) as ControlProtocol;
		//		cp.AddressServer.PreloadRegistrar(address, domainName);
		//	}
		//}

		public ControlProtocol StandaloneProtocol
		{
			set
			{
				mStandaloneProtocol = value;
			}
		} ControlProtocol mStandaloneProtocol;
	}
}
