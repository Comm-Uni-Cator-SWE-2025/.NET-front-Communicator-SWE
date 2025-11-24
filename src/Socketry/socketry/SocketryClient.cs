using packetparser;
using socket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socketry
{
    public class SocketryClient : Socketry
    {
        public SocketryClient(byte[] socketPerTunnel, int serverPort, Dictionary<String, Func<byte[], byte[]>> _procedures) 
        {
            this.SetProcedures(_procedures);

            Link link = new Link(serverPort);
            link.ConfigureBlocking(true);
            Packet initPacket = new Packet.Init(socketPerTunnel);
            link.SendPacket(initPacket);

            Console.WriteLine("Sent init...");
            Packet acceptPacket = link.GetPacket() as Packet.Accept;

            if (!(acceptPacket != null)) 
            {
                throw new Exception("Expected accept packet");
            }

            short[] ports = ((Packet.Accept)acceptPacket).ports;
            Console.WriteLine("Reached here...");

            this.SetTunnelsFromPorts(ports, socketPerTunnel);
        }

        public void SetTunnelsFromPorts(short[] ports, byte[] socketsPerTunnel) {
            List<Tunnel> tunnels = new List<Tunnel>();
            byte LastSocketNum = 0;
            foreach (byte socketNum in socketsPerTunnel)
            {
                List<short> portsForTunnel = new List<short>();
                for(int i=LastSocketNum; i < LastSocketNum + socketNum; i++)
                {
                    portsForTunnel.Add(ports[i]);
                }
                LastSocketNum += socketNum;
                try
                {
                    Tunnel tunnel = new Tunnel(portsForTunnel.ToArray());
                    tunnels.Add(tunnel);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            this._tunnels = tunnels.ToArray();
        }
    }
}
