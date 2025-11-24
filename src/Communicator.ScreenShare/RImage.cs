// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Net;
using System.Text;

namespace Communicator.ScreenShare;
public class RImage
{
    public int[][] Image { get; }

    public string Ip { get; }

    private RImage(int[][] imageArgs, string ipArgs)
    {
        Ip = ipArgs;
        Image = imageArgs;
    }
    public static RImage Deserialize(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        // --- get the IP ---

        int ipLen = IPAddress.NetworkToHostOrder(reader.ReadInt32());

        // Read the IP bytes
        byte[] ipBytes = reader.ReadBytes(ipLen);


        string ip = Encoding.UTF8.GetString(ipBytes);

        // --- get the UIImage ---
        int height = IPAddress.NetworkToHostOrder(reader.ReadInt32());
        int width = IPAddress.NetworkToHostOrder(reader.ReadInt32());


        int[][] image = new int[height][];
        for (int i = 0; i < height; i++)
        {
            image[i] = new int[width];
            for (int j = 0; j < width; j++)
            {

                int r = reader.ReadByte();
                int g = reader.ReadByte();
                int b = reader.ReadByte();

                // Reconstruct the ARGB pixel with full alpha
                int pixel = (Utils.BYTE_MASK << Utils.INT_MASK_24) | // Alpha (full)
                            (r << Utils.INT_MASK_16) |             // Red
                            (g << Utils.INT_MASK_8) |              // Green
                            b;                                     // Blue

                image[i][j] = pixel;
            }
        }
        return new RImage(image, ip);
    }
}
