using System.Threading.Tasks;
using Xunit;
using Communicator.UX.Analytics.ViewModels;
using Communicator.UX.Analytics.Models;
using Communicator.UX.Analytics.Services;

namespace MeetAnalyticsApp.Tests
{
    public class MeetAnalyticsTests
    {
        // -----------------------------------------------------------
        // TEST 1  Service: FetchStatsAsync returns valid stats
        // -----------------------------------------------------------
        [Fact]
        public async Task FetchStatsAsync_ShouldReturnValidStats()
        {
            // Arrange
            var service = new MeetAnalyticsService();

            // Act
            MeetStats stats = await service.FetchStatsAsync();

            // Assert
            Assert.NotNull(stats);
            Assert.True(stats.UsersPresent >= 0, "UsersPresent cannot be negative");
            Assert.True(stats.UsersLoggedOut >= 0, "UsersLoggedOut cannot be negative");
            Assert.NotNull(stats.PreviousSummary); // Can be empty, but must exist
        }

        // -----------------------------------------------------------
        // TEST 2  Service: FetchMessagesAsync returns a collection
        // -----------------------------------------------------------
        [Fact]
        public async Task FetchMessagesAsync_ShouldReturnMessages()
        {
            // Arrange
            var service = new MeetAnalyticsService();

            // Act
            var messages = await service.FetchMessagesAsync();

            // Assert
            Assert.NotNull(messages);
        }

        // -----------------------------------------------------------
        // TEST 3  ViewModel: MeetStats is initialized
        // -----------------------------------------------------------
        [Fact]
        public void MainViewModel_ShouldContainValidStats()
        {
            // Arrange
            var vm = new MeetAnalyticsViewModel();

            // Act
            var stats = vm.MeetStats;

            // Assert
            Assert.NotNull(stats);
            Assert.True(stats.UsersPresent >= 0);
            Assert.True(stats.UsersLoggedOut >= 0);
            Assert.NotNull(stats.PreviousSummary);
        }

        // -----------------------------------------------------------
        // TEST 4  ViewModel: Messages collection is initialized
        // -----------------------------------------------------------
        [Fact]
        public void MainViewModel_ShouldContainMessages()
        {
            // Arrange
            var vm = new MeetAnalyticsViewModel();

            // Act
            var messages = vm.Messages;

            // Assert
            Assert.NotNull(messages);
        }

        // -----------------------------------------------------------
        // TEST 5  PropertyChanged should raise event when MeetStats changes
        // -----------------------------------------------------------
        [Fact]
        public void MainViewModel_ShouldRaisePropertyChangedEvent()
        {
            // Arrange
            var vm = new MeetAnalyticsViewModel();
            bool eventRaised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.MeetStats))
                    eventRaised = true;
            };

            // Act
            vm.MeetStats = new MeetStats
            {
                UsersPresent = 100,
                UsersLoggedOut = 20,
                PreviousSummary = "Some summary"
            };

            // Assert
            Assert.True(eventRaised, "PropertyChanged event should be raised for MeetStats");
        }
    }
}
