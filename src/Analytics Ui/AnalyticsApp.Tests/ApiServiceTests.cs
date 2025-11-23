using AnalyticsApp.Models;
using AnalyticsApp.Services;
using Xunit;

namespace AnalyticsApp.Tests;

/// <summary>
/// Tests for ApiService.FetchAIDataAsync
/// </summary>
public class ApiServiceTests
{
    /// <summary>
    /// Ensures API returns exactly 10 items.
    /// </summary>
    [Fact]
    public async Task FetchAIDataAsync_ShouldReturn10Items()
    {
        // Arrange
        var service = new ApiService();

        // Act
        var result = await service.FetchAIDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Count);
    }

    /// <summary>
    /// Ensures all items have valid timestamps and sentiment values.
    /// </summary>
    [Fact]
    public async Task FetchAIDataAsync_ShouldContainValidValues()
    {
        // Arrange
        var service = new ApiService();

        // Act
        var result = await service.FetchAIDataAsync();

        // Assert
        Assert.All(result, item =>
        {
            Assert.True(item.Time > DateTime.MinValue, "Time should be a real timestamp");
            Assert.InRange(item.Value, -10, 10);   // adjust range if needed
        });
    }
}
