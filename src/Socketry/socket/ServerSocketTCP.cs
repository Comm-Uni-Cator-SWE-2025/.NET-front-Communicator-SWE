using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace socket
{
    public class ServerSocketTCP : IServerSocket
    {
        private Socket _osServerSocket;

        public ServerSocketTCP()
        {
            _osServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _osServerSocket.Blocking = false;
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
}
