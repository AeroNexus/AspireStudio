using System.Collections.Generic;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;
using Aspire.Framework;

namespace Aspire.CoreModels
{
	public interface IPnPBrowser : IPublishable
	{
		AspireComponent this[string name] { get; }
		XtedsCache Cache { get; }
		void CancelSubscription(IDataMessage message, Address provider);
		AspireComponent Component(uint hash);
		List<AspireComponent> Components { get; }
		string DomainName { get; }
		int PublishableByteArrayLength { get; set; }
		void RequestXteds(AspireComponent component);
		void ResendGetComponentInfo(AspireComponent component);
		Address RoutedAddress(Address logicalAddress);
		int Send(XtedsMessage message, Address destination,int tag=0);
		void Subscribe(IDataMessage message, Address provider);
		void Subscribe(string componentName, string interfaceName, string messageName);
		void Unload();
		void UseComponent(string componentName);
		void XtedsIsAvailable(AspireComponent component, string xtedsText);
	}


}
