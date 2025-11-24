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
    private static byte[] CreateImageBytes(string ip, int width, int height, byte r = 0, byte g = 0, byte b = 0)
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
        // this first test just checks that a tiny 1x1 image keeps the ip and pixel values
        byte[] data = CreateImageBytes("10.0.0.1", 1, 1, 255, 128, 64);

        RImage image = RImage.Deserialize(data);

        Assert.Equal("10.0.0.1", image.Ip);
        Assert.Single(image.Image);
        Assert.Single(image.Image[0]);

        int pixel = image.Image[0][0];
        Assert.Equal(0xFF, (pixel >> 24) & 0xFF);
        Assert.Equal(255, (pixel >> 16) & 0xFF);
        Assert.Equal(128, (pixel >> 8) & 0xFF);
        Assert.Equal(64, pixel & 0xFF);
    }

    [Fact]
    public void Deserialize_RectangularImage_FillsAllRowsAndColumns()
    {
        // here i use a 2x3 image to be sure every row/column gets filled correctly
        byte[] data = CreateImageBytes("rect@example.com", 3, 2, 10, 20, 30);

        RImage image = RImage.Deserialize(data);

        Assert.Equal(2, image.Image.Length);
        Assert.All(image.Image, row => Assert.Equal(3, row.Length));
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
        // zero height/width should not crash, so i make sure we just get empty arrays back
        RImage zeroHeight = RImage.Deserialize(CreateImageBytes("zero-height", 1, 0));
        Assert.Empty(zeroHeight.Image);

        RImage zeroWidth = RImage.Deserialize(CreateImageBytes("zero-width", 0, 1));
        Assert.Single(zeroWidth.Image);
        Assert.Empty(zeroWidth.Image[0]);
    }

    [Fact]
    public void Deserialize_InvalidPayload_Throws()
    {
        // finally i feed corrupt data to confirm we still throw instead of reading garbage
        Assert.ThrowsAny<Exception>(() => RImage.Deserialize(Array.Empty<byte>()));

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write(IPAddress.HostToNetworkOrder(4));
        writer.Write(Encoding.UTF8.GetBytes("abcd"));
        writer.Write(IPAddress.HostToNetworkOrder(2));
        writer.Write(IPAddress.HostToNetworkOrder(2));
        writer.Write(new byte[] { 1, 2 }); // incomplete pixel data

        Assert.ThrowsAny<Exception>(() => RImage.Deserialize(stream.ToArray()));
    }
}



