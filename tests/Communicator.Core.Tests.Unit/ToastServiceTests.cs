using System;
using Communicator.Core.UX.Models;
using Communicator.Core.UX.Services;
using Xunit;

namespace Communicator.Core.Tests.Unit;

public class ToastServiceTests
{
    [Fact]
    public void ShowSuccess_RaisesToastRequested_WithSuccessType()
    {
        var service = new ToastService();
        ToastMessage? receivedMessage = null;
        service.ToastRequested += (s, e) => receivedMessage = e.Message;

        service.ShowSuccess("Success!");

        Assert.NotNull(receivedMessage);
        Assert.Equal("Success!", receivedMessage.Message);
        Assert.Equal(ToastType.Success, receivedMessage.Type);
    }

    [Fact]
    public void ShowError_RaisesToastRequested_WithErrorType()
    {
        var service = new ToastService();
        ToastMessage? receivedMessage = null;
        service.ToastRequested += (s, e) => receivedMessage = e.Message;

        service.ShowError("Error!");

        Assert.NotNull(receivedMessage);
        Assert.Equal("Error!", receivedMessage.Message);
        Assert.Equal(ToastType.Error, receivedMessage.Type);
    }

    [Fact]
    public void ShowWarning_RaisesToastRequested_WithWarningType()
    {
        var service = new ToastService();
        ToastMessage? receivedMessage = null;
        service.ToastRequested += (s, e) => receivedMessage = e.Message;

        service.ShowWarning("Warning!");

        Assert.NotNull(receivedMessage);
        Assert.Equal("Warning!", receivedMessage.Message);
        Assert.Equal(ToastType.Warning, receivedMessage.Type);
    }

    [Fact]
    public void ShowInfo_RaisesToastRequested_WithInfoType()
    {
        var service = new ToastService();
        ToastMessage? receivedMessage = null;
        service.ToastRequested += (s, e) => receivedMessage = e.Message;

        service.ShowInfo("Info!");

        Assert.NotNull(receivedMessage);
        Assert.Equal("Info!", receivedMessage.Message);
        Assert.Equal(ToastType.Info, receivedMessage.Type);
    }
}
