//using System;

using Aspire.Core.Messaging;

namespace Aspire.Core.xTEDS
{
	public interface IDataMessage
	{
		int AllowableLeasePeriod { get; set; }
		MessageId MessageId { get; }
		string Name { get; }
		Address Provider { get; }
		string ProviderName { get; }
		Publication Publication { get; set; }
		bool Verified { get; set; }
		XtedsMessage XtedsMessage { get; }
	}
}
