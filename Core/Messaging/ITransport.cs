﻿using System;

using Aspire.Core.Utilities;

namespace Aspire.Core.Messaging
{
	/// <summary>
	/// ITransport is the most basic interface implemented by transports in Aspire
	/// </summary>
	public interface ITransport
	{
		int Close();
		int Open();
		int Read(byte[] buffer, SecTime timeout);
		int Write(byte[] buffer, int offset, int length, Address destination, SecTime timeout);

		string Name { get; }
		Address ListenAddress { get; }
		string ListenAddressString { get; }
	}
}
