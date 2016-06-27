using System;
using System.ComponentModel;
using System.Xml.Serialization;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;

namespace Aspire.Core
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public abstract class ControlProtocol : Protocol
	{
		public enum Status { Stopped, Operational, Broken };

		protected IApplication mApplication;
		protected XtedsProtocol mXtedsProtocol;

		public ControlProtocol(IApplication app, Transport transport, ProtocolId id) :
			base(id, transport)
		{
			mApplication = app;
			mXtedsProtocol = null;
		}

		public abstract ControlProtocol Clone();

		[DefaultValue(0)]
		[Description("Level of diagnostic printouts.")]
		[Category("Diagnostics")]
		[XmlIgnore]
		public virtual int DebugLevel
		{
			get { return mDebugLevel; }
			set { mDebugLevel = value; }
		} protected int mDebugLevel;

		public abstract IPublisher FindMessage(MessageId messageId);

		public abstract bool IsOperational { get; }

		public abstract Address LocalManagerAddress { get; }

		internal virtual bool Query(Query query) { return false; }

		public abstract int RegisterQueryClient(IQueryClient client);

		public abstract void SendSubscriptionCancel(Address destination, MessageId messageId,
			Address client=null, int dialog=0);
		public abstract bool SendSubscriptionRequest(Address destination, MessageId messageId,
			byte Ith, byte Priority, int requestedLeasePeriod,
			Address client=null, bool resumeCanceled=false);
		public abstract void SendSubscriptionRequestReply(Address destination, bool ack,
			int leasePeriod, MessageId messageId, Address client);
		public abstract void SendTimeAtTheTone(Address destination, UInt32 toneSec,
			UInt32 toneSubSec, float timeRatio);

		public abstract void Start();
		public abstract void Stop();

		public XtedsProtocol XtedsProtocol { set { mXtedsProtocol = value; } }

		public virtual Uuid XtedsUid { get; set; }
	}
}
