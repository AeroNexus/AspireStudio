using System;
using System.Net;

using Aspire.Core.Utilities;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Aspire.Core.Messaging
{
    public class TcpTransport : Transport
    {
        private int listenPort;
        private bool isOpen = false;
        private TcpPeerToPeerClient client;
        public TcpTransport() : this(0) { }

        public TcpTransport(int port)
            : base("tcp")
        {
            if (port < 0 || port > 65535)
                throw new ArgumentOutOfRangeException("port");
            this.listenPort = port;
        }


        public override int Open()
        {
            if (client != null)
                throw new InvalidOperationException("cannot open");
            client = new TcpPeerToPeerClient(listenPort);
            int ret = client.Open();
            if(ret == 0)
            {
                var ep = client.LocalEndpoint as IPEndPoint;
                this.ListenAddress.Parse(string.Format("127.0.0.1:{0}", ep.Port));
            }
            return ret;
        }
      
        public override int Close()
        {
            if (client == null)
                return 0;
            int ret = client.Close();
            client = null;
            return ret;
        }

        public override int Read(byte[] buffer, SecTime timeout)
        {
            // there are bugs in the rest of the code that send large negative values to mean essentially 'no delay'
            // -1 is wait forever, so we map anything < -1 to 0, and pass other values unchanged
            int ms = timeout.ToMilliSeconds;
            ms = ms < -1 ? 0 : ms;

            return client.Read(buffer, ms);
        }

        public override int Write(byte[] buffer, int offset, int length, Address destination, SecTime timeout)
        {
            // there are bugs in the rest of the code that send large negative values to mean essentially 'no delay'
            // -1 is wait forever, so we map anything < -1 to 0, and pass other values unchanged
            int ms = timeout.ToMilliSeconds;
            ms = ms < -1 ? 0 : ms; 

            Address2 addr = destination as Address2;
            if (addr == null)
                throw new ArgumentException("destination");
            IPEndPoint ep = new IPEndPoint(
                new IPAddress(addr.NetworkDependentAddress),
                addr.Ndi
            );
            return client.Write(buffer, offset, length, ep, ms);
        }

        private readonly Address2 _listenAddress = new Address2();
        public override Address ListenAddress { get { return this._listenAddress; } }

        public override string ListenAddressString
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public override bool SupportsBestEffortDelivery
        {
            get { return false; }
        }

        public override bool SupportsBroadcast
        {
            get { return false; }
        }

        public override bool SupportsMulticast
        {
            get { return false; }
        }

        public override bool SupportsReliableDelivery
        {
            get { return true; }
        }
    }
}
