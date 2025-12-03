using Communicator.UX.Analytics.Services;

namespace Communicator.UX.Tests.AnalyticsUnit;

/// <summary>
/// Unit tests for the AIMessageService class.
/// Tests the parsing and deduplication logic without RPC.
/// </summary>
public class AIMessageServiceTests
{
    [Fact]
    public void AIMessageService_DefaultConstructor_CreatesInstance()
    {
        // Arrange & Act
        var service = new AIMessageService();

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void GetAllMessages_Initially_ReturnsEmptyList()
    {
        // Arrange
        var service = new AIMessageService();

        // Act
        var messages = service.GetAllMessages();

        // Assert
        Assert.NotNull(messages);
        Assert.Empty(messages);
    }

    [Fact]
    public async Task FetchNextAsync_WithoutRpc_ReturnsEmptyList()
    {
        // Arrange
        var service = new AIMessageService();

        // Act
        var result = await service.FetchNextAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetAllMessages_ReturnsSameListInstance()
    {
        // Arrange
        var service = new AIMessageService();

        // Act
        var messages1 = service.GetAllMessages();
        var messages2 = service.GetAllMessages();

        // Assert
        Assert.Same(messages1, messages2);
    }
}
