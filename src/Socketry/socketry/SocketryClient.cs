// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Packetparser;

namespace Socketry;

public class SocketryClient : Socketry
{
    public SocketryClient(byte[] socketPerTunnel, int serverPort, Dictionary<string, Func<byte[], byte[]>> procedures)
    {
        this.SetProcedures(procedures);

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

        short[] ports = ((Packet.Accept)acceptPacket).Ports;
        Console.WriteLine("Reached here...");

        this.SetTunnelsFromPorts(ports, socketPerTunnel);
    }

    public void SetTunnelsFromPorts(short[] ports, byte[] socketsPerTunnel)
    {
        List<Tunnel> tunnels = new List<Tunnel>();
        byte lastSocketNum = 0;
        foreach (byte socketNum in socketsPerTunnel)
        {
            List<short> portsForTunnel = new List<short>();
            for (int i = lastSocketNum; i < lastSocketNum + socketNum; i++)
            {
                portsForTunnel.Add(ports[i]);
            }
            lastSocketNum += socketNum;
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
