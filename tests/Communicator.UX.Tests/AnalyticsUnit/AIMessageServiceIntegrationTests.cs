using Moq;
using Communicator.Controller.RPC;
using Communicator.UX.Analytics.Services;

namespace Communicator.UX.Tests.AnalyticsUnit;

/// <summary>
/// Integration tests for AIMessageService with mocked RPC.
/// </summary>
public class AIMessageServiceIntegrationTests
{
    [Fact]
    public void AIMessageService_WithRpc_CreatesInstance()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();

        // Act
        var service = new AIMessageService(mockRpc.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task FetchNextAsync_WithNullResponse_ReturnsEmptyList()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        mockRpc.Setup(r => r.Call("core/AiAction", It.IsAny<byte[]>()))
               .ReturnsAsync((byte[])null!);

        var service = new AIMessageService(mockRpc.Object);

        // Act
        var result = await service.FetchNextAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FetchNextAsync_WithEmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        mockRpc.Setup(r => r.Call("core/AiAction", It.IsAny<byte[]>()))
               .ReturnsAsync(Array.Empty<byte>());

        var service = new AIMessageService(mockRpc.Object);

        // Act
        var result = await service.FetchNextAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FetchNextAsync_CallsCorrectRpcEndpoint()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        mockRpc.Setup(r => r.Call("core/AiAction", It.IsAny<byte[]>()))
               .ReturnsAsync(Array.Empty<byte>());

        var service = new AIMessageService(mockRpc.Object);

        // Act
        await service.FetchNextAsync();

        // Assert
        mockRpc.Verify(r => r.Call("core/AiAction", It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task FetchNextAsync_WithException_ReturnsEmptyList()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        mockRpc.Setup(r => r.Call("core/AiAction", It.IsAny<byte[]>()))
               .ThrowsAsync(new Exception("RPC error"));

        var service = new AIMessageService(mockRpc.Object);

        // Act
        var result = await service.FetchNextAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetAllMessages_WithRpc_ReturnsEmptyListInitially()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        var service = new AIMessageService(mockRpc.Object);

        // Act
        var messages = service.GetAllMessages();

        // Assert
        Assert.NotNull(messages);
        Assert.Empty(messages);
    }

    [Fact]
    public void GetAllMessages_ReturnsSameListInstance()
    {
        // Arrange
        var mockRpc = new Mock<IRPC>();
        var service = new AIMessageService(mockRpc.Object);

        // Act
        var messages1 = service.GetAllMessages();
        var messages2 = service.GetAllMessages();

        // Assert
        Assert.Same(messages1, messages2);
    }
}
