// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Drawing;

namespace Communicator.ScreenShare;
public static class Utils
{
    /// <summary>
    /// Key constant for start_video_capture.
    /// </summary>
    public const string START_VIDEO_CAPTURE = "startVideoCapture";
    /// <summary>
    /// Key constant for stop_video_capture.
    /// </summary>
    public const string STOP_VIDEO_CAPTURE = "stopVideoCapture";
    /// <summary>
    /// Key constant for start_screen_capture.
    /// </summary>
    public const string START_SCREEN_CAPTURE = "startScreenCapture";
    /// <summary>
    /// Key constant for stop_screen_capture.
    /// </summary>
    public const string STOP_SCREEN_CAPTURE = "stopScreenCapture";
    /// <summary>
    /// Key constant for subscribe_as_viewer.
    /// </summary>
    public const string SUBSCRIBE_AS_VIEWER = "subscribeAsViewer";
    /// <summary>
    /// Key constant for unsubscribe_as_viewer.
    /// </summary>
    /// 

    public const string STOP_SHARE = "stopShare";

    public const string UPDATE_UI = "updateUI";
    /// <summary>
    /// Key constant for unsubscribe_as_viewer.
    /// </summary>
    public const string MODULE_REMOTE_KEY = "screenNVideo";
    /// <summary>
    /// Key constant for unsubscribe_as_viewer.
    /// </summary>
    public const int BUFFER_SIZE = 1024 * 10; // 10 kb
    /// <summary>
    /// Scale factor for X axis.
    /// </summary>
    public const int SCALE_X = 7;
    /// <summary>
    /// Scale factor for Y axis.
    /// </summary>
    public const int SCALE_Y = 5;
    /// <summary>
    /// PaddingX for the videoCapture to stitch to the ScreenCapture.
    /// </summary>
    public const int VIDEO_PADDING_X = 20;
    /// <summary>
    /// PaddingY for the videoCapture to stitch to the ScreenCapture.
    /// </summary>
    public const int VIDEO_PADDING_Y = 20;

    /// <summary>
    /// Width of the server.
    /// </summary>
    public const int SERVER_WIDTH = 800;
    /// <summary>
    /// Height of the server.
    /// </summary>
    public const int SERVER_HEIGHT = 600;
    /// <summary>
    /// Width of the client.
    /// </summary>
    public const int BYTE_MASK = 0xff;
    /// <summary>
    /// INT mask to get the first 8 bits.
    /// </summary>
    public const int INT_MASK_24 = 24;
    /// <summary>
    /// INT mask to get the second 8 bits.
    /// </summary>
    public const int INT_MASK_16 = 16;
    /// <summary>
    /// INT mask to get the third 8 bits.
    /// </summary>
    public const int INT_MASK_8 = 8;

    /// <summary>
    /// Seconds in milliseconds.
    /// </summary>
    public const int SEC_IN_MS = 1000;

    /// <summary>
    /// Milli-seconds in nanoseconds.
    /// </Next>
    // C# 7+ supports underscores as numeric separators, just like Java.
    public const int MSEC_IN_NS = 1_000_000;

    /// <summary>
    /// Maximum tries to serialize the compressed packets.
    /// </summary>
    public const int MAX_TRIES_TO_SERIALIZE = 3;

    /// <summary>
    /// Writes the given int to the buffer in big-endian format.
    /// </summary>
    /// <param name="bufferOut">The buffer to write to (e.g., MemoryStream).</param>
    /// <param name="data">The data to write.</param>
    public static void WriteInt(Stream bufferOut, int data)
    {
        // Note: Java's 'ByteArrayOutputStream.write(int)' writes the lowest 8 bits.
        // C#'s 'Stream.WriteByte(byte)' does the same.
        bufferOut.WriteByte((byte)((data >> INT_MASK_24) & BYTE_MASK));
        bufferOut.WriteByte((byte)((data >> INT_MASK_16) & BYTE_MASK));
        bufferOut.WriteByte((byte)((data >> INT_MASK_8) & BYTE_MASK));
        bufferOut.WriteByte((byte)(data & BYTE_MASK));
    }

    /// <summary>
    /// Converts the given image to its ARGB form.
    /// </summary>
    /// <param name="feed">The image (Bitmap).</param>
    /// <returns>int[][] : ARGB matrix 0xAARRGGBB</returns>
    public static int[][] ConvertToRGBMatrix(Bitmap feed)
    {
        // In C#, int[][] is a "jagged array" (an array of arrays),
        // which is the direct equivalent of Java's int[][].
        int[][] matrix = new int[feed.Height][];
        for (int i = 0; i < feed.Height; i++)
        {
            matrix[i] = new int[feed.Width];
            for (int j = 0; j < feed.Width; j++)
            {
                // .NET's GetPixel(x, y) returns a Color object.
                // .ToArgb() returns the 32-bit ARGB int,
                // which is equivalent to Java's BufferedImage.getRGB(x, y).
                matrix[i][j] = feed.GetPixel(j, i).ToArgb();
            }
        }
        return matrix;
    }

    /// <summary>
    /// Gets the local machine's outbound IP address.
    /// </summary>
    /// <returns>The IP address as a string.</returns>
    public static string GetSelfIP()
    {
        // Get IP address as string by connecting a UDP socket to a public DNS.
        // This forces the OS to choose the correct network interface.
        try
        {
            // We use 'using' for IDisposable objects,
            // which is the C# equivalent of try-with-resources.
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            // UdpClient(string, int) can also be used, but this
            // is a more direct parallel to the Java DatagramSocket logic.
            socket.Connect("8.8.8.8", 10002);
            IPEndPoint localEndPoint = socket.LocalEndPoint as IPEndPoint;
            return localEndPoint?.Address.ToString() ?? "127.0.0.1";
        }
        catch (SocketException e)
        {
            // In C#, RuntimeException doesn't exist. We just use the base Exception.
            // We pass the original exception as the 'innerException'.
            throw new Exception("Could not determine local IP address", e);
        }
    }
}
