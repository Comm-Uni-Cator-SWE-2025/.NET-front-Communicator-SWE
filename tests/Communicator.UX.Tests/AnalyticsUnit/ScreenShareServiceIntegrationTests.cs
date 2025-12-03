using Moq;
using Communicator.Controller.RPC;
using Communicator.UX.Analytics.Services;

namespace Communicator.UX.Tests.AnalyticsUnit;

/// <summary>
/// Integration tests for ScreenShareService with mocked RPC.
/// </summary>
public class ScreenShareServiceIntegrationTests
{
    [Fact]
    public void ScreenShareService_WithRpc_CreatesInstance()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();

        // Act
        var service = new ScreenShareService(mockRpc.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task FetchTelemetryAsync_WithNullResponse_ReturnsEmptyList()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        mockRpc.Setup(r => r.Call("core/ScreenTelemetry", It.IsAny<byte[]>()))
               .ReturnsAsync((byte[])null!);

        var service = new ScreenShareService(mockRpc.Object);

        // Act
        var result = await service.FetchTelemetryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FetchTelemetryAsync_WithEmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        mockRpc.Setup(r => r.Call("core/ScreenTelemetry", It.IsAny<byte[]>()))
               .ReturnsAsync(Array.Empty<byte>());

        var service = new ScreenShareService(mockRpc.Object);

        // Act
        var result = await service.FetchTelemetryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FetchTelemetryAsync_CallsCorrectRpcEndpoint()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        mockRpc.Setup(r => r.Call("core/ScreenTelemetry", It.IsAny<byte[]>()))
               .ReturnsAsync(Array.Empty<byte>());

        var service = new ScreenShareService(mockRpc.Object);

        // Act
        await service.FetchTelemetryAsync();

        // Assert
        mockRpc.Verify(r => r.Call("core/ScreenTelemetry", It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task FetchTelemetryAsync_WithException_ReturnsEmptyList()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        mockRpc.Setup(r => r.Call("core/ScreenTelemetry", It.IsAny<byte[]>()))
               .ThrowsAsync(new Exception("RPC error"));

        var service = new ScreenShareService(mockRpc.Object);

        // Act
        var result = await service.FetchTelemetryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetAllTelemetry_WithRpc_ReturnsEmptyListInitially()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        var service = new ScreenShareService(mockRpc.Object);

        // Act
        var telemetry = service.GetAllTelemetry();

        // Assert
        Assert.NotNull(telemetry);
        Assert.Empty(telemetry);
    }

    [Fact]
    public void GetAllFpsDataPoints_WithRpc_ReturnsEmptyListInitially()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        var service = new ScreenShareService(mockRpc.Object);

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
        var mockRpc = new Mock<IRPC>();
        var service = new ScreenShareService(mockRpc.Object);

        // Act
        var telemetry1 = service.GetAllTelemetry();
        var telemetry2 = service.GetAllTelemetry();

        // Assert
        Assert.Same(telemetry1, telemetry2);
    }
}
