using System;
using System.Linq;
using Xunit;
using Communicator.UX.Analytics.Services;
using Communicator.UX.Analytics.Models;

namespace AnalyticsApp.Tests
{
    public class AIMessageServiceTests
    {
        // -----------------------------------------------------------
        // TEST 1 — Service must return at least 1 message
        // -----------------------------------------------------------
        [Fact]
        public void GetNext_ShouldReturnMessages()
        {
            // Arrange
            var service = new AIMessageService();

            // Act
            var messages = service.GetNext();

            // Assert
            Assert.NotNull(messages);
            Assert.NotEmpty(messages);
        }

        // -----------------------------------------------------------
        // TEST 2 — Returned messages must match the expected first group
        // -----------------------------------------------------------
        [Fact]
        public void GetNext_FirstGroup_ShouldMatchExpectedMessages()
        {
            // Arrange
            var service = new AIMessageService();

            var expected = new[]
            {
                "Developer 1 will handle the backend deployment scripts.",
                "Developer 2 will update the UI today."
            };

            // Act
            var result = service.GetNext().Select(m => m.Message).ToArray();

            // Assert
            Assert.Equal(expected.Length, result.Length);
            Assert.Equal(expected, result);
        }

        // -----------------------------------------------------------
        // TEST 3 — Each returned message must have a timestamp
        // -----------------------------------------------------------
        [Fact]
        public void GetNext_ShouldAssignDateTimeForEachMessage()
        {
            // Arrange
            var service = new AIMessageService();

            // Act
            var messages = service.GetNext();

            // Assert
            foreach (var msg in messages)
            {
                Assert.True(msg.Time > DateTime.MinValue);
            }
        }

        // -----------------------------------------------------------
        // TEST 4 — Service should cycle (round-robin)
        // -----------------------------------------------------------
        [Fact]
        public void GetNext_ShouldCycleMessageGroups()
        {
            // Arrange
            var service = new AIMessageService();

            // Act
            var g1 = service.GetNext().Select(m => m.Message).ToArray();
            var g2 = service.GetNext().Select(m => m.Message).ToArray();
            var g3 = service.GetNext().Select(m => m.Message).ToArray();
            var g4 = service.GetNext().Select(m => m.Message).ToArray();

            // After 4 calls, next must be g1 again
            var g5 = service.GetNext().Select(m => m.Message).ToArray();

            // Assert
            Assert.Equal(g1, g5);
        }
    }
}
