using AnalyticsApp.Models;
using AnalyticsApp.Services;
using Xunit;

namespace AnalyticsApp.Tests;

/// <summary>
/// Tests for ScreenShareService which returns sample screenshare sentiment data.
/// </summary>
public class ScreenShareServiceTests
{
    [Fact]
    public async Task ScreenShareDatasAsync_ShouldReturn10Items()
    {
        // Arrange
        var service = new ScreenShareService();

        // Act
        var result = await service.ScreenShareDatasAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Count);
    }

    [Fact]
    public async Task ScreenShareDatasAsync_ShouldReturnCorrectFirstItem()
    {
        // Arrange
        var service = new ScreenShareService();

        // Act
        var result = await service.ScreenShareDatasAsync();
        var first = result.First();

        // Assert
        var expectedUtc = DateTime.Parse("2025-11-07T10:00:00Z").ToUniversalTime();
        Assert.Equal(expectedUtc, first.Time.ToUniversalTime());
        Assert.Equal(7.0, first.Sentiment);
    }
}
