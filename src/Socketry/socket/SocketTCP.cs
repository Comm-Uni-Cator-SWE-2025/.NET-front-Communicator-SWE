// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net;
using System.Net.Sockets;

namespace NewSocket;

public class SocketTCP : ISocket
{
    private Socket _osSocket;

    public SocketTCP()
    {
        _osSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {
            Blocking = false
        };
    }

    public SocketTCP(Socket socket)
    {
        _osSocket = socket;
    }

    public int Read(MemoryStream dst)
    {
        if (!_osSocket.Blocking && !_osSocket.Poll(0, SelectMode.SelectRead))
        {
            return 0;
        }

        byte[] temp = new byte[1024];
        int r = 0;
        try
        {
            r = _osSocket.Receive(temp);
            dst.Write(temp, 0, r);
        }
        catch (SocketException e)
        {
            if (e.SocketErrorCode != SocketError.WouldBlock)
            {
                Console.WriteLine($"Socket Exception: {e.Message}");
            }
        }
        if (r > 0)
        {
            Console.WriteLine($"Received message ({r} bytes):");
        }
        return r;
    }

    public long Read(MemoryStream dst, int offset, int length)
    {
        return _osSocket.Receive(dst.GetBuffer(), offset, length, SocketFlags.None);
    }

    public int Write(MemoryStream src)
    {
        byte[] buffer = src.GetBuffer();
        Console.WriteLine($"Sending message ({buffer.Length} bytes):");
        return _osSocket.Send(buffer);
    }

    public long Write(MemoryStream src, int offset, int length)
    {
        return _osSocket.Send(src.GetBuffer(), offset, length, SocketFlags.None);
    }

    public Socket ConfigureBlocking(bool block)
    {
        _osSocket.Blocking = block;
        return _osSocket;
    }

    public bool Connect(EndPoint remote)
    {
        _osSocket.Connect(remote);
        return _osSocket.Connected;
    }

    public bool IsConnected()
    {
        return _osSocket.Connected;
    }

    public bool IsBlocking()
    {
        return _osSocket.Blocking;
    }

    public SelectionKey Register(Selector sel, int ops, object att)
    {
        return sel.Register(_osSocket, ops, att);
    }

    public int Receive(byte[] temp)
    {
        throw new NotImplementedException();
    }

    public bool Poll(int v, SelectMode selectRead)
    {
        throw new NotImplementedException();
    }
}
