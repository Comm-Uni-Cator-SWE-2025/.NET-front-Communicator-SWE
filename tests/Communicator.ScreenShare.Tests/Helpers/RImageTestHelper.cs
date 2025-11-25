/*
 * -----------------------------------------------------------------------------
 *  File: RImageTestHelper.cs
 *  Owner: Devansh Manoj Kesan
 *  Roll Number :142201017
 *  Module : ScreenShare
 *
 * -----------------------------------------------------------------------------
 */

using System.IO;
using System.Net;
using System.Text;
using Communicator.ScreenShare;

namespace Communicator.ScreenShare.Tests.Helpers;

public static class RImageTestHelper
{
     // Builds the byte payload that RImage.Deserialize expects.
    public static byte[] CreateRImageBytes(string ip, int width, int height)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        byte[] ipBytes = Encoding.UTF8.GetBytes(ip);
        writer.Write(IPAddress.HostToNetworkOrder(ipBytes.Length));
        writer.Write(ipBytes);
        writer.Write(IPAddress.HostToNetworkOrder(height));
        writer.Write(IPAddress.HostToNetworkOrder(width));

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((byte)0);
            }
        }

        return stream.ToArray();
    }

     // Shortcut for the common 1x1 frame used in tests.
    public static byte[] CreateSimpleRImageBytes(string ip)
        => CreateRImageBytes(ip, 1, 1);
}

