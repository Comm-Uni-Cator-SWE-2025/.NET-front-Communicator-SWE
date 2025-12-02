using System;
using Communicator.App.Services;
using Communicator.UX.Core.Services;
using Xunit;

namespace Communicator.App.Tests.Unit.Services;

public sealed class RpcEventServiceTests
{
    [Fact]
    public void TriggerFrameReceivedRaisesEvent()
    {
        var service = new RpcEventService();
        byte[] data = [1, 2, 3];
        RpcDataEventArgs? args = null;

        service.FrameReceived += (s, e) => args = e;

        service.TriggerFrameReceived(data);

        Assert.NotNull(args);
        Assert.Equal(data, args.Data);
    }

    [Fact]
    public void TriggerParticipantsListUpdatedRaisesEvent()
    {
        var service = new RpcEventService();
        string json = "{}";
        RpcStringEventArgs? args = null;

        service.ParticipantsListUpdated += (s, e) => args = e;

        service.TriggerParticipantsListUpdated(json);

        Assert.NotNull(args);
        Assert.Equal(json, args.Value);
    }

    [Fact]
    public void TriggerStopShareReceivedRaisesEvent()
    {
        var service = new RpcEventService();
        byte[] data = [4, 5, 6];
        RpcDataEventArgs? args = null;

        service.StopShareReceived += (s, e) => args = e;

        service.TriggerStopShareReceived(data);

        Assert.NotNull(args);
        Assert.Equal(data, args.Data);
    }

    [Fact]
    public void TriggerLogoutRaisesEvent()
    {
        var service = new RpcEventService();
        string message = "User logged out";
        RpcStringEventArgs? args = null;

        service.Logout += (s, e) => args = e;

        service.TriggerLogout(message);

        Assert.NotNull(args);
        Assert.Equal(message, args.Value);
    }

    [Fact]
    public void TriggerEndMeetingRaisesEvent()
    {
        var service = new RpcEventService();
        string message = "Meeting ended";
        RpcStringEventArgs? args = null;

        service.EndMeeting += (s, e) => args = e;

        service.TriggerEndMeeting(message);

        Assert.NotNull(args);
        Assert.Equal(message, args.Value);
    }

    [Fact]
    public void TriggerChatMessageReceivedRaisesEvent()
    {
        var service = new RpcEventService();
        byte[] data = [7, 8, 9];
        RpcDataEventArgs? args = null;

        service.ChatMessageReceived += (s, e) => args = e;

        service.TriggerChatMessageReceived(data);

        Assert.NotNull(args);
        Assert.Equal(data, args.Data);
    }

    [Fact]
    public void TriggerFileMetadataReceivedRaisesEvent()
    {
        var service = new RpcEventService();
        byte[] data = [10, 11, 12];
        RpcDataEventArgs? args = null;

        service.FileMetadataReceived += (s, e) => args = e;

        service.TriggerFileMetadataReceived(data);

        Assert.NotNull(args);
        Assert.Equal(data, args.Data);
    }

    [Fact]
    public void TriggerFileSaveSuccessRaisesEvent()
    {
        var service = new RpcEventService();
        byte[] data = [13, 14, 15];
        RpcDataEventArgs? args = null;

        service.FileSaveSuccess += (s, e) => args = e;

        service.TriggerFileSaveSuccess(data);

        Assert.NotNull(args);
        Assert.Equal(data, args.Data);
    }

    [Fact]
    public void TriggerFileSaveErrorRaisesEvent()
    {
        var service = new RpcEventService();
        byte[] data = [16, 17, 18];
        RpcDataEventArgs? args = null;

        service.FileSaveError += (s, e) => args = e;

        service.TriggerFileSaveError(data);

        Assert.NotNull(args);
        Assert.Equal(data, args.Data);
    }

    [Fact]
    public void TriggerMessageDeletedRaisesEvent()
    {
        var service = new RpcEventService();
        byte[] data = [19, 20, 21];
        RpcDataEventArgs? args = null;

        service.MessageDeleted += (s, e) => args = e;

        service.TriggerMessageDeleted(data);

        Assert.NotNull(args);
        Assert.Equal(data, args.Data);
    }

    [Fact]
    public void TriggerCanvasUpdateReceivedRaisesEvent()
    {
        var service = new RpcEventService();
        byte[] data = [22, 23, 24];
        RpcDataEventArgs? args = null;

        service.CanvasUpdateReceived += (s, e) => args = e;

        service.TriggerCanvasUpdateReceived(data);

        Assert.NotNull(args);
        Assert.Equal(data, args.Data);
    }

    [Fact]
    public void TriggerCanvasAnalyticsUpdateReceivedRaisesEvent()
    {
        var service = new RpcEventService();
        byte[] data = [25, 26, 27];
        RpcDataEventArgs? args = null;

        service.CanvasAnalyticsUpdateReceived += (s, e) => args = e;

        service.TriggerCanvasAnalyticsUpdateReceived(data);

        Assert.NotNull(args);
        Assert.Equal(data, args.Data);
    }

    [Fact]
    public void TriggerWithNoSubscribersDoesNotThrow()
    {
        var service = new RpcEventService();

        // Should not throw even without subscribers
        service.TriggerFrameReceived([1]);
        service.TriggerStopShareReceived([1]);
        service.TriggerParticipantsListUpdated("{}");
        service.TriggerLogout("logout");
        service.TriggerEndMeeting("end");
        service.TriggerChatMessageReceived([1]);
        service.TriggerFileMetadataReceived([1]);
        service.TriggerFileSaveSuccess([1]);
        service.TriggerFileSaveError([1]);
        service.TriggerMessageDeleted([1]);
        service.TriggerCanvasUpdateReceived([1]);
        service.TriggerCanvasAnalyticsUpdateReceived([1]);
    }
}
