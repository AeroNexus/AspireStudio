using System;
using System.Net;
using System.Net.Sockets;

using Aspire.Utilities;

namespace Aspire.CoreModels
{
	public class PmListener
	{
		const int size = 64;
		const int ListenPort = 1999;
		const SocketFlags flags = SocketFlags.None;
		Socket listener;
		byte[] buffer = new byte[size];
		IAsyncResult result;

		public EventHandler Received;

		private PmListener()
		{
			Open();
		}

		public static PmListener The
		{
			get
			{
				if (theListener == null)
					theListener = new PmListener();
				return theListener;
			}
		} static PmListener theListener;

		void OnReceive(IAsyncResult result)
		{
			int length;
			SocketError errorCode;
			try
			{
				length = listener.EndReceive(result, out errorCode);
			}
			catch (SocketException e)
			{
				if (e.SocketErrorCode == SocketError.ConnectionReset)
				{
					result = listener.BeginReceive(buffer, 0, size, flags, new AsyncCallback(OnReceive), null);
					Log.WriteLine("{0} OnPm: {2}", this, e.Message);
					return;
				}
				Log.WriteLine(e.Message);
				return;
			}
			if (length > 0 && Received != null)
				Received(this, EventArgs.Empty);

			result = listener.BeginReceive(buffer, 0, size, flags, new AsyncCallback(OnReceive), null);
		}

		void Open()
		{
			listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			var localEP = new IPEndPoint(IPAddress.Any, ListenPort);
			listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			listener.Bind(localEP);
			result = listener.BeginReceive(buffer, 0, size, flags, new AsyncCallback(OnReceive), null);
		}

		public void Reset()
		{
			int length;
			SocketError errorCode;
			try
			{
				length = listener.EndReceive(result, out errorCode);
			}
			catch (SocketException e)
			{
				Log.ReportException(e,"PmListener.Reset");
			}

			result = listener.BeginReceive(buffer, 0, size, flags, new AsyncCallback(OnReceive), null);
		}
	}
}
