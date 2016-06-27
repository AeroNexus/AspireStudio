using System;
using System.ComponentModel;

using Aspire.Framework;
using Aspire.Core;
using Aspire.Core.Messaging;
using Aspire.Core.xTEDS;

namespace Aspire.CoreModels
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public interface IEventMsg
	{
		void Set(Address localAddress, Address remoteAddress, int sequence);
	}
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public interface IQueuedMessages
	{
		void DequeueMsg(IAdapterState adapterState, IPublisher msg);
		void QueueMsg(IAdapterState adapterState, IPublisher msg);
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	public interface IAdapter : IKnownMarshaler
	{
		//Clock Clock { get; }
		void ChangeProperty(string propertyName);
		bool DoIo { get; set; }
		string DomainName { get; }
		long FrameCount { get; }
		//VariableMarshaler KnownMarshaler(IVariable ivariable);
		uint IpAddress { get; }
		bool IsVerifying { get; }
		int DefaultLeasePeriod { get; }
		MessageHandler MessageHandler { get; }
		string Name { get; }
		bool NotYetRegistering { get; }
		IQueuedMessages QueuedMessages { get; }
		int Parse(byte[] buf, int length);
		int PublishableByteArrayLength { get; }
		void Remove(int tag, Xteds.Interface.Message msg, IAdapterState adapterState);
		void StateHasRegistered();
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	public interface IAdapterState
	{
		IAdapter Adapter { get; set; }
		//Xteds.Interface.Message AddMessage(Message rMsg, Xteds.Interface.Message xMsg);
		//Asim Asim { get; }
		void Close();
		Address Address { get; }
		Blackboard.Item DevicePowerState { get; }
		bool DontRegister { get; set; }
		uint Id { get; }
		void LeaseHasExpired(Xteds.Interface.DataMessage message, Address subscriber);
		string Name { get; }
		IDataMessage OwnDataMessage(string interfaceName, string messageName);
		XtedsMessage OwnMessage(string interfaceName, string messageName);
		void Publish(IDataMessage message);
		void ReplyTo(XtedsMessage request);
		int Send(XtedsMessage message, Address destination);
		int Send(XtedsMessage message);
		XtedsMessage WhenMessageArrives(XtedsMessage message,XtedsMessage.Handler handler);
		void WhenSubscribedTo(IDataMessage message, SubscribedToHandler handler);
		Xteds Xteds { get; }
		string XtedsName { get; }
		XtedsProtocol XtedsProtocol { get; }
	}

}
