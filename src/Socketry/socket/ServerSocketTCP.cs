// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net;
using System.Net.Sockets;

namespace NewSocket;

public class ServerSocketTCP : IServerSocket
{
    private Socket _osServerSocket;

    public ServerSocketTCP()
    {
        _osServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {
            Blocking = false
        };
    }

    public Socket Bind(EndPoint local, int backlog)
    {
        _osServerSocket.Bind(local);
        _osServerSocket.Listen(backlog);
        return _osServerSocket;
    }

    public Socket ConfigureBlocking(bool block)
    {
        _osServerSocket.Blocking = block;
        return _osServerSocket;
    }

    public ISocket Accept()
    {
        return new SocketTCP(_osServerSocket.Accept());
    }


}
