using System.Globalization;
using System.Windows;
using Communicator.UX.Core.Converters;
using Xunit;

namespace Communicator.Core.Tests.Unit;

public class BooleanToVisibilityConverterTests
{
    [Fact]
    public void Convert_True_ReturnsVisible()
    {
        var converter = new BooleanToVisibilityConverter();
        var result = converter.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_False_ReturnsCollapsed()
    {
        var converter = new BooleanToVisibilityConverter();
        var result = converter.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_True_WithParameter_ReturnsCollapsed()
    {
        var converter = new BooleanToVisibilityConverter();
        var result = converter.Convert(true, typeof(Visibility), "invert", CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_False_WithParameter_ReturnsVisible()
    {
        var converter = new BooleanToVisibilityConverter();
        var result = converter.Convert(false, typeof(Visibility), "invert", CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Visible, result);
    }
}
