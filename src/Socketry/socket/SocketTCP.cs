using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace socket
{
    public class SocketTCP : ISocket
    {
        private Socket _osSocket;

        public SocketTCP()
        {
            _osSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _osSocket.Blocking = false;
        }

        public SocketTCP(Socket _socket)
        {
            _osSocket= _socket;
        }

        public int Read(MemoryStream dst)
        {
            if (!_osSocket.Blocking && !_osSocket.Poll(0, SelectMode.SelectRead))
            {
                return 0;
            }

            byte[] temp =new byte[1024];
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
                    System.Diagnostics.Debug.WriteLine($"[Socketry] Socket Exception: {e.Message}");
                }
            }
            if (r > 0)
            {
                // System.Diagnostics.Debug.WriteLine($"[Socketry] Received message ({r} bytes):");
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
            // System.Diagnostics.Debug.WriteLine($"[Socketry] Sending message ({buffer.Length} bytes):");
            return _osSocket.Send(buffer);
        }

        public long Write(MemoryStream src, int offset, int length)
        {
            return _osSocket.Send(src.GetBuffer(), offset, length, SocketFlags.None);
        }

        public Socket ConfigureBlocking(bool block) {
            _osSocket.Blocking= block;
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

        public SelectionKey Register(Selector sel, int ops, Object att)
        {
            // TODO: How to implement register
            return sel.register(_osSocket, ops, att);
        }

    }
}
