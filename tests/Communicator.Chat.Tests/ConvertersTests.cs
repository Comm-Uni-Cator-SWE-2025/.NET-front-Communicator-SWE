using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Communicator.Chat;
using FluentAssertions;
using Xunit;

namespace Communicator.Chat.Tests;

// ============================================================================
// FileSizeConverter Tests (10 tests - no Application dependency)
// ============================================================================
public class FileSizeConverterTests
{
    private readonly FileSizeConverter _converter;

    public FileSizeConverterTests()
    {
        _converter = new FileSizeConverter();
    }

    [Fact]
    public void Convert_WithZeroBytes_ReturnsZeroB()
    {
        var result = _converter.Convert(0L, typeof(string), null!, CultureInfo.InvariantCulture);
        result.Should().Be("0 B");
    }

    [Fact]
    public void Convert_With1024Bytes_ReturnsKB()
    {
        var result = _converter.Convert(1024L, typeof(string), null!, CultureInfo.InvariantCulture);
        result.Should().Be("1.0 KB");
    }

    [Fact]
    public void Convert_WithOneMB_ReturnsMB()
    {
        var result = _converter.Convert(1024L * 1024L, typeof(string), null!, CultureInfo.InvariantCulture);
        result.Should().Be("1.0 MB");
    }

    [Fact]
    public void Convert_WithOneGB_ReturnsGB()
    {
        var result = _converter.Convert(1024L * 1024L * 1024L, typeof(string), null!, CultureInfo.InvariantCulture);
        result.Should().Be("1.0 GB");
    }

    [Fact]
    public void Convert_WithFractionalKB_FormatsCorrectly()
    {
        var result = _converter.Convert(2560L, typeof(string), null!, CultureInfo.InvariantCulture);
        result.Should().Be("2.5 KB");
    }

    [Fact]
    public void Convert_WithLargeFile_FormatsCorrectly()
    {
        long fileSize = 50L * 1024L * 1024L;
        var result = _converter.Convert(fileSize, typeof(string), null!, CultureInfo.InvariantCulture);
        result.Should().Be("50.0 MB");
    }

    [Fact]
    public void Convert_WithNonLongValue_ReturnsZeroB()
    {
        var result = _converter.Convert("not a number", typeof(string), null!, CultureInfo.InvariantCulture);
        result.Should().Be("0 B");
    }

    [Fact]
    public void Convert_WithNullValue_ReturnsZeroB()
    {
        var result = _converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);
        result.Should().Be("0 B");
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        Action act = () => _converter.ConvertBack("1 KB", typeof(long), null!, CultureInfo.InvariantCulture);
        act.Should().Throw<NotImplementedException>();
    }
}

// ============================================================================
// MessageAlignmentConverter Tests (4 tests - no Application dependency)
// ============================================================================
public class MessageAlignmentConverterTests
{
    private readonly MessageAlignmentConverter _converter;

    public MessageAlignmentConverterTests()
    {
        _converter = new MessageAlignmentConverter();
    }

    [Fact]
    public void Convert_WithTrue_ReturnsRight()
    {
        var result = _converter.Convert(true, typeof(HorizontalAlignment), null!, CultureInfo.InvariantCulture);
        result.Should().Be(HorizontalAlignment.Right);
    }

    [Fact]
    public void Convert_WithFalse_ReturnsLeft()
    {
        var result = _converter.Convert(false, typeof(HorizontalAlignment), null!, CultureInfo.InvariantCulture);
        result.Should().Be(HorizontalAlignment.Left);
    }

    [Fact]
    public void Convert_WithNonBooleanValue_ReturnsLeft()
    {
        var result = _converter.Convert("not a boolean", typeof(HorizontalAlignment), null!, CultureInfo.InvariantCulture);
        result.Should().Be(HorizontalAlignment.Left);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        Action act = () => _converter.ConvertBack(HorizontalAlignment.Right, typeof(bool), null!, CultureInfo.InvariantCulture);
        act.Should().Throw<NotImplementedException>();
    }
}
