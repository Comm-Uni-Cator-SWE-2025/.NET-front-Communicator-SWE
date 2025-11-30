using Communicator.UX.Analytics.Models;
using Communicator.UX.Analytics.Services;
using Xunit;

namespace AnalyticsApp.Tests
{
    /// <summary>
    /// Basic tests to confirm ApiService calls successfully and returns data safely.
    /// </summary>
    public class ApiServiceTests
    {
        /// <summary>
        /// Ensures service method executes without throwing any exception.
        /// </summary>
        [Fact]
        public async Task FetchAIDataAsync_ShouldExecuteWithoutError()
        {
            // Arrange
            var service = new ApiService();

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => service.FetchAIDataAsync());
            Assert.Null(exception);   // passes if no exception thrown
        }

        /// <summary>
        /// Ensures service never returns null (can return empty list, that's fine).
        /// </summary>
        [Fact]
        public async Task FetchAIDataAsync_ShouldNotReturnNull()
        {
            // Arrange
            var service = new ApiService();

            // Act
            var result = await service.FetchAIDataAsync();

            // Assert
            Assert.NotNull(result);   // empty list OK, but not null
        }

        /// <summary>
        /// Ensures returned result is a collection type, even if it's empty.
        /// </summary>
        [Fact]
        public async Task FetchAIDataAsync_ShouldReturnListType()
        {
            // Arrange
            var service = new ApiService();

            // Act
            var result = await service.FetchAIDataAsync();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<AIData>>(result);
        }
    }
}
