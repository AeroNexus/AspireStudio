using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Aspire.Core.Utilities;

namespace Aspire.Core.Messaging
{
	public class TcpPeerToPeerClient
	{
        private readonly object mutex = new object();
        private readonly Semaphore readCount = new Semaphore(0, int.MaxValue);
        private readonly LinkedList<byte[]> readPackets = new LinkedList<byte[]>();
        private readonly Dictionary<IPEndPoint, TcpSession> sessionByEp = new Dictionary<IPEndPoint, TcpSession>();
        private readonly Socket acceptSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
   
        private readonly int listenPort;
        private bool wasOpened = false;
        private bool isOpen = false;

        public EndPoint LocalEndpoint { get; private set; }

        public TcpPeerToPeerClient() : this(0) { }

        public TcpPeerToPeerClient(int port)
        {
            if (port < 0 || port > 65535)
                throw new ArgumentOutOfRangeException("port");
            this.listenPort = port;
        }


        public int Open()
        {
            if (wasOpened)
                throw new InvalidOperationException("Client has already been opened");
            wasOpened = true;

            try
            {
                acceptSocket.Bind(new IPEndPoint(IPAddress.Any, this.listenPort));
                acceptSocket.Listen(int.MaxValue);
                acceptSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);

                this.LocalEndpoint = acceptSocket.LocalEndPoint;

                this.acceptSocket.BeginAccept(OnAccept, this);
                isOpen = true;
                return 0;
            }
            catch(Exception ex)
            {
                return -1;
            }
        }

        public int Close()
        {
            if (!isOpen)
                return 0;
            isOpen = false;
            try
            {
                this.acceptSocket.Close();
            }
            catch { }

            return 0;
        }

        public int Read(byte[] buffer, int msTimeout)
        {
            if (!readCount.WaitOne(msTimeout))
                return 0;

            byte[] packet;
            lock (this.mutex)
            {
                System.Diagnostics.Debug.Assert(readPackets.Count > 0);
                packet = readPackets.First.Value;
                readPackets.RemoveFirst();
            }
            System.Diagnostics.Debug.Assert(packet != null);
            int copyLen = buffer.Length < packet.Length ? buffer.Length : packet.Length;
            Array.Copy(packet, buffer, copyLen);
            return copyLen;
        }

        public int Write(byte[] buffer, int offset, int length, IPEndPoint ep, int msTimeout)
        {
            TcpSession session = null;
            lock (this.mutex)
            {
                if(!this.sessionByEp.TryGetValue(ep, out session))
                {
                    Socket clientSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                    clientSock.NoDelay = true;

                    session = new TcpSession(clientSock, ep);
                    sessionByEp[ep] = session;
                    clientSock.BeginConnect(ep, OnConnect, session);
                }
            }

            try
            {
                lock(session.Monitor)
                {
                    while(true)
                    {
                        if (session.Closing) break;
                        if (session.Connected && session.PendingSend == null) break;

                        if (!Monitor.Wait(session.Monitor, msTimeout))
                            return 0;
                        ;
                    }
                    if (session.Closing)
                        return 0;

                    FramingState state = new FramingState(buffer, offset, length);
                    session.PendingSend = state;
                    session.Socket.BeginSend(state.HeaderBuffer, 0, state.HeaderBuffer.Length, SocketFlags.None, OnSendHeader, session); 
                }

                return length;
            }
            catch(ObjectDisposedException)
            {
                return 0;
            }
        }

        private void OnPacketReceived(byte[] bytes)
        {
            lock(this.mutex)
            {
                this.readPackets.AddLast(bytes);
                this.readCount.Release();
            }
        }

        private void OnAccept(IAsyncResult result)
        {
            TcpPeerToPeerClient client = result.AsyncState as TcpPeerToPeerClient;
            try
            {
                Socket sock = client.acceptSocket.EndAccept(result);
                sock.NoDelay = true;
                IPEndPoint ep = sock.RemoteEndPoint as IPEndPoint;
                FramingState recvState = new FramingState();

                lock (client.mutex)
                {
                    if (client.sessionByEp.ContainsKey(ep))
                        return;
                    var session = new TcpSession(sock, ep);
                    lock (session.Monitor)
                    {
                        client.sessionByEp[ep] = session;

                        session.Connected = true;
                        session.PendingReceive = recvState;
                        session.Socket.BeginReceive(recvState.HeaderBuffer, 0, 4, SocketFlags.None, OnReceiveHeader, session);
                    }
                }
            }
            catch
            {
                ; // <shrug>
            }
            finally
            {
                client.acceptSocket.BeginAccept(OnAccept, client);
            }
        }

