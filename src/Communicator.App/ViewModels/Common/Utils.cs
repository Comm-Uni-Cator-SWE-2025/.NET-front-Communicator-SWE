using System;
using System.Net;
using System.Net.Sockets;

namespace Communicator.App.ViewModels.Common;

public static class Utils
{
    /// <summary>
    /// Gives the IP address of self machine.
    /// </summary>
    /// <returns>IP address of self machine</returns>
    public static string? GetSelfIP()
    {
        try
        {
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            const int PingPort = 10002;
            socket.Connect("8.8.8.8", PingPort);
            IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
            return endPoint?.Address.ToString();
        }
        catch (Exception e)
        {
#pragma warning disable CA2201 // Do not raise reserved exception types
            throw new Exception("Could not determine local IP address. in GetSelfIP method.", e);
#pragma warning restore CA2201 // Do not raise reserved exception types
        }
    }
}
