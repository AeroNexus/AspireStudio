using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Aspire.Core.Utilities;

namespace Aspire.Core.Messaging
{
	partial class TcpTransport : Transport
	{
		enum ErrorCode { OK, Error, OperationAborted };

		class TcpPeerToPeerClient : ISessionObserver
		{
			const int Seconds = 1000;
			const int KBytes = 1024;
			const int MAX_MSG_LEN = 64 * KBytes;
			bool mStarted = false;
			int mPort, mReadTimeout, mWriteTimeout;
			System.Threading.Timer mReadTimer, mWriteTimer;
			Acceptor mAcceptor = new Acceptor();
			DescriptorPool mReadPool = new DescriptorPool();
			DescriptorPool mWritePool = new DescriptorPool();
			Queue<TcpPeerToPeerSession> mReadBlockedSessions = new Queue<TcpPeerToPeerSession>();
			TransferDescriptorList mReadableDescriptors = new TransferDescriptorList();
			Dictionary<IPEndPoint, TcpPeerToPeerSession> mSessionByEndPoint = new Dictionary<IPEndPoint, TcpPeerToPeerSession>();
			IAsyncSession.ConnectDescriptor mConnectDescriptor = new IAsyncSession.ConnectDescriptor();
			AutoResetEvent mConnectEvent = new AutoResetEvent(false);

			internal TcpPeerToPeerClient(int port)
			{
				mPort = port;
				mReadTimeout = 1 * Seconds;
				mWriteTimeout = 10 * Seconds;
				mReadTimer = new System.Threading.Timer(new TimerCallback(OnReadTimeout));
				mWriteTimer = new System.Threading.Timer(new TimerCallback(OnReadTimeout));
			}

			TcpPeerToPeerSession AsyncAccept()
			{
				var session = TcpPeerToPeerSession.Create(this);
				mAcceptor.AsyncAccept(session);
				return session;
			}

			void mAcceptor_Accepted(TcpPeerToPeerSession session, ErrorCode error)
			{
				if (error == ErrorCode.OK)
				{
					var sock = session.Socket;
					sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
					sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 64 * KBytes);
					sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 64 * KBytes);
					var remoteEndPoint = sock.RemoteEndPoint as IPEndPoint;
					session.RemoteEndPoint = remoteEndPoint;
					mSessionByEndPoint[remoteEndPoint] = session;

					session.Start();
				}

				AsyncAccept();
			}

