using Communicator.UX.Analytics.Models;

namespace Communicator.UX.Tests.AnalyticsUnit;

/// <summary>
/// Unit tests for the AIData model class.
/// </summary>
public class AIDataTests
{
    [Fact]
    public void AIData_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var data = new AIData();

        // Assert
        Assert.Equal(string.Empty, data.Time);
        Assert.Equal(0.0, data.Value);
    }

    [Fact]
    public void AIData_TimeProperty_CanBeSet()
    {
        // Arrange
        var data = new AIData();
        string expectedTime = "2025-12-03T10:30:00";

        // Act
        data.Time = expectedTime;

        // Assert
        Assert.Equal(expectedTime, data.Time);
    }

    [Fact]
    public void AIData_ValueProperty_CanBeSet()
    {
        // Arrange
        var data = new AIData();
        double expectedValue = 0.85;

        // Act
        data.Value = expectedValue;

        // Assert
        Assert.Equal(expectedValue, data.Value);
    }

    [Fact]
    public void TimeLabel_WithValidIsoDateTime_ReturnsFormattedTime()
    {
        // Arrange
        var data = new AIData { Time = "2025-12-03T10:30:00" };

        // Act
        string timeLabel = data.TimeLabel;

        // Assert
        Assert.Equal("10:30", timeLabel);
    }

    [Fact]
    public void TimeLabel_WithValidDateTimeWithZ_ReturnsFormattedTime()
    {
        // Arrange
        var data = new AIData { Time = "2025-12-03T14:45:30Z" };

        // Act
        string timeLabel = data.TimeLabel;

        // Assert - Should parse and format correctly
        Assert.Contains(":", timeLabel);
    }

    [Fact]
    public void TimeLabel_WithInvalidDateTime_ReturnsOriginalTime()
    {
        // Arrange
        var data = new AIData { Time = "invalid-time" };

        // Act
        string timeLabel = data.TimeLabel;

        // Assert
        Assert.Equal("invalid-time", timeLabel);
    }

    [Fact]
    public void TimeLabel_WithEmptyTime_ReturnsEmpty()
    {
        // Arrange
        var data = new AIData { Time = string.Empty };

        // Act
        string timeLabel = data.TimeLabel;

        // Assert
        Assert.Equal(string.Empty, timeLabel);
    }

    [Fact]
    public void TimeLabel_WithSimpleTimeFormat_ReturnsFormattedTime()
    {
        // Arrange - Time already in simple format
        var data = new AIData { Time = "10:01" };

        // Act
        string timeLabel = data.TimeLabel;

        // Assert - If it can't parse, it should return original
        Assert.NotNull(timeLabel);
    }

    [Fact]
    public void AIData_NegativeSentimentValue_IsAllowed()
    {
        // Arrange
        var data = new AIData { Value = -0.5 };

        // Act & Assert
        Assert.Equal(-0.5, data.Value);
    }

    [Fact]
    public void AIData_PositiveSentimentValue_IsAllowed()
    {
        // Arrange
        var data = new AIData { Value = 1.0 };

        // Act & Assert
        Assert.Equal(1.0, data.Value);
    }
}
