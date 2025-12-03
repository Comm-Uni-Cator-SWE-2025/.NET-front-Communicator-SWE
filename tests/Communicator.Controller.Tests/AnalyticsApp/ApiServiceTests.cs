using Moq;
using Communicator.Controller.RPC;
using Communicator.UX.Analytics.Models;
using Communicator.UX.Analytics.Services;
using Xunit;

namespace AnalyticsApp.Tests
{
    /// <summary>
    /// Tests for ApiService which fetches AI sentiment data from core/AiSentiment RPC.
    /// </summary>
    public class ApiServiceTests
    {
        /// <summary>
        /// Ensures service can be created with RPC.
        /// </summary>
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

        /// <summary>
        /// Ensures GetAllData returns empty list initially.
        /// </summary>
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

        /// <summary>
        /// Ensures FetchAIDataAsync with null response returns empty list.
        /// </summary>
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

        /// <summary>
        /// Ensures FetchAIDataAsync with empty response returns empty list.
        /// </summary>
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

        /// <summary>
        /// Ensures FetchAIDataAsync calls the correct RPC endpoint.
        /// </summary>
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

        /// <summary>
        /// Ensures FetchAIDataAsync handles exceptions gracefully.
        /// </summary>
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

        /// <summary>
        /// Ensures GetAllData returns same list instance.
        /// </summary>
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

        /// <summary>
        /// Ensures result is assignable to IEnumerable of AIData.
        /// </summary>
        [Fact]
        public async Task FetchAIDataAsync_ReturnsListType()
        {
            // Arrange
            var mockRpc = new Mock<IRPC>();
            mockRpc.Setup(r => r.Call("core/AiSentiment", It.IsAny<byte[]>()))
                   .ReturnsAsync(Array.Empty<byte>());

            var service = new ApiService(mockRpc.Object);

            // Act
            var result = await service.FetchAIDataAsync();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<AIData>>(result);
        }
    }
}
