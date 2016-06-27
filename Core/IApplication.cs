//using System;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;

namespace Aspire.Core
{
	public enum DomainScope { Specifically, Locally, Remotely, All };

	public interface IApplicationLite
	{
		/// <summary>
		/// The execution period used to dispatch periodic processing
		/// </summary>
		SecTime ExecutionPeriod { get; }
		/// <summary>
		/// Is the application in the midst of closing
		/// </summary>
		bool IsClosing { get; }
		/// <summary>
		/// The MessageHandler
		/// </summary>
		MessageHandler MessageHandler { get; }
		/// <summary>
		/// The application name used in error messages
		/// </summary>
		string Name { get; }
		/// <summary>
		/// When a Directory is added/removed
		/// </summary>
		/// <param name="coreAddress"></param>
		/// <param name="added">true if Added, false if removed</param>
		void OnDirectory(CoreAddress coreAddress, bool added);
		/// <summary>
		/// When the LocalManager starts or shutsdown
		/// </summary>
		/// <param name="coreAddress"></param>
		/// <param name="up">true if LM started, false if shutdown</param>
		void OnLocalManager(CoreAddress coreAddress, bool up);
		/// <summary>
		/// The periodic processing method
		/// </summary>
		void Perform();
	}

	public interface IApplication : IApplicationLite
	{
		Uuid CompUid { get; }
		ControlProtocol ControlProtocol { get; set; }
		string DomainName { get; }
		uint ElapsedSeconds { get; }
		IKnownMarshaler IKnownMarshaler { get; }
		void OnQueryForMessage(int id, XtedsMessage msg);
		void OnQueryFound(int id, int responseId, StructuredResponse response);
		void OnRegistrarChanged();
		void OnSubscriptionReply(XtedsMessage xmsg);
		void OnXtedsDelivered();
		void QueuePublisher(IPublisher publisher, bool cancel);
		IXteds IXteds { get; }
		/// <summary>
		/// Resumption of earlier instance of this app
		/// </summary>
		void Resume();
		string XtedsText { get; }
		Uuid XtedsUid { get; }
	}
	public interface IQueryClient
	{
		void OnQueryForMessage(int queryId, XtedsMessage xmsg, StructuredResponse mResponse, int responseId);
	};
}
