﻿using System.Net;

namespace Leak.Suckets
{
    public class TcpSocketConnect
    {
        private readonly TcpSocketStatus status;
        private readonly TcpSocket socket;
        private readonly IPEndPoint endpoint;

        public TcpSocketConnect(TcpSocketStatus status, TcpSocket socket, IPEndPoint endpoint)
        {
            this.status = status;
            this.socket = socket;
            this.endpoint = endpoint;
        }

        public TcpSocketStatus Status
        {
            get { return status; }
        }

        public TcpSocket Socket
        {
            get { return socket; }
        }

        public IPEndPoint Endpoint
        {
            get { return endpoint; }
        }
    }
}