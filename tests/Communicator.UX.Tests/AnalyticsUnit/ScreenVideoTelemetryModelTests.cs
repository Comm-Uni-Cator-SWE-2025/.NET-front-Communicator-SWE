using Communicator.UX.Analytics.Models;

namespace Communicator.UX.Tests.AnalyticsUnit;

/// <summary>
/// Unit tests for the ScreenVideoTelemetryModel class.
/// </summary>
public class ScreenVideoTelemetryModelTests
{
    [Fact]
    public void ScreenVideoTelemetryModel_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var model = new ScreenVideoTelemetryModel();

        // Assert
        Assert.Equal(0, model.StartTime);
        Assert.Equal(0, model.EndTime);
        Assert.NotNull(model.FpsEvery3Seconds);
        Assert.Empty(model.FpsEvery3Seconds);
        Assert.False(model.WithCamera);
        Assert.False(model.WithScreen);
        Assert.Equal(0.0, model.AvgFps);
        Assert.Equal(0.0, model.MaxFps);
        Assert.Equal(0.0, model.MinFps);
    }

    [Fact]
    public void ScreenVideoTelemetryModel_StartTime_CanBeSet()
    {
        // Arrange
        var model = new ScreenVideoTelemetryModel();
        long expectedTime = 1733234400000; // Dec 3, 2025 10:00:00 UTC

        // Act
        model.StartTime = expectedTime;

        // Assert
        Assert.Equal(expectedTime, model.StartTime);
    }

    [Fact]
    public void ScreenVideoTelemetryModel_EndTime_CanBeSet()
    {
        // Arrange
        var model = new ScreenVideoTelemetryModel();
        long expectedTime = 1733238000000; // Dec 3, 2025 11:00:00 UTC

        // Act
        model.EndTime = expectedTime;

        // Assert
        Assert.Equal(expectedTime, model.EndTime);
    }

    [Fact]
    public void ScreenVideoTelemetryModel_FpsEvery3Seconds_CanBePopulated()
    {
        // Arrange
        var model = new ScreenVideoTelemetryModel();
        var fpsValues = new List<double> { 30.0, 29.5, 31.0, 28.0, 30.5 };

        // Act
        model.FpsEvery3Seconds = fpsValues;

        // Assert
        Assert.Equal(5, model.FpsEvery3Seconds.Count);
        Assert.Equal(30.0, model.FpsEvery3Seconds[0]);
        Assert.Equal(28.0, model.FpsEvery3Seconds[3]);
    }

    [Fact]
    public void ScreenVideoTelemetryModel_WithCamera_CanBeSet()
    {
        // Arrange
        var model = new ScreenVideoTelemetryModel();

        // Act
        model.WithCamera = true;

        // Assert
        Assert.True(model.WithCamera);
    }

    [Fact]
    public void ScreenVideoTelemetryModel_WithScreen_CanBeSet()
    {
        // Arrange
        var model = new ScreenVideoTelemetryModel();

        // Act
        model.WithScreen = true;

        // Assert
        Assert.True(model.WithScreen);
    }

    [Fact]
    public void StartDateTime_WithValidEpochTime_ReturnsCorrectDateTime()
    {
        // Arrange
        var model = new ScreenVideoTelemetryModel
        {
            StartTime = 1733234400000 // Known epoch time
        };

        // Act
        DateTime startDateTime = model.StartDateTime;

        // Assert - Verify it's a valid date after Unix epoch
        Assert.True(startDateTime.Year >= 1970);
        Assert.Equal(12, startDateTime.Month);
    }

    [Fact]
    public void EndDateTime_WithValidEpochTime_ReturnsCorrectDateTime()
    {
        // Arrange
        var model = new ScreenVideoTelemetryModel
        {
            EndTime = 1733238000000
        };

        // Act
        DateTime endDateTime = model.EndDateTime;

        // Assert - Verify it's a valid date after Unix epoch
        Assert.True(endDateTime.Year >= 1970);
    }

    [Fact]
    public void TimeLabel_ReturnsFormattedTime()
    {
        // Arrange
        var model = new ScreenVideoTelemetryModel
        {
            StartTime = 1733234400000
        };

        // Act
        string timeLabel = model.TimeLabel;

        // Assert
        Assert.Contains(":", timeLabel);
        Assert.Equal(8, timeLabel.Length); // "HH:mm:ss" format
    }

    [Fact]
    public void ScreenVideoTelemetryModel_WithZeroStartTime_ReturnsEpochDateTime()
    {
        // Arrange
        var model = new ScreenVideoTelemetryModel { StartTime = 0 };

        // Act
        DateTime startDateTime = model.StartDateTime;

        // Assert
        Assert.Equal(1970, startDateTime.ToUniversalTime().Year);
    }

    [Fact]
    public void ScreenVideoTelemetryModel_P95Fps_CanBeSet()
    {
        // Arrange
        var model = new ScreenVideoTelemetryModel();

        // Act
        model.P95Fps = 25.5;

        // Assert
        Assert.Equal(25.5, model.P95Fps);
    }

    [Fact]
    public void ScreenVideoTelemetryModel_AllMetrics_CanBeSet()
    {
        // Arrange & Act
        var model = new ScreenVideoTelemetryModel
        {
            AvgFps = 30.0,
            MaxFps = 60.0,
            MinFps = 15.0,
            P95Fps = 28.0
        };

        // Assert
        Assert.Equal(30.0, model.AvgFps);
        Assert.Equal(60.0, model.MaxFps);
        Assert.Equal(15.0, model.MinFps);
        Assert.Equal(28.0, model.P95Fps);
    }
}
