﻿using System;
using System.Net;
using System.Threading.Tasks;

namespace Leak.Suckets
{
    public interface TcpSocket : IDisposable
    {
        void Bind();

        void Bind(int port);

        void Bind(IPAddress address);

        TcpSocketInfo Info();

        void Listen(int backlog);

        void Accept(TcpSocketAcceptCallback callback);

        Task<TcpSocketAccept> Accept();

        void Connect(IPEndPoint endpoint, TcpSocketConnectCallback callback);

        Task<TcpSocketConnect> Connect(IPEndPoint endpoint);

        void Send(TcpSocketBuffer buffer, TcpSocketSendCallback callback);

        Task<TcpSocketSend> Send(TcpSocketBuffer buffer);

        void Receive(TcpSocketBuffer buffer, TcpSocketReceiveCallback callback);

        Task<TcpSocketReceive> Receive(TcpSocketBuffer buffer);
    }
}