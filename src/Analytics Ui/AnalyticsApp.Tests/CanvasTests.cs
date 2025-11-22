using Xunit;
using AnalyticsApp.Models;
using AnalyticsApp.Services;
using AnalyticsApp.ViewModels;

namespace AnalyticsApp.Tests
{
    public class CanvasDataServiceTests
    {
        [Fact]
        public void FetchNext_ShouldReturnValidSnapshot()
        {
            // Arrange
            var service = new CanvasDataService();

            // Act
            CanvasData data = service.FetchNext();

            // Assert
            Assert.NotNull(data);
            Assert.True(data.FreeHand >= 0);
            Assert.True(data.StraightLine >= 0);
            Assert.True(data.Rectangle >= 0);
            Assert.True(data.Ellipse >= 0);
            Assert.True(data.Triangle >= 0);
        }

        [Fact]
        public void FetchNext_ShouldCycleThroughSnapshots()
        {
            // Arrange
            var service = new CanvasDataService();

            // Act
            var first = service.FetchNext();
            for (int i = 0; i < 5; i++)
                service.FetchNext();   // move through remaining snapshots

            var seventh = service.FetchNext(); // should loop back to snapshot 1

            // Assert: loop back behaves like first snapshot
            Assert.Equal(first.FreeHand, seventh.FreeHand);
            Assert.Equal(first.StraightLine, seventh.StraightLine);
            Assert.Equal(first.Rectangle, seventh.Rectangle);
            Assert.Equal(first.Ellipse, seventh.Ellipse);
            Assert.Equal(first.Triangle, seventh.Triangle);
        }
    }

    public class CanvasGraphViewModelTests
    {
        [Fact]
        public void AddSnapshot_ShouldAddDataCorrectly()
        {
            // Arrange
            var vm = new CanvasGraphViewModel();
            var data = new CanvasData
            {
                FreeHand = 10,
                StraightLine = 5,
                Rectangle = 3,
                Ellipse = 2,
                Triangle = 7
            };

            // Act
            vm.AddSnapshot(data, "T1");

            // Assert
            Assert.Single(vm.Labels);
            Assert.Single(vm.FreeHand);
            Assert.Single(vm.Line);
            Assert.Single(vm.Rectangle);
            Assert.Single(vm.Ellipse);
            Assert.Single(vm.Triangle);

            Assert.Equal(10, vm.FreeHand[0]);
            Assert.Equal(5, vm.Line[0]);
            Assert.Equal(3, vm.Rectangle[0]);
            Assert.Equal(2, vm.Ellipse[0]);
            Assert.Equal(7, vm.Triangle[0]);
        }

        [Fact]
        public void AddSnapshot_ShouldKeepOnlyLastThreeSnapshots()
        {
            // Arrange
            var vm = new CanvasGraphViewModel();

            // Add 4 snapshots → should drop T1
            vm.AddSnapshot(new CanvasData(), "T1");
            vm.AddSnapshot(new CanvasData(), "T2");
            vm.AddSnapshot(new CanvasData(), "T3");
            vm.AddSnapshot(new CanvasData(), "T4");

            // Assert
            Assert.Equal(3, vm.Labels.Count);
            Assert.DoesNotContain("T1", vm.Labels);

            Assert.Equal("T2", vm.Labels[0]);
            Assert.Equal("T3", vm.Labels[1]);
            Assert.Equal("T4", vm.Labels[2]);
        }

        [Fact]
        public void AddSnapshot_AllSeriesShouldStayInSync()
        {
            // Arrange
            var vm = new CanvasGraphViewModel();

            // Act: add several snapshots
            for (int i = 1; i <= 4; i++)
            {
                vm.AddSnapshot(new CanvasData
                {
                    FreeHand = i,
                    StraightLine = i + 1,
                    Rectangle = i + 2,
                    Ellipse = i + 3,
                    Triangle = i + 4
                }, $"T{i}");
            }

            // Assert ALL collections = 3 items
            Assert.Equal(3, vm.FreeHand.Count);
            Assert.Equal(3, vm.Line.Count);
            Assert.Equal(3, vm.Rectangle.Count);
            Assert.Equal(3, vm.Ellipse.Count);
            Assert.Equal(3, vm.Triangle.Count);
            Assert.Equal(3, vm.Labels.Count);

            // Check last values match snapshot 4
            Assert.Equal(4, vm.FreeHand.Last());
            Assert.Equal(5, vm.Line.Last());
            Assert.Equal(6, vm.Rectangle.Last());
            Assert.Equal(7, vm.Ellipse.Last());
            Assert.Equal(8, vm.Triangle.Last());
        }
    }
}
