// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net;
using System.Net.Sockets;

/**
 ServerSocketChannel -> Socket
 SocketChannel -> Socket
 SelectableChannel -> Socket
 */
namespace NewSocket;

public interface ISocket
{
    int Read(MemoryStream dst);

    int Write(MemoryStream src);

    Socket ConfigureBlocking(bool block);

    bool Connect(EndPoint remote);

    bool IsConnected();

    bool IsBlocking();

    SelectionKey Register(Selector sel, int ops, object att);

    SelectionKey Register(Selector sel, int ops)
    {
        return Register(sel, ops, null);
    }

    int Receive(byte[] temp);
    bool Poll(int v, SelectMode selectRead);
}
