using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

/**
 ServerSocketChannel -> Socket
 SocketChannel -> Socket
 SelectableChannel -> Socket
 */
namespace socket
{
    public interface ISocket
    {
        int Read(MemoryStream dst);

        int Write(MemoryStream src);

        Socket ConfigureBlocking(bool block);

        bool Connect(EndPoint remote);

        bool IsConnected();

        bool IsBlocking();

        SelectionKey Register(Selector sel, int ops, Object att);

        SelectionKey Register(Selector sel, int ops)
        {
            return Register(sel, ops, null);
        }
    }
}
