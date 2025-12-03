using System;
using Communicator.App.Services;
using Xunit;

namespace Communicator.App.Tests.Unit.Services;

public sealed class CloudMessageEventArgsTests
{
    [Fact]
    public void DefaultConstructorSetsEmptyDefaults()
    {
        var args = new CloudMessageEventArgs();

        Assert.Equal(string.Empty, args.SenderName);
        Assert.Equal(string.Empty, args.Message);
        Assert.Equal(default(CloudMessageType), args.MessageType);
    }

    [Fact]
    public void CanSetMessageType()
    {
        var args = new CloudMessageEventArgs {
            MessageType = CloudMessageType.QuickDoubt
        };

        Assert.Equal(CloudMessageType.QuickDoubt, args.MessageType);
    }

    [Fact]
    public void CanSetSenderName()
    {
        var args = new CloudMessageEventArgs {
            SenderName = "TestUser"
        };

        Assert.Equal("TestUser", args.SenderName);
    }

    [Fact]
    public void CanSetMessage()
    {
        var args = new CloudMessageEventArgs {
            Message = "Hello World"
        };

        Assert.Equal("Hello World", args.Message);
    }

    [Fact]
    public void CanSetAllProperties()
    {
        var args = new CloudMessageEventArgs {
            MessageType = CloudMessageType.QuickDoubt,
            SenderName = "User1",
            Message = "Test message"
        };

        Assert.Equal(CloudMessageType.QuickDoubt, args.MessageType);
        Assert.Equal("User1", args.SenderName);
        Assert.Equal("Test message", args.Message);
    }

    [Fact]
    public void InheritsFromEventArgs()
    {
        var args = new CloudMessageEventArgs();
        Assert.IsAssignableFrom<EventArgs>(args);
    }
}
