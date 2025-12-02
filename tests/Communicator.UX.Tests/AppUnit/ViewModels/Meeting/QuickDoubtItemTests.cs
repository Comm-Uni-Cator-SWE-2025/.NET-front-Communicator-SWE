using System;
using Communicator.App.ViewModels.Meeting;
using Xunit;

namespace Communicator.App.Tests.Unit.ViewModels.Meeting;

public sealed class QuickDoubtItemTests
{
    [Fact]
    public void DefaultConstructorSetsDefaultValues()
    {
        QuickDoubtItem item = new QuickDoubtItem();

        Assert.Equal(string.Empty, item.Id);
        Assert.Equal(string.Empty, item.SenderName);
        Assert.Equal(string.Empty, item.Message);
        Assert.Equal(default(DateTime), item.Timestamp);
    }

    [Fact]
    public void IdPropertyCanBeSetAndRetrieved()
    {
        QuickDoubtItem item = new QuickDoubtItem();

        item.Id = "test-id-123";

        Assert.Equal("test-id-123", item.Id);
    }

    [Fact]
    public void SenderNamePropertyCanBeSetAndRetrieved()
    {
        QuickDoubtItem item = new QuickDoubtItem();

        item.SenderName = "John Doe";

        Assert.Equal("John Doe", item.SenderName);
    }

    [Fact]
    public void MessagePropertyCanBeSetAndRetrieved()
    {
        QuickDoubtItem item = new QuickDoubtItem();

        item.Message = "What is the answer to question 5?";

        Assert.Equal("What is the answer to question 5?", item.Message);
    }

    [Fact]
    public void TimestampPropertyCanBeSetAndRetrieved()
    {
        QuickDoubtItem item = new QuickDoubtItem();
        DateTime now = DateTime.Now;

        item.Timestamp = now;

        Assert.Equal(now, item.Timestamp);
    }

    [Fact]
    public void AllPropertiesCanBeSetViaObjectInitializer()
    {
        DateTime timestamp = new DateTime(2025, 12, 2, 10, 30, 0);

        QuickDoubtItem item = new QuickDoubtItem {
            Id = "doubt-001",
            SenderName = "Alice",
            Message = "Can you explain this concept?",
            Timestamp = timestamp
        };

        Assert.Equal("doubt-001", item.Id);
        Assert.Equal("Alice", item.SenderName);
        Assert.Equal("Can you explain this concept?", item.Message);
        Assert.Equal(timestamp, item.Timestamp);
    }

    [Fact]
    public void PropertiesCanBeModifiedAfterCreation()
    {
        QuickDoubtItem item = new QuickDoubtItem {
            Id = "original-id",
            SenderName = "Original Name",
            Message = "Original Message",
            Timestamp = DateTime.MinValue
        };

        item.Id = "modified-id";
        item.SenderName = "Modified Name";
        item.Message = "Modified Message";
        item.Timestamp = DateTime.MaxValue;

        Assert.Equal("modified-id", item.Id);
        Assert.Equal("Modified Name", item.SenderName);
        Assert.Equal("Modified Message", item.Message);
        Assert.Equal(DateTime.MaxValue, item.Timestamp);
    }

    [Fact]
    public void PropertiesAcceptNullValues()
    {
        QuickDoubtItem item = new QuickDoubtItem {
            Id = "test",
            SenderName = "test",
            Message = "test"
        };

        // These properties are string type with = string.Empty default
        // Setting to null should work as strings are nullable reference types
        item.Id = null!;
        item.SenderName = null!;
        item.Message = null!;

        Assert.Null(item.Id);
        Assert.Null(item.SenderName);
        Assert.Null(item.Message);
    }
}
