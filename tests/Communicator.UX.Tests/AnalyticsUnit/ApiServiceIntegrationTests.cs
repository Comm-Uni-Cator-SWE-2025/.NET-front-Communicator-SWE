using Moq;
using Communicator.Controller.RPC;
using Communicator.Controller.Serialization;
using Communicator.UX.Analytics.Services;
using Communicator.UX.Analytics.Models;

namespace Communicator.UX.Tests.AnalyticsUnit;

/// <summary>
/// Integration tests for ApiService with mocked RPC.
/// </summary>
public class ApiServiceIntegrationTests
{
    [Fact]
    public void ApiService_WithRpc_CreatesInstance()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();

        // Act
        var service = new ApiService(mockRpc.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void GetAllData_Initially_ReturnsEmptyList()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        var service = new ApiService(mockRpc.Object);

        // Act
        var data = service.GetAllData();

        // Assert
        Assert.NotNull(data);
        Assert.Empty(data);
    }

    [Fact]
    public async Task FetchAIDataAsync_WithNullResponse_ReturnsEmptyList()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        mockRpc.Setup(r => r.Call("core/AiSentiment", It.IsAny<byte[]>()))
               .ReturnsAsync((byte[])null!);

        var service = new ApiService(mockRpc.Object);

        // Act
        var result = await service.FetchAIDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FetchAIDataAsync_WithEmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        mockRpc.Setup(r => r.Call("core/AiSentiment", It.IsAny<byte[]>()))
               .ReturnsAsync(Array.Empty<byte>());

        var service = new ApiService(mockRpc.Object);

        // Act
        var result = await service.FetchAIDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FetchAIDataAsync_CallsCorrectRpcEndpoint()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        mockRpc.Setup(r => r.Call("core/AiSentiment", It.IsAny<byte[]>()))
               .ReturnsAsync(Array.Empty<byte>());

        var service = new ApiService(mockRpc.Object);

        // Act
        await service.FetchAIDataAsync();

        // Assert
        mockRpc.Verify(r => r.Call("core/AiSentiment", It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task FetchAIDataAsync_WithException_ReturnsEmptyList()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        mockRpc.Setup(r => r.Call("core/AiSentiment", It.IsAny<byte[]>()))
               .ThrowsAsync(new Exception("RPC error"));

        var service = new ApiService(mockRpc.Object);

        // Act
        var result = await service.FetchAIDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetAllData_ReturnsSameListInstance()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        var service = new ApiService(mockRpc.Object);

        // Act
        var data1 = service.GetAllData();
        var data2 = service.GetAllData();

        // Assert
        Assert.Same(data1, data2);
    }
}
