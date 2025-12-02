/*
 * -----------------------------------------------------------------------------
 *  File: RImageTests.cs
 *  Owner: Devansh Manoj Kesan
 *  Roll Number :142201017
 *  Module : ScreenShare
 *
 * -----------------------------------------------------------------------------
 */

/*
 * Test cases for the RImage helper.
 * I wrote these to cover the normal happy path and the error cases
 * so we can be confident about the byte deserialization logic.
 */

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Communicator.ScreenShare;

namespace Communicator.ScreenShare.Tests;

public class RImageTests
{
    private static byte[] CreateImageBytes(string ip, int width, int height, byte r = 0, byte g = 0, byte b = 0, long dataRate = 0)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // IP header
        byte[] ipBytes = Encoding.UTF8.GetBytes(ip);
        writer.Write(IPAddress.HostToNetworkOrder(ipBytes.Length));
        writer.Write(ipBytes);

        // Data rate (the implementation reads an Int64 here)
        writer.Write(dataRate);

        // Image dimensions (height then width, both in network byte order)
        writer.Write(IPAddress.HostToNetworkOrder(height));
        writer.Write(IPAddress.HostToNetworkOrder(width));

        // Pixel data (RGB bytes)
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                writer.Write(r);
                writer.Write(g);
                writer.Write(b);
            }
        }

        return stream.ToArray();
    }

    [Fact]
    public void Deserialize_SinglePixelImage_ReconstructsIpAndArgb()
    {
        // tiny 1x1 image: verify IP, data rate and decoded ARGB pixel
        byte[] data = CreateImageBytes("10.0.0.1", 1, 1, 255, 128, 64, dataRate: 42);

        RImage image = RImage.Deserialize(data);

        Assert.Equal("10.0.0.1", image.Ip);
        Assert.Equal(42, image.DataRate);
        Assert.Single(image.Image);
        Assert.Single(image.Image[0]);

        int pixel = image.Image[0][0];
        Assert.Equal(0xFF, (pixel >> 24) & 0xFF);   // alpha
        Assert.Equal(255, (pixel >> 16) & 0xFF);    // red
        Assert.Equal(128, (pixel >> 8) & 0xFF);     // green
        Assert.Equal(64, pixel & 0xFF);             // blue
    }

    [Fact]
    public void Deserialize_RectangularImage_FillsAllRowsAndColumns()
    {
        // 2x3 image: ensure every row/column gets filled correctly
        byte[] data = CreateImageBytes("rect@example.com", 3, 2, 10, 20, 30);

        RImage image = RImage.Deserialize(data);

        Assert.Equal(2, image.Image.Length); // height
        Assert.All(image.Image, row => Assert.Equal(3, row.Length)); // width

        Assert.All(image.Image.SelectMany(row => row), pixel =>
        {
            Assert.Equal(10, (pixel >> 16) & 0xFF);
            Assert.Equal(20, (pixel >> 8) & 0xFF);
            Assert.Equal(30, pixel & 0xFF);
        });
    }

    [Fact]
    public void Deserialize_ZeroHeightOrWidth_ProducesEmptyRows()
    {
        // zero height should give empty top-level array
        RImage zeroHeight = RImage.Deserialize(CreateImageBytes("zero-height", width: 1, height: 0));
        Assert.Empty(zeroHeight.Image);

        // zero width should give rows with empty inner arrays
        RImage zeroWidth = RImage.Deserialize(CreateImageBytes("zero-width", width: 0, height: 1));
        Assert.Single(zeroWidth.Image);
        Assert.Empty(zeroWidth.Image[0]);
    }

    [Fact]
    public void Deserialize_InvalidPayload_Throws()
    {
        // completely empty buffer
        Assert.ThrowsAny<Exception>(() => RImage.Deserialize(Array.Empty<byte>()));

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // valid IP header
        writer.Write(IPAddress.HostToNetworkOrder(4));
        writer.Write(Encoding.UTF8.GetBytes("abcd"));

        // valid data rate
        writer.Write(0L);

        // valid dimensions
        writer.Write(IPAddress.HostToNetworkOrder(2)); // height
        writer.Write(IPAddress.HostToNetworkOrder(2)); // width

        // incomplete pixel data (should be 2*2*3 = 12 bytes, we write only 2)
        writer.Write(new byte[] { 1, 2 });

        Assert.ThrowsAny<Exception>(() => RImage.Deserialize(stream.ToArray()));
    }
}