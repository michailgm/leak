﻿using System;
using System.Net;

namespace Leak.Suckets
{
    internal class TcpSocketConnectResult : TcpSocketResult
    {
        public TcpSocket Socket { get; set; }

        public IPEndPoint Endpoint { get; set; }

        public TcpSocketConnectCallback OnConnected { get; set; }

        public TcpSocketConnect Unpack(IAsyncResult result)
        {
            return new TcpSocketConnect(Status, Socket, Endpoint);
        }

        protected override void OnCompleted(int affected)
        {
            OnConnected?.Invoke(new TcpSocketConnect(Status, Socket, Endpoint));
        }

        protected override void OnFailed(TcpSocketStatus status)
        {
            OnConnected?.Invoke(new TcpSocketConnect(Status, Socket, Endpoint));
        }
    }
}