using System;
using System.Globalization;
using System.Windows;
using Communicator.Core.UX.Converters;
using Xunit;

namespace Communicator.Core.Tests.Unit;

public class StringToVisibilityConverterTests
{
    [Fact]
    public void Convert_NullString_ReturnsCollapsed()
    {
        var converter = new StringToVisibilityConverter();
        var result = converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_EmptyString_ReturnsCollapsed()
    {
        var converter = new StringToVisibilityConverter();
        var result = converter.Convert(string.Empty, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_NonEmptyString_ReturnsVisible()
    {
        var converter = new StringToVisibilityConverter();
        var result = converter.Convert("Hello", typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_NullString_WithParameter_ReturnsVisible()
    {
        var converter = new StringToVisibilityConverter();
        var result = converter.Convert(null, typeof(Visibility), "invert", CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_EmptyString_WithParameter_ReturnsVisible()
    {
        var converter = new StringToVisibilityConverter();
        var result = converter.Convert(string.Empty, typeof(Visibility), "invert", CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_NonEmptyString_WithParameter_ReturnsCollapsed()
    {
        var converter = new StringToVisibilityConverter();
        var result = converter.Convert("Hello", typeof(Visibility), "invert", CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        var converter = new StringToVisibilityConverter();
        Assert.Throws<NotImplementedException>(() => converter.ConvertBack(Visibility.Visible, typeof(string), null, CultureInfo.InvariantCulture));
    }
}
