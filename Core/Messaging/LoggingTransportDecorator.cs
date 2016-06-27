﻿using System;

using Aspire.Core.Utilities;

namespace Aspire.Core.Messaging
{
	public class LoggingTransportDecorator : TransportDecorator
	{
		IApplicationLite mApplication;
		int mMaxLength = 32;

		public LoggingTransportDecorator(Transport transport, IApplicationLite app, int maxLength=32)
			: base(transport)
		{
			mApplication = app;
			mMaxLength = maxLength;
		}

		#region ITransport implementation

		public override int Read(byte[] buffer, SecTime timeout)
		{
			int n = mTransport.Read(buffer, timeout);
			if (n > 0)
			{
				var str = BitConverter.ToString(buffer, 0, Math.Min(n,mMaxLength));
				MsgConsole.WriteLine("{0}<< {1}", mApplication.Name, str);
			}
			return n;
		}

		public override int Write(byte[] buffer, int offset, int length, Address destination, SecTime timeout)
		{
			var str = BitConverter.ToString(buffer, offset, Math.Min(length, mMaxLength));
			MsgConsole.WriteLine("{0}>> {1}", mApplication.Name, str);

			return mTransport.Write(buffer, offset, length, destination, timeout);
		}

		#endregion

		public Transport Transport { get { return mTransport; } }
	}
}
