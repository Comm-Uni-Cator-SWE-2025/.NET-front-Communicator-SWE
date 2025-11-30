using System.Globalization;
using Communicator.UX.Core.Converters;
using Xunit;

namespace Communicator.Core.Tests.Unit;

public class InverseBoolConverterTests
{
    [Fact]
    public void Convert_True_ReturnsFalse()
    {
        var converter = new InverseBoolConverter();
        var result = converter.Convert(true, typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.Equal(false, result);
    }

    [Fact]
    public void Convert_False_ReturnsTrue()
    {
        var converter = new InverseBoolConverter();
        var result = converter.Convert(false, typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.Equal(true, result);
    }

    [Fact]
    public void ConvertBack_True_ReturnsFalse()
    {
        var converter = new InverseBoolConverter();
        var result = converter.ConvertBack(true, typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.Equal(false, result);
    }
}
