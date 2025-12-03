using Moq;
using Xunit;
using Communicator.Controller.RPC;
using Communicator.UX.Analytics.Services;
using Communicator.UX.Analytics.Models;

namespace AnalyticsApp.Tests
{
    /// <summary>
    /// Tests for AIMessageService which fetches action items from core/AiAction RPC.
    /// </summary>
    public class AIMessageServiceTests
    {
        /// <summary>
        /// Service with default constructor creates instance (for testing without RPC).
        /// </summary>
        [Fact]
        public void AIMessageService_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var service = new AIMessageService();

            // Assert
            Assert.NotNull(service);
        }

        /// <summary>
        /// Service with RPC parameter creates instance.
        /// </summary>
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

        /// <summary>
        /// GetAllMessages returns empty list initially.
        /// </summary>
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

        /// <summary>
        /// FetchNextAsync without RPC returns empty list.
        /// </summary>
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

        /// <summary>
        /// FetchNextAsync with null response returns empty list.
        /// </summary>
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

        /// <summary>
        /// FetchNextAsync with empty response returns empty list.
        /// </summary>
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

        /// <summary>
        /// FetchNextAsync calls the correct RPC endpoint.
        /// </summary>
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

        /// <summary>
        /// FetchNextAsync handles exceptions gracefully.
        /// </summary>
        [Fact]
        public async Task FetchNextAsync_WithException_ReturnsEmptyList()
        {
            // Arrange
            var mockRpc = new Mock<IRPC>();
            mockRpc.Setup(r => r.Call("core/AiAction", It.IsAny<byte[]>()))
                   .ThrowsAsync(new InvalidOperationException("RPC error"));

            var service = new AIMessageService(mockRpc.Object);

            // Act
            var result = await service.FetchNextAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// GetAllMessages returns same list instance.
        /// </summary>
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
}