        private void OnConnect(IAsyncResult result)
        {
            var session = result.AsyncState as TcpSession;
            lock (session.Monitor)
            {
                try
                {
                    session.Socket.EndConnect(result);

                    FramingState state = new FramingState();
                    session.PendingReceive = state;
                    session.Socket.BeginReceive(state.HeaderBuffer, 0, 4, SocketFlags.None, OnReceiveHeader, session);
                    session.Connected = true;
                }
                catch
                {
                    lock (this.mutex)
                    {
                        this.sessionByEp.Remove(session.ep);
                    }
                    session.Closing = true;
                    session.Dispose();
                }
            }
        }

        private void OnSendHeader(IAsyncResult result)
        {
            TcpSession session = result.AsyncState as TcpSession;
            lock (session.Monitor)
            {
                try
                {
                    FramingState writeState = session.PendingSend;
                    System.Diagnostics.Debug.Assert(writeState != null);

                    writeState.HeaderBytesTransferred += session.Socket.EndSend(result);
                    int total = 4;
                    int remain = total - writeState.HeaderBytesTransferred;
                    System.Diagnostics.Debug.Assert(remain >= 0);

                    if (remain > 0)
                    {
                        session.Socket.BeginSend(
                            writeState.HeaderBuffer,
                            writeState.HeaderBytesTransferred,
                            remain, 
                            SocketFlags.None, OnSendHeader, session
                        );
                    }
                    else
                    {
                        if (writeState.DataBuffer.Length <= 0)
                            session.PendingSend = null;
                        else
                        {
                            session.Socket.BeginSend(
                                writeState.DataBuffer,
                                0,
                                writeState.DataBuffer.Length,
                                SocketFlags.None, OnSendData, session
                            );
                        }
                    }
                }
                catch
                {
                    session.Closing = true;
                }
            }
        }

        private void OnSendData(IAsyncResult result)
        {
            TcpSession session = result.AsyncState as TcpSession;
            lock (session.Monitor)
            {
                try
                {
                    FramingState sendState = session.PendingSend;
                    System.Diagnostics.Debug.Assert(sendState != null);

                    sendState.DataBytesTransferred += session.Socket.EndSend(result);
                    int total = sendState.DataBuffer.Length;
                    int remain = total - sendState.DataBytesTransferred;
                    System.Diagnostics.Debug.Assert(remain >= 0);

                    if (remain > 0)
                    {
                        session.Socket.BeginSend(
                            sendState.DataBuffer,
                            sendState.DataBytesTransferred,
                            remain,
                            SocketFlags.None, OnSendData, session
                        );
                    }
                    else
                    {
                        session.PendingSend = null;
                    }
                }
                catch
                {
                    session.Closing = true;
                }
            }
        }