			IAsyncSession.ConnectDescriptor AsyncConnect(IPEndPoint remoteEndPoint)
			{
				var desc = mConnectDescriptor;
				var session = TcpPeerToPeerSession.Create(this);
				desc.session = session;
				desc.complete = false;

				var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				session.Socket = sock;
				sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
				sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, MAX_MSG_LEN);
				sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, MAX_MSG_LEN);
				sock.Bind(remoteEndPoint);
				session.RemoteEndPoint = remoteEndPoint;
				mSessionByEndPoint[remoteEndPoint] = session;

				sock.BeginConnect(remoteEndPoint, new AsyncCallback(OnConnect), desc);

				return desc;
			}

			void DispatchReaders()
			{
				if (mReadBlockedSessions.Count == 0)
					return;
				var desc = mReadPool.TryTake();
				if (desc == null)
				{
					return;
				}
				var session = mReadBlockedSessions.Dequeue();
				session.BeginRead(desc);
			}

			void OnConnect(IAsyncResult result)
			{
				if (result.IsCompleted)
				{
					var desc = result.AsyncState as IAsyncSession.ConnectDescriptor;
					desc.session.Socket.EndConnect(result);
					desc.complete = true;
					desc.error = ErrorCode.OK;
					if (desc.error == ErrorCode.OK)
					{
						desc.session.Start();
					}
					mConnectEvent.Set();
				}
			}

			void OnError(TcpPeerToPeerSession session, ErrorCode error)
			{
				mSessionByEndPoint.Remove(session.RemoteEndPoint);
				session.Stop();
			}

			public void OnReadComplete(TcpPeerToPeerSession session,
				IAsyncSession.TransferDescriptor desc,
				ErrorCode error)
			{

				if (error == ErrorCode.OK)
				{
					mReadableDescriptors.Enqueue(desc);
					IAsyncSession.TransferDescriptor ndesc = mReadPool.TryTake();
					if (null != ndesc)
					{
						session.BeginRead(ndesc);
					}
					else
					{
						mReadBlockedSessions.Enqueue(session);
					}
				}
				else
				{
					mReadPool.Give(desc);
					DispatchReaders();
					OnError(session, error);
				}
			}

			public void OnStart(TcpPeerToPeerSession session)
			{
				mReadBlockedSessions.Enqueue(session);
				mReadPool.Create(2, MAX_MSG_LEN);
				mWritePool.Create(1, MAX_MSG_LEN);
				DispatchReaders();
			}

			public void OnStop(TcpPeerToPeerSession session)
			{
				mReadPool.Destroy(2);
				mWritePool.Destroy(1);
			}

			public void OnWriteComplete(TcpPeerToPeerSession session,
				IAsyncSession.TransferDescriptor desc,
				ErrorCode error)
			{
				mWritePool.Give(desc);
				if (error != ErrorCode.OK)
				{
					OnError(session, error);
				}
			}

			internal IPEndPoint LocalEndpoint { get { return mStarted ? mAcceptor.LocalEndPoint : null; } }

			internal int Port { get { return mPort; } }

			internal int Read(byte[] buf)
			{
				mReadTimer.Change(mReadTimeout, 0);

				while (mReadableDescriptors.Count == 0)
				{
					if (mReadTimedOut)
						return 0;
					Thread.Sleep(1);
				}

				IAsyncSession.TransferDescriptor desc = mReadableDescriptors.Dequeue();

				int rlen = desc.Length < buf.Length ? desc.Length : buf.Length;
				Buffer.BlockCopy(desc.Buffer, desc.PayloadOffset, buf, 0, rlen);
				mReadPool.Give(desc);
				DispatchReaders();
				return rlen;
			}

			internal int Start()
			{
				if (mStarted)
					return -1;

				//poll_timer_.async_wait(on_poll_timer_expired_);

				var endPoint = new IPEndPoint(IPAddress.Any, Port);
				mAcceptor.Open(ProtocolType.Tcp);
				mAcceptor.SetOption(SocketOptionName.ReuseAddress, true);
				mAcceptor.Bind(endPoint);
				mAcceptor.Accepted += new Acceptor.AcceptedHandler(mAcceptor_Accepted);
				mAcceptor.Listen();
				mPort = mAcceptor.LocalEndPoint.Port;

				mStarted = true;
				AsyncAccept();
				return 0;
			}

			internal int Stop()
			{
				if (!mStarted)
					return -1;

				mStarted = false;
				//poll_timer_.cancel();
				mSessionByEndPoint.Clear();
				mReadableDescriptors.Clear();
				return 0;
			}

			void OnReadTimeout(object state)
			{
				mReadTimedOut = true;
				mReadTimer.Change(Timeout.Infinite, Timeout.Infinite);
			} bool mReadTimedOut;

			void OnWriteTimeout(object state)
			{
				mWriteTimedOut = true;
				mWriteTimer.Change(Timeout.Infinite, Timeout.Infinite);
			} bool mWriteTimedOut;

			internal int Write(byte[] buf, int offset, int length, IPEndPoint endPoint)
			{
				mWriteTimer.Change(mWriteTimeout, 0); // one shot

				// find session for remoteEndPoint.  if none exists, establish.
				TcpPeerToPeerSession session;
				if (!mSessionByEndPoint.TryGetValue(endPoint, out session))
				{
					var cdesc = AsyncConnect(endPoint);
					session = cdesc.session;

					mConnectEvent.WaitOne(mWriteTimeout);
					if (!cdesc.complete) // timed out
						return 0;
					if (cdesc.error != ErrorCode.OK)
						return -1;
				}

				//return session.Write(buf, length); // move all the following code to Session

				while (!session.Started())
				{
					Thread.Sleep(1);
					if (mWriteTimedOut)
						return 0;
				}

				while (session.WritePending())
				{
					Thread.Sleep(1);
					if (mWriteTimedOut)
						return 0;
				}

				IAsyncSession.TransferDescriptor desc;
				while (null == (desc = mWritePool.TryTake()))
				{
					Thread.Sleep(1);
					if (mWriteTimedOut)
						return 0;
				}

				desc.Length = length;
				Buffer.BlockCopy(buf, offset, desc.Buffer, desc.PayloadOffset, length);
				session.BeginWrite(desc);
				return length;
			}
		}

		interface ISessionObserver
		{
			void OnStart(TcpPeerToPeerSession session);
			void OnStop(TcpPeerToPeerSession session);
			void OnReadComplete(TcpPeerToPeerSession session,
				IAsyncSession.TransferDescriptor desc,
				ErrorCode error);
			void OnWriteComplete(TcpPeerToPeerSession session,
				IAsyncSession.TransferDescriptor desc,
				ErrorCode error);
		};

		class Acceptor
		{
			Socket mSocket;
			IPEndPoint mLocalEndPoint;

			internal delegate void AcceptedHandler(TcpPeerToPeerSession session, ErrorCode error);

			internal event AcceptedHandler Accepted;

			internal void AsyncAccept(TcpPeerToPeerSession session)
			{
				mSocket.BeginAccept(new AsyncCallback(OnAccepted), session);
			}

			internal void Bind(IPEndPoint endPoint)
			{
				mLocalEndPoint = endPoint;
				mSocket.Bind(endPoint);
			}
			internal IPEndPoint LocalEndPoint
			{
				get { return mSocket.LocalEndPoint as IPEndPoint; }
			}
			internal void Listen()
			{
				mSocket.Listen(5);
			}
			void OnAccepted(IAsyncResult result)
			{
				if (result.IsCompleted)
				{
					var session = result.AsyncState as TcpPeerToPeerSession;
					session.Socket = mSocket.EndAccept(result);
					if (Accepted != null)
						Accepted(session, ErrorCode.OK);
				}
			}

			internal void Open(ProtocolType protocol)
			{
				mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			}

			internal void SetOption(SocketOptionName opt, bool value)
			{
				mSocket.SetSocketOption(SocketOptionLevel.Socket, opt, value);
			}
		}

		class IAsyncSession
		{
			internal class TransferDescriptor
			{
				byte[] header = new byte[4];
				byte[] buffer; // .Net gives aligned memory
				int length;

				internal byte[] Buffer { get { return buffer; } }
				internal int Capacity { get; set; }
				internal byte[] Header { get { return header; } }
				internal int Length
				{
					get { return length; }
					set
					{
						length = value;
						BitConverter.GetBytes(length).CopyTo(buffer, 0);
					}
				}
				internal int PayloadOffset { get { return 4; } }

				internal TransferDescriptor(int capacity)
				{
					Capacity = capacity;
					buffer = new byte[capacity + 4];
				}

				~TransferDescriptor()
				{
					//delete[] buffer;
				}
			};

			internal class ConnectDescriptor
			{
				internal TcpPeerToPeerSession session = new TcpPeerToPeerSession();
				internal bool complete;
				internal ErrorCode error;
			};
		};

		class TcpPeerToPeerSession : ISessionObserver
		{
			internal TcpPeerToPeerSession() { }

			internal TcpPeerToPeerSession(ISessionObserver observer)
			{
				mObserver = observer;
			}

			internal static TcpPeerToPeerSession Create(ISessionObserver observer)
			{
				return new TcpPeerToPeerSession(observer);
			}

			internal Socket Socket
			{
				get { return mSocket; }
				set { mSocket = value; }
			}

			internal void Start()
			{
				if (!mStarted)
				{
					mStarted = true;
					OnStart(this);
				}
			}

			internal void Stop()
			{
				if (mStarted)
				{
					mStarted = false;
					mSocket.Close();
					OnStop(this);
				}
			}

			internal bool Started()
			{
				return mStarted;
			}

			internal IAsyncSession.TransferDescriptor BeginRead(IAsyncSession.TransferDescriptor desc)
			{
				//assert(mActiveReadDescriptor == null);
				mActiveReadDescriptor = desc;
				mSocket.BeginReceive(desc.Header, 0, 4, SocketFlags.None, new AsyncCallback(OnReadHeader), this);
				return desc;
			}

			internal void BeginWrite(IAsyncSession.TransferDescriptor desc)
			{
				//assert(mActiveWriteDescriptor == null);
				mActiveWriteDescriptor = desc;
				mSocket.BeginSend(desc.Buffer, 0, desc.Length + 4, SocketFlags.None, new AsyncCallback(OnWritten), this);
			}

			bool ReadPending()
			{
				return (mActiveReadDescriptor != null);
			}

			internal bool WritePending()
			{
				return (mActiveWriteDescriptor != null);
			}

			internal IPEndPoint RemoteEndPoint
			{
				get { return mRemoteEndPoint; }
				set { mRemoteEndPoint = value; }
			}

			const int MAX_MSG_LEN = 1024 * 64;
			const int MAGIC_NUM = 0xF1D0;

			public void OnStart(TcpPeerToPeerSession session)
			{
				mObserver.OnStart(session);
			}

			public void OnStop(TcpPeerToPeerSession session)
			{
				mObserver.OnStop(session);
			}

			public void OnReadComplete(TcpPeerToPeerSession session,
				IAsyncSession.TransferDescriptor desc,
				ErrorCode error)
			{
				//assert(mActiveReadDescriptor != null);
				mActiveReadDescriptor = null;
				mObserver.OnReadComplete(session, desc, error);
			}

			public void OnWriteComplete(TcpPeerToPeerSession session,
				IAsyncSession.TransferDescriptor desc,
				ErrorCode error)
			{
				//assert(mActiveWriteDescriptor != null);
				mActiveWriteDescriptor = null;
				mObserver.OnWriteComplete(session, desc, error);
			}

			void OnReadHeader(IAsyncResult result)
			{
				if (result.IsCompleted)
				{
					SocketError error;
					var len = mSocket.EndReceive(result, out error);
					var session = result.AsyncState as TcpPeerToPeerSession;
					OnReadComplete(session, session.mActiveReadDescriptor, ErrorCode.OK);
				}

				//assert(mActiveReadDescriptor != null);
				mSocket.BeginReceive(mActiveReadDescriptor.Buffer, 0, mActiveReadDescriptor.Capacity,
					SocketFlags.None, new AsyncCallback(OnReadMessage), this);
			}

			void OnReadMessage(IAsyncResult result)
			{
				if (result.IsCompleted)
				{
					//assert(mActiveReadDescriptor != null);
					OnReadComplete(this, mActiveReadDescriptor, ErrorCode.OK);
				}
			}

			void OnWritten(IAsyncResult result)
			{
				if (result.IsCompleted)
				{
					OnWriteComplete(this, mActiveWriteDescriptor, ErrorCode.OK);
				}
			}

			Socket mSocket;
			ISessionObserver mObserver;
			IPEndPoint mRemoteEndPoint;
			IAsyncSession.TransferDescriptor mActiveReadDescriptor;
			IAsyncSession.TransferDescriptor mActiveWriteDescriptor;

			bool mStarted;
		};

		class DescriptorPool
		{
			internal DescriptorPool()
			{
			}

			internal void Create(int count, int length)
			{
				if (mDesiredDeleteNumDesc > 0)
				{
					count -= mDesiredDeleteNumDesc;
					if (count < 0)
					{
						mDesiredDeleteNumDesc = -count;
						return;
					}
				}
				for (int ix = 0; ix < count; ix++)
				{
					IAsyncSession.TransferDescriptor desc = new IAsyncSession.TransferDescriptor(length);
					mDescriptors.Enqueue(desc);
				}
			}

			internal void Destroy(int count)
			{
				mDesiredDeleteNumDesc += count;
			}

			internal IAsyncSession.TransferDescriptor TryTake()
			{
				if (mDescriptors.Count == 0)
					return null;
				IAsyncSession.TransferDescriptor desc = mDescriptors.Dequeue();
				return desc;
			}

			internal void Give(IAsyncSession.TransferDescriptor desc)
			{
				if (mDesiredDeleteNumDesc > 0)
				{
					--mDesiredDeleteNumDesc;
					//delete desc;
				}
				else
				{
					mDescriptors.Enqueue(desc);
				}
			}

			TransferDescriptorList mDescriptors = new TransferDescriptorList();
			int mDesiredDeleteNumDesc;
		};

		class TransferDescriptorList : Queue<IAsyncSession.TransferDescriptor>
		{
		}

	}
}
