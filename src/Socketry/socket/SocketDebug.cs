// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;

namespace NewSocket;

public class SocketDebug : ISocket
{
    PipeStream _readPipe, _writePipe;
    bool _connected;
    bool _blocking;

    public SocketDebug()
    {
        _readPipe = new AnonymousPipeServerStream();
        _writePipe = new AnonymousPipeClientStream("writePipe");
    }

    public bool IsConnected()
    {
        return _connected;
    }

    public bool IsBlocking()
    {
        return _blocking;
    }

    public int Read(MemoryStream dst)
    {
        int r = _readPipe.Read(dst.GetBuffer());
        return r;
    }

    public int Write(MemoryStream src)
    {
        _writePipe.Write(src.GetBuffer());
        return 0;
    }

    public Socket ConfigureBlocking(bool blocking)
    {
        this._blocking = blocking;
        return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public bool Connect(EndPoint remote)
    {
        _connected = true;
        return true;
    }

    public SelectionKey Register(Selector sel, int ops, object att)
    {
        //return sel.register(sel,ops,att);
        return null;
    }

    public int ReadOtherSide(MemoryStream dst)
    {
        int r = _writePipe.Read(dst.GetBuffer());
        return r;
    }

    public int WriteOtherSide(MemoryStream src)
    {
        _readPipe.Write(src.GetBuffer());
        return 0;
    }
}
