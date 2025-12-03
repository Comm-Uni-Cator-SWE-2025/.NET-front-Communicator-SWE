using Communicator.UX.Analytics.Services;

namespace Communicator.UX.Tests.AnalyticsUnit;

/// <summary>
/// Unit tests for the ScreenShareService class.
/// Tests the service behavior without RPC.
/// </summary>
public class ScreenShareServiceTests
{
    [Fact]
    public void ScreenShareService_DefaultConstructor_CreatesInstance()
    {
        // Arrange & Act
        var service = new ScreenShareService();

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void GetAllTelemetry_Initially_ReturnsEmptyList()
    {
        // Arrange
        var service = new ScreenShareService();

        // Act
        var telemetry = service.GetAllTelemetry();

        // Assert
        Assert.NotNull(telemetry);
        Assert.Empty(telemetry);
    }

    [Fact]
    public async Task FetchTelemetryAsync_WithoutRpc_ReturnsEmptyList()
    {
        // Arrange
        var service = new ScreenShareService();

        // Act
        var result = await service.FetchTelemetryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetAllFpsDataPoints_Initially_ReturnsEmptyList()
    {
        // Arrange
        var service = new ScreenShareService();

        // Act
        var dataPoints = service.GetAllFpsDataPoints();

        // Assert
        Assert.NotNull(dataPoints);
        Assert.Empty(dataPoints);
    }

    [Fact]
    public void GetAllTelemetry_ReturnsSameListInstance()
    {
        // Arrange
        var service = new ScreenShareService();

        // Act
        var telemetry1 = service.GetAllTelemetry();
        var telemetry2 = service.GetAllTelemetry();

        // Assert
        Assert.Same(telemetry1, telemetry2);
    }
}
