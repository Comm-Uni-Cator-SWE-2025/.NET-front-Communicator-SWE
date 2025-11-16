using packetparser;
using socket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace socketry
{
    public class SocketryServer : Socketry
    {
        private static short LINK_PORT_STATUS = (short)10000;

        public SocketryServer(int serverPort, Dictionary<String, Func<byte[], byte[]>> _procedures)
        {
            this.SetProcedures(_procedures);

            IServerSocket initServer = StartAt(serverPort);
            if (initServer == null) { 
                // throw exception
            }
            initServer.ConfigureBlocking(true);

            ISocket clientInitChannel = initServer.Accept();

            Link link = new Link(clientInitChannel);
            link.ConfigureBlocking(true);

            Packet initPacket = link.GetPacket();

            if(!(initPacket is not Packet.Init))
            {
                Console.WriteLine("Expected init packet...");
                //throw exception
            }

            byte[] socketsPerTunnel = ((Packet.Init)initPacket).channels;

            List<IServerSocket> serverSockets = new List<IServerSocket> ();
            List<short> ports = new List<short> ();
            short currentPort = LINK_PORT_STATUS;

            foreach(byte socketNum in socketsPerTunnel)
            {
                for(short i = 0; i < socketNum; i++)
                {
                    while (true)
                    {
                        currentPort++;
                        IServerSocket serverSocket = StartAt(currentPort);
                        if (serverSocket == null) { 
                            continue;
                        }

                        serverSockets.Add(serverSocket);
                        ports.Add(currentPort);
                        break;
                    }
                }
            }

            short[] portsArray = new short[ports.Count];
            for(int i = 0; i < ports.Count; i++)
            {
                portsArray[i] = ports[i];
            }

            Packet acceptPacket = new Packet.Accept(portsArray);
            link.SendPacket(acceptPacket);

            List<ISocket> clientSockets = new List<ISocket> ();

            foreach (IServerSocket serverSocket in serverSockets)
            {
                serverSocket.ConfigureBlocking (true);
                ISocket clientChannel = serverSocket.Accept();
                clientSockets.Add(clientChannel);
            }

            this.SetTunnelsFromSockets(clientSockets.ToArray(), socketsPerTunnel);

        }

        public void SetTunnelsFromSockets(ISocket[] sockets, byte[] socketsPerTunnel)
        {
            List<Tunnel> tunnels = new List<Tunnel>();

            byte LastSocketNum = 0;
            foreach (byte socketNum in socketsPerTunnel)
            {
                List<ISocket> socketsForTunnel = sockets.Skip(LastSocketNum).Take(socketNum).ToList();
                LastSocketNum += socketNum;
                try
                {
                    Tunnel tunnel = new Tunnel(socketsForTunnel.ToArray<ISocket>());
                    tunnels.Add(tunnel);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private IServerSocket StartAt(int serverPort)
        {
            IServerSocket serverSocketChannel;
            try
            {
                serverSocketChannel = new ServerSocketTCP();
                serverSocketChannel.Bind(new IPEndPoint(IPAddress.Loopback, serverPort));
                return serverSocketChannel;
            }
            catch (Exception e) { 
                Console.WriteLine(e.ToString());
            }
            return null;
        }
    }
}
