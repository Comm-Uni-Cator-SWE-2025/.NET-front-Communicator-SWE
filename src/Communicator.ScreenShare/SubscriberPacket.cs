using System.Linq;
using System.Net;

/**
 * Contributed by Sandeep Kumar.
 */
namespace Communicator.ScreenShare;

/// <summary>
/// Subscribe Packet.
/// </summary>
public record SubscriberPacket(string Ip, bool ReqCompression)
{
    /// <summary>
    /// Serializes the packet for the networking layer, using Big-Endian byte order for IP integers.
    /// </summary>
    /// <returns>Serialized byte array</returns>
    public byte[] Serialize()
    {
        // The original Java logic serialized each IP octet as a 4-byte integer (16 bytes total for IP)
        // plus 1 byte for compression and 1 dummy byte.

        // 4 * 4 bytes (int) + 1 byte (reqCompression) + 1 byte (dummy) = 18 bytes
        const int TotalLength = (4 * sizeof(int)) + sizeof(byte) + sizeof(byte);

        byte[] buffer = new byte[TotalLength];
        int offset = 0;

        // --- 1. Dummy Byte ---
        // Java code: buffer.put((byte) (0));
        buffer[offset++] = (byte)0;

        // --- 2. IP Integers (4 x 4-byte, Big-Endian) ---
        // Split the IP string and parse each octet into an integer.
        int[] ipInts = Ip.Split('.')
                         .Select(int.Parse)
                         .ToArray();

        // PutInt in Java's ByteBuffer is Big-Endian. We must replicate this manually in C#.
        foreach (int ipInt in ipInts)
        {
            // Convert the int (32-bit value) to 4 bytes in Big-Endian (Most Significant Byte first)
            // (ipInt >> 24) is the most significant byte, written first.

            // Byte 1: MSB
            buffer[offset++] = (byte)((ipInt >> 24) & 0xFF);
            // Byte 2
            buffer[offset++] = (byte)((ipInt >> 16) & 0xFF);
            // Byte 3
            buffer[offset++] = (byte)((ipInt >> 8) & 0xFF);
            // Byte 4: LSB
            buffer[offset++] = (byte)(ipInt & 0xFF);
        }

        // --- 3. reqCompression Byte ---
        // Java code: buffer.put((byte) (reqCompression ? 1 : 0));
        buffer[offset] = (byte)(ReqCompression ? 1 : 0);

        return buffer;
    }
}