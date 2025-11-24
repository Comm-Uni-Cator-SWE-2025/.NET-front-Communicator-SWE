using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace socket
{
    public class SocketDebug : ISocket
    {
        PipeStream readPipe, writePipe;
        bool connected;
        bool blocking;

        public SocketDebug()
        {
            readPipe = new AnonymousPipeServerStream();
            writePipe = new AnonymousPipeClientStream("writePipe");
        }

        public bool IsConnected()
        {
            return connected;
        }

        public bool IsBlocking()
        {
            return blocking;
        }

        public int Read(MemoryStream dst)
        {
            int r =readPipe.Read(dst.GetBuffer());
            return r;
        }

        public int Write(MemoryStream src)
        {
            writePipe.Write(src.GetBuffer());
            return 0;
        }

        public Socket ConfigureBlocking(bool blocking)
        {
            this.blocking = blocking;
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public bool Connect(EndPoint remote)
        {
            connected = true;
            return true;
        }

        public SelectionKey Register(Selector sel, int ops, Object att)
        {
            //return sel.register(sel,ops,att);
            return null;
        }

        public int ReadOtherSide(MemoryStream dst)
        {
            int r = writePipe.Read(dst.GetBuffer());
            return r;
        }

        public int WriteOtherSide(MemoryStream src)
        {
            readPipe.Write(src.GetBuffer());
            return 0;
        }
    }
}