        private void OnReceiveHeader(IAsyncResult result)
        {
            TcpSession session = result.AsyncState as TcpSession;
            lock (session.Monitor)
            {
                try
                {
                    FramingState recvState = session.PendingReceive;
                    System.Diagnostics.Debug.Assert(recvState != null);

                    recvState.HeaderBytesTransferred += session.Socket.EndReceive(result);
                    int total = 4;
                    int remain = total - recvState.HeaderBytesTransferred;
                    System.Diagnostics.Debug.Assert(remain >= 0);

                    if (remain > 0)
                    {
                        session.Socket.BeginReceive(
                            recvState.HeaderBuffer,
                            recvState.HeaderBytesTransferred,
                            remain,
                            SocketFlags.None, OnReceiveHeader, session
                        );
                    }
                    else
                    {
                        int datalen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvState.HeaderBuffer, 0));
                        if (datalen < 0 || datalen > 1024 * 1024)
                            throw new InvalidOperationException("Bad recv length " + datalen);

                        recvState.DataBuffer = new byte[datalen];
                        if (datalen == 0)
                        {
                            OnPacketReceived(recvState.DataBuffer);

                            recvState = new FramingState();
                            session.PendingReceive = recvState;
                            session.Socket.BeginReceive(recvState.HeaderBuffer, 0, 4, SocketFlags.None, OnReceiveHeader, session);
                        }
                        else
                        {
                            session.Socket.BeginReceive(
                                recvState.DataBuffer,
                                0,
                                recvState.DataBuffer.Length,
                                SocketFlags.None, OnReceiveData, session
                            );
                        }
                    }
                }
                catch
                {
                    session.Closing = true;
                }
            }
        }

        private void OnReceiveData(IAsyncResult result)
        {
            TcpSession session = result.AsyncState as TcpSession;
            lock (session.Monitor)
            {
                try
                {
                    FramingState recvState = session.PendingReceive;
                    System.Diagnostics.Debug.Assert(recvState != null);

                    recvState.DataBytesTransferred += session.Socket.EndReceive(result);
                    int total = recvState.DataBuffer.Length;
                    int remain = total - recvState.DataBytesTransferred;
                    System.Diagnostics.Debug.Assert(remain >= 0);

                    if(remain == 0)
                    {
                        OnPacketReceived(recvState.DataBuffer);

                        recvState = new FramingState();
                        session.PendingReceive = recvState;
                        session.Socket.BeginReceive(recvState.HeaderBuffer, 0, 4, SocketFlags.None, OnReceiveHeader, session);
                    }
                    else
                    {
                        session.Socket.BeginReceive(
                            recvState.DataBuffer,
                            recvState.DataBytesTransferred,
                            remain,
                            SocketFlags.None, OnReceiveData, session
                        );
                    }
                }
                catch
                {
                    session.Closing = true;
                }
            }
        }

        private class FramingState
        {
            public byte[] HeaderBuffer { get; private set; }
            public byte[] DataBuffer { get; internal set; }
            public int HeaderBytesTransferred { get; set; }
            public int DataBytesTransferred { get; set; }

            public FramingState()
            {
                this.HeaderBuffer = new byte[4];
            }
            public FramingState(byte[] buffer, int offset, int length)
            {
                if (length < 0 || length > 1024 * 1024)
                    throw new InvalidOperationException("Invalid length " + length);

                this.HeaderBuffer = BitConverter.GetBytes((int)IPAddress.HostToNetworkOrder((int)length));
                this.DataBuffer = new byte[length];
                Array.Copy(buffer, offset, this.DataBuffer, 0, length);
            }
        }

        private class TcpSession : IDisposable
        {
            public object Monitor { get; private set; }

            private bool _closing = false;
            public bool Closing
            {
                get { return _closing; }
                internal set
                {
                    _closing = value;
                    if (_closing)
                        System.Threading.Monitor.PulseAll(this.Monitor);
                }
            }

            private bool _connected = false;
            public bool Connected
            {
                get { return _connected; }
                internal set
                {
                    _connected = value;
                    if(_connected)
                        System.Threading.Monitor.PulseAll(this.Monitor);
                }
            }

            private FramingState _pendingRead = null;
            public FramingState PendingReceive
            {
                get { return _pendingRead; }
                internal set
                {
                    _pendingRead = value;
                    if(_pendingRead == null)
                        System.Threading.Monitor.PulseAll(this.Monitor);
                }
            }

            private FramingState _pendingWrite = null;
            public FramingState PendingSend
            {
                get { return _pendingWrite; }
                internal set
                {
                    _pendingWrite = value;
                    if(_pendingWrite == null)
                        System.Threading.Monitor.PulseAll(this.Monitor);
                }
            }

            public Socket Socket { get; private set; }
            public IPEndPoint ep { get; private set; }
            public TcpSession(Socket sock, IPEndPoint ep)
            {
                this.ep = ep;
                this.Monitor = new object();
                this.Socket = sock;
            }

            #region IDisposable Support
            public void Dispose()
            {
                this.Socket.Dispose();
            }

            #endregion
        }

    }
}
