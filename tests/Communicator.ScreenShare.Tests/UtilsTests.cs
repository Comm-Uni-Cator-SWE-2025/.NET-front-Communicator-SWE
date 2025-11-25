/*
 * -----------------------------------------------------------------------------
 *  File: UtilsTests.cs
 *  Owner: Devansh Manoj Kesan
 *  Roll Number :142201017
 *  Module : ScreenShare
 *
 * -----------------------------------------------------------------------------
 */

/*
 * Test cases for core helpers inside Utils.
 * These comments explain that we validate the big-endian writer,
 * the bitmap conversion, and the IP helper in a clear way.
 */

using System;
using System.Drawing;
using System.IO;
using System.Net;
using Communicator.ScreenShare;

namespace Communicator.ScreenShare.Tests;

public sealed class UtilsTests : IDisposable
{
    private readonly List<Bitmap> _bitmaps = new();

    private Bitmap CreateBitmap(int width = 2, int height = 2)
    {
        var bitmap = new Bitmap(width, height);
        _bitmaps.Add(bitmap);
        return bitmap;
    }

    [Fact]
    public void WriteInt_WritesBigEndianBytes()
    {
        using var stream = new MemoryStream();
        Utils.WriteInt(stream, 0x12345678);
        var bytes = stream.ToArray();
        Assert.Equal(new byte[] { 0x12, 0x34, 0x56, 0x78 }, bytes);
    }

    [Fact]
    public void ConvertToRGBMatrix_ReturnsArgbValues()
    {
        var bitmap = CreateBitmap(1, 1);
        bitmap.SetPixel(0, 0, Color.FromArgb(255, 10, 20, 30));

        int[][] matrix = Utils.ConvertToRGBMatrix(bitmap);

        Assert.Single(matrix);
        Assert.Single(matrix[0]);
        int pixel = matrix[0][0];
        Assert.Equal(10, (pixel >> 16) & 0xFF);
        Assert.Equal(20, (pixel >> 8) & 0xFF);
        Assert.Equal(30, pixel & 0xFF);
    }

    [Fact]
    public void GetSelfIP_ReturnsAddressOrLoopback()
    {
        string ip = Utils.GetSelfIP();
        Assert.False(string.IsNullOrWhiteSpace(ip));
        Assert.True(ip == "127.0.0.1" || IPAddress.TryParse(ip, out _));
    }

    public void Dispose()
    {
        foreach (var bitmap in _bitmaps)
        {
            bitmap.Dispose();
        }
        _bitmaps.Clear();
    }
}


