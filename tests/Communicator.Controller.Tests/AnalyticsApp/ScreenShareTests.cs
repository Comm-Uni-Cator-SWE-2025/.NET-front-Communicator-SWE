using Xunit;
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
    // ScreenShareService Tests (Only check call success)
    // ------------------------------------------------------------
    public class ScreenShareServiceTests
    {
        /// <summary>
        /// Service should execute without any exception.
        /// </summary>
        [Fact]
        public async Task ScreenShareDatasAsync_ShouldExecuteWithoutError()
        {
            var service = new ScreenShareService();

            var exception = await Record.ExceptionAsync(() => service.ScreenShareDatasAsync());

            Assert.Null(exception); // Means method ran successfully
        }

        /// <summary>
        /// Service must not return null (empty list is allowed).
        /// </summary>
        [Fact]
        public async Task ScreenShareDatasAsync_ShouldNotReturnNull()
        {
            var service = new ScreenShareService();

            var result = await service.ScreenShareDatasAsync();

            Assert.NotNull(result); // OK if this is empty
        }

        /// <summary>
        /// Service must return List type, even if empty.
        /// </summary>
        [Fact]
        public async Task ScreenShareDatasAsync_ShouldReturnListType()
        {
            var service = new ScreenShareService();

            var result = await service.ScreenShareDatasAsync();

            Assert.IsAssignableFrom<IEnumerable<ScreenShareData>>(result);
        }
    }
}
