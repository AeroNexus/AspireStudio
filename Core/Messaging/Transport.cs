using System.ComponentModel;

using Aspire.Core.Utilities;

namespace Aspire.Core.Messaging
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public abstract class Transport : ITransport, IReliableTransport
	{
		protected string mName;
		protected SecTime mReadTimeout, mWriteTimeout;

		public Transport(string name)
		{
			mName = name;
			mReadTimeout = new SecTime(1, 0);
			mWriteTimeout = new SecTime(SecTime.Infinite);
		}

		public Transport(string name, SecTime readTimeout, SecTime writeTimeout)
		{
			mName = name;
			mReadTimeout = readTimeout;
			mWriteTimeout = writeTimeout;
		}

		// Transport implementation

		public virtual int Read(byte[] buffer)
		{
			return Read(buffer, mReadTimeout);
		}

		public virtual int Write(byte[] buffer, int offset, int length, Address destination)
		{
			return Write(buffer, offset, length, destination, mWriteTimeout);
		}

		public virtual SecTime ReadTimeout
		{
			get { return mReadTimeout; }
			set { mReadTimeout = value; }
		}

		public virtual SecTime WriteTimeout
		{
			get { return mWriteTimeout; }
			set { mWriteTimeout = value; }
		}


		# region ITransport

		public abstract int Close();
		public abstract Address ListenAddress { get; }
		public abstract string ListenAddressString { get; }
		public abstract int Open();
		public virtual string Name { get { return mName; } }
		public abstract int Read(byte[] buffer, SecTime timeout);
		public abstract int Write(byte[] buffer, int offset, int length, Address destination, SecTime timeout);

		#endregion

		#region IReliableTransport
		/*
		 * These are the supported capabilities of a transport.  They live outside of
		 * ITransport because the functionallity is implemented outside of ITransport.
		 * The equivalent of these indicators is to use rtti to see if the transport
		 * implements the IReliableTransport interface for example (which doesn't exist yet).
		 * I'm not sure about this design but it's a step in the direction I want to see it go.
		 */
		public abstract bool SupportsReliableDelivery{get;}
		public abstract bool SupportsBestEffortDelivery{get;}
		public abstract bool SupportsBroadcast{get;}
		public abstract bool SupportsMulticast { get; }

		#endregion

		// Legacy
		public virtual int SendToLocal(uint destPort, byte[] buffer, int length)
		{
			return 0;
		}

	}
}
