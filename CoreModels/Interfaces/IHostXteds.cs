using System;
using System.ComponentModel;

using Aspire.Core.xTEDS;

namespace Aspire.CoreModels
{
	/// <summary>
	/// Just what do we expect from a host
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public interface IHostXteds : IHostXtedsLite
	{
		// Is there an adapter managing this host ?
		IAdapterState AdapterState { get; }

		/// <summary>
		/// Test to make sure the host can parse a test message
		/// </summary>
		bool CanParse { get; }

		void HookNotification(XtedsMessage xmsg);

		// Is there already a marshaler for this host
		VariableMarshaler KnownMarshaler(IVariable variable);

		/// <summary>
		/// A moniker for publishing
		/// </summary>
		string Name { get; }

		/// <summary>
		/// A DataMessage is used by the UI and needs to be subscribed to
		/// </summary>
		/// <param name="dataMessage"></param>
		void NeedSubscription(IDataMessage dataMessage);

		/// <summary>
		/// Parse an Aspire message
		/// </summary>
		void ParseAspireMessage(byte[] buf, int length);

		/// <summary>
		/// A moniker for logging
		/// </summary>
		string ProviderName { get; }

		/// <summary>
		/// Get maximum length allowed in published byte[]s.
		/// </summary>
		int PublishableByteArrayLength { get; }

		/// <summary>
		/// Publish an Aspire Notification message
		/// </summary>
		/// <param name="dataMsg"></param>
		void Publish(IDataMessage dataMsg);

		/// <summary>
		/// Send a Aspire message
		/// </summary>
		void Send(Xteds.Interface.Message cmdMsg);

		/// <summary>
		/// Get the SensorId associated with the host
		/// </summary>
		//uint SensorId { get; }

		/// <summary>
		/// Subscribe to a Aspire message
		/// </summary>
		void Subscribe(Xteds.Interface.DataMessage dataMsg);

		/// <summary>
		/// Is the message locally testable
		/// </summary>
		bool Testable { get; }

		/// <summary>
		/// Unmarshal a MarshaledBuffer into a message.
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="length"></param>
		/// <param name="msg"></param>
		void Unmarshal(byte[] buf, int length, Xteds.Interface.Message msg);

		/// <summary>
		/// Unsubscribe to an Aspire message
		/// </summary>
		void Unsubscribe(Xteds.Interface.DataMessage dataMsg);

		/// <summary>
		/// Get a Dictionary binding for a given Xteds variable
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		XtedsVariable XtedsVariable(string name);
	}
}
