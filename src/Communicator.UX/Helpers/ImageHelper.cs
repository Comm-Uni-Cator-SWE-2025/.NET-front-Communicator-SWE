using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Communicator.UX.Helpers;

/// <summary>
/// Helper class for image conversions between RPC frame data and WPF image formats.
/// </summary>
public static class ImageHelper
{
    /// <summary>
    /// Converts int[][] pixel array from RImage to WPF BitmapSource.
    /// Pixel format: ARGB (each int contains alpha, red, green, blue channels).
    /// </summary>
    /// <param name="pixels">2D pixel array [height][width] with ARGB values.</param>
    /// <returns>BitmapSource for WPF display, or null if input is invalid.</returns>
    public static BitmapSource? ConvertToWpfBitmap(int[][]? pixels)
    {
        if (pixels == null || pixels.Length == 0 || pixels[0].Length == 0)
        {
            return null;
        }

        int height = pixels.Length;
        int width = pixels[0].Length;

        // Create WriteableBitmap for WPF
        var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

        try
        {
            bitmap.Lock();

            unsafe
            {
                byte* backBuffer = (byte*)bitmap.BackBuffer;
                int stride = bitmap.BackBufferStride;

                // Convert int[][] to byte array (BGRA format for WPF)
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int argb = pixels[y][x];

                        // Extract ARGB channels
                        byte a = (byte)((argb >> 24) & 0xFF);
                        byte r = (byte)((argb >> 16) & 0xFF);
                        byte g = (byte)((argb >> 8) & 0xFF);
                        byte b = (byte)(argb & 0xFF);

                        // Calculate pixel position
                        int offset = (y * stride) + (x * 4);

                        // Write BGRA (WPF format)
                        backBuffer[offset] = b;
                        backBuffer[offset + 1] = g;
                        backBuffer[offset + 2] = r;
                        backBuffer[offset + 3] = a;
                    }
                }
            }

            bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, width, height));
        }
        finally
        {
            bitmap.Unlock();
        }

        // Freeze for cross-thread access (frames may come from RPC thread)
        bitmap.Freeze();
        return bitmap;
    }

    /// <summary>
    /// Converts int[][] pixel array to BitmapSource with safe marshalling (slower but safer).
    /// Use this if the unsafe version causes issues.
    /// </summary>
    public static BitmapSource? ConvertToWpfBitmapSafe(int[][]? pixels)
    {
        if (pixels == null || pixels.Length == 0 || pixels[0].Length == 0)
        {
            return null;
        }

        int height = pixels.Length;
        int width = pixels[0].Length;

        // Create byte array for pixel data
        byte[] pixelData = new byte[height * width * 4]; // BGRA format

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int argb = pixels[y][x];

                // Extract ARGB channels
                byte a = (byte)((argb >> 24) & 0xFF);
                byte r = (byte)((argb >> 16) & 0xFF);
                byte g = (byte)((argb >> 8) & 0xFF);
                byte b = (byte)(argb & 0xFF);

                // Calculate pixel position
                int offset = (y * width * 4) + (x * 4);

                // Write BGRA
                pixelData[offset] = b;
                pixelData[offset + 1] = g;
                pixelData[offset + 2] = r;
                pixelData[offset + 3] = a;
            }
        }

        // Create BitmapSource from byte array
        BitmapSource bitmap = BitmapSource.Create(
            width,
            height,
            96,
            96,
            PixelFormats.Bgra32,
            null,
            pixelData,
            width * 4
        );

        bitmap.Freeze();
        return bitmap;
    }
}
