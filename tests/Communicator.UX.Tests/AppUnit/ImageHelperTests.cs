using System;
using System.Linq;
using System.Windows.Media.Imaging;
using Communicator.App.Helpers;
using Xunit;

namespace Communicator.App.Tests.Unit;

public class ImageHelperTests
{
    [Xunit.Fact]
    public void ConvertToWpfBitmapSafe_NullOrEmpty_ReturnsNull()
    {
        Assert.Null(ImageHelper.ConvertToWpfBitmapSafe(null));
        Assert.Null(ImageHelper.ConvertToWpfBitmapSafe(new int[0][]));
    }

    [Xunit.Fact]
    public void ConvertToWpfBitmapSafe_ValidPixels_ReturnsBitmapWithCorrectSize()
    {
        int width = 2;
        int height = 3;
        var pixels = Enumerable.Range(0, height)
            .Select(y => Enumerable.Range(0, width).Select(x => unchecked((int)0xFF0000FF)).ToArray())
            .ToArray();

        var bmp = ImageHelper.ConvertToWpfBitmapSafe(pixels);

        Assert.NotNull(bmp);
        Assert.Equal(width, bmp!.PixelWidth);
        Assert.Equal(height, bmp.PixelHeight);
    }
}
