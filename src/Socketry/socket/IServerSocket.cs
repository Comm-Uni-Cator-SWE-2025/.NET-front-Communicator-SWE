using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace socket
{
    public interface IServerSocket
    {
        Socket Bind(EndPoint local)
        {
            return Bind(local, 0);
        }

        Socket Bind(EndPoint local, int backlog);

        Socket ConfigureBlocking(bool block);

        ISocket Accept();
    }
}
