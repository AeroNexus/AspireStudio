using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Xml.Serialization;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;

namespace Aspire.Core
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class MessageHandler
	{
		//Address mOwnAddress;
		IApplicationLite mApplication;
		List<Protocol> mProtocols = new List<Protocol>();
		Transport mTransport;

		//static int tags=1;
		//int tag = tags++;

		public event EventHandler HandleUnknownProtocol;

		public MessageHandler(IApplicationLite application,Transport transport)
		{
			mApplication = application;
			mTransport = transport;
		}

		//public MessageHandler(MessageHandler parent,IApplicationLite application)
		//{
		//	mApplication = application;
		//	mTransport = parent.mTransport;
		//	mOwnAddress = parent.OwnAddress.Clone();
		//}

		public Protocol AddProtocol(ProtocolId protocolId, Protocol protocol)
		{
			while (mProtocols.Count <= (int)protocolId)
				mProtocols.Add(null); // Lists need placeholders so can reference by id
			mProtocols[(int)protocolId] = protocol;

			return protocol;
		}

		public void Close()	{}

		static readonly SecTime maxWait = SecTime.Milliseconds(250);
    
    /// <summary>
    /// Field messages sent by other components to this Application-based component.
    /// Blocks on the asynchronous listen transport for lesser of executePeriod/0.25s,
    /// waiting for messages to arrive.
    /// When they do, select the appropriate protocol and give the network buffer to
    /// the protocol for appropriate action. Calls Execute() via a Timer. Blocks while
    /// app has not closed
    /// </summary>
    /// <param name="application"></param>
    public void FieldMessages()
		{
			var now = new SecTime();

			TimerList theTimerList = TimerList.The;
			while(!mApplication.IsClosing)
			{
				Clock.GetTime(ref now);
				theTimerList.Dispatch(now);

				Clock.GetTime(ref now);
				SecTime timerDeadline = theTimerList.UntilEarliestDeadline(now);
				SecTime timeout = SecTimeBracket(timerDeadline, maxWait);

				Poll(timeout);
			}
		}

    /// <summary>
    /// We need to serialize access to protocol.Parse because data is shared among
    /// multiple thread (at least when there are multiple Directories).
    /// See ie AspireBrowser::mCompIdBuf. It is written by multiple threads via
    /// Parse->mapped messages.
    /// </summary>
    static readonly object parseMutex = new object();
    public bool Parse(byte[] buffer, int length, Message parseHeader)
		{
      lock (parseMutex)
      {
        byte protocolId = buffer[0];
        if (protocolId < mProtocols.Count)
        {
          // Temporarily snag Monarch Local protocol. Need to move Aspire protocols up to 3,4,5
          if (protocolId == 2 && buffer[1] >= 32 && buffer[1] <= 36)
            protocolId = (byte)ProtocolId.Monarch;
          Protocol protocol = mProtocols[protocolId];
          if (protocol != null)
            return protocol.Parse(buffer, length, parseHeader);
        }
        else if (HandleUnknownProtocol != null)
          HandleUnknownProtocol(buffer, EventArgs.Empty);
        return false;
      }
		}

		byte[] pollBuffer = new byte[64*1024];
		public bool Poll(SecTime timeout)
		{
			
			int len = mTransport.Read(pollBuffer,timeout);
			if (len > 0)
			{
				Parse(pollBuffer, len, null);
				return true;
			}
			else if (len < 0)
				System.Threading.Thread.Sleep(1); // Keep from spinning when there is an error

			return false;
		}

		SecTime pollSec = new SecTime();
		public bool Poll(int seconds)
		{
			pollSec.Seconds = seconds;
			return Poll(pollSec);
		}

		/// <summary>
		/// Check for messages sent by other components to this Application-based component.
		///	Blocks on the asynchronous listen transport for lesser of wait/executePeriod/0.25s,
		///	waiting for messages to arrive.
		///	When they do, select the appropriate protocol and give the network buffer to
		///	the protocol for appropriate action. Calls Execute() via a Timer. Only called a single pass,
		///	used for polling.
		/// </summary>
		/// <param name="wait"></param>
		public void PollMessagesAndTimers(SecTime wait)
		{
			SecTime now = new SecTime();

			TimerList theTimerList = TimerList.The;
			Clock.GetTime(ref now);
			theTimerList.Dispatch(now);
			Clock.GetTime(ref now);
			SecTime timerDeadline = theTimerList.UntilEarliestDeadline(now);
			SecTime timeout = SecTimeBracket(wait,SecTimeBracket(timerDeadline, maxWait));

			Poll(timeout);
		}

		public Protocol Protocol(ProtocolId id)
		{
			if ((int)id >= mProtocols.Count) return null;
			return mProtocols[(int)id];
		}

		SecTime SecTimeBracket(SecTime time, SecTime timeMax)
		{
			if (time.Seconds == -1)
				return timeMax;
			return time > timeMax ? timeMax : time;
		}

	}
}
