using Moq;
using Xunit;
using Communicator.Controller.RPC;
using Communicator.UX.Analytics.ViewModels;
using Communicator.UX.Analytics.Models;
using Communicator.UX.Analytics.Services;

namespace AnalyticsApp.Tests
{
    // ------------------------------------------------------------
    // ScreenShareGraphViewModel Tests (Graph Logic)
    // ------------------------------------------------------------
    public class ScreenShareGraphViewModelTests
    {
        [Fact]
        public void AddPoint_ShouldAddToCollection()
        {
            var vm = new ScreenShareGraphViewModel();

            vm.AddPoint(10, 5);

            Assert.Single(vm.Points);
            Assert.Equal(10, vm.Points[0].X);
            Assert.Equal(5, vm.Points[0].Y);
        }

        [Fact]
        public void Add_ShouldScrollWhenLimitExceeded()
        {
            var vm = new ScreenShareGraphViewModel();
            vm.WindowSeconds = 50;

            vm.Add(60, 4);

            Assert.Single(vm.Points);
            Assert.Equal(10, vm.XAxes[0].MinLimit);
            Assert.Equal(60, vm.XAxes[0].MaxLimit);
        }

        [Fact]
        public void AddMultiplePoints_ShouldIncreaseCollection()
        {
            var vm = new ScreenShareGraphViewModel();

            vm.AddPoint(0, 1);
            vm.AddPoint(5, 2);
            vm.AddPoint(10, 3);

            Assert.Equal(3, vm.Points.Count);
        }
    }

    // ------------------------------------------------------------
    // ScreenShareService Tests (RPC-based)
    // ------------------------------------------------------------
    public class ScreenShareServiceTests
    {
        /// <summary>
        /// Service with default constructor creates instance (for testing without RPC).
        /// </summary>
        [Fact]
        public void ScreenShareService_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var service = new ScreenShareService();

            // Assert
            Assert.NotNull(service);
        }

        /// <summary>
        /// Service with RPC parameter creates instance.
        /// </summary>
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

        /// <summary>
        /// GetAllTelemetry returns empty list initially.
        /// </summary>
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

        /// <summary>
        /// FetchTelemetryAsync without RPC returns empty list.
        /// </summary>
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

        /// <summary>
        /// FetchTelemetryAsync with null response returns empty list.
        /// </summary>
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

        /// <summary>
        /// FetchTelemetryAsync with empty response returns empty list.
        /// </summary>
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

        /// <summary>
        /// FetchTelemetryAsync calls the correct RPC endpoint.
        /// </summary>
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

        /// <summary>
        /// FetchTelemetryAsync handles exceptions gracefully.
        /// </summary>
        [Fact]
        public async Task FetchTelemetryAsync_WithException_ReturnsEmptyList()
        {
            // Arrange
            var mockRpc = new Mock<IRPC>();
            mockRpc.Setup(r => r.Call("core/ScreenTelemetry", It.IsAny<byte[]>()))
                   .ThrowsAsync(new InvalidOperationException("RPC error"));

            var service = new ScreenShareService(mockRpc.Object);

            // Act
            var result = await service.FetchTelemetryAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// GetAllFpsDataPoints returns empty list initially.
        /// </summary>
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

        /// <summary>
        /// GetAllTelemetry returns same list instance.
        /// </summary>
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
}
