using Communicator.UX.Analytics.Models;

namespace Communicator.UX.Tests.AnalyticsUnit;

/// <summary>
/// Unit tests for the AIMessageData model class.
/// </summary>
public class AIMessageDataTests
{
    [Fact]
    public void AIMessageData_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var data = new AIMessageData();

        // Assert
        Assert.Equal(default(DateTime), data.Time);
        Assert.Equal(string.Empty, data.Message);
    }

    [Fact]
    public void AIMessageData_TimeProperty_CanBeSet()
    {
        // Arrange
        var data = new AIMessageData();
        DateTime expectedTime = new DateTime(2025, 12, 3, 10, 30, 0);

        // Act
        data.Time = expectedTime;

        // Assert
        Assert.Equal(expectedTime, data.Time);
    }

    [Fact]
    public void AIMessageData_MessageProperty_CanBeSet()
    {
        // Arrange
        var data = new AIMessageData();
        string expectedMessage = "Test action item message";

        // Act
        data.Message = expectedMessage;

        // Assert
        Assert.Equal(expectedMessage, data.Message);
    }

    [Fact]
    public void AIMessageData_CanBeCreatedWithObjectInitializer()
    {
        // Arrange
        DateTime expectedTime = DateTime.Now;
        string expectedMessage = "Schedule follow-up meeting";

        // Act
        var data = new AIMessageData
        {
            Time = expectedTime,
            Message = expectedMessage
        };

        // Assert
        Assert.Equal(expectedTime, data.Time);
        Assert.Equal(expectedMessage, data.Message);
    }

    [Fact]
    public void AIMessageData_EmptyMessage_IsAllowed()
    {
        // Arrange & Act
        var data = new AIMessageData { Message = string.Empty };

        // Assert
        Assert.Equal(string.Empty, data.Message);
    }

    [Fact]
    public void AIMessageData_LongMessage_IsAllowed()
    {
        // Arrange
        string longMessage = new string('A', 1000);

        // Act
        var data = new AIMessageData { Message = longMessage };

        // Assert
        Assert.Equal(1000, data.Message.Length);
    }

    [Fact]
    public void AIMessageData_SpecialCharactersInMessage_ArePreserved()
    {
        // Arrange
        string specialMessage = "Action: Review @team's code & submit PR #123!";

        // Act
        var data = new AIMessageData { Message = specialMessage };

        // Assert
        Assert.Equal(specialMessage, data.Message);
    }

    [Fact]
    public void AIMessageData_TimeWithMilliseconds_IsPreserved()
    {
        // Arrange
        DateTime timeWithMs = new DateTime(2025, 12, 3, 10, 30, 45, 123);

        // Act
        var data = new AIMessageData { Time = timeWithMs };

        // Assert
        Assert.Equal(123, data.Time.Millisecond);
    }
}
