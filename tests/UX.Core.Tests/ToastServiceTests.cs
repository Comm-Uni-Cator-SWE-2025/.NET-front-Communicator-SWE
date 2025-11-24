using Communicator.Core.UX.Models;
using Communicator.Core.UX.Services;

namespace UX.Core.Tests;

public class ToastServiceTests
{
    [Fact]
    public void ShowSuccess_RaisesToastRequestedEvent_WithSuccessType()
    {
        var service = new ToastService();
        ToastMessage? receivedToast = null;

        service.ToastRequested += toast => receivedToast = toast;

        service.ShowSuccess("Success message");

        Assert.NotNull(receivedToast);
        Assert.Equal("Success message", receivedToast.Message);
        Assert.Equal(ToastType.Success, receivedToast.Type);
        Assert.Equal(3000, receivedToast.Duration);
    }

    [Fact]
    public void ShowError_RaisesToastRequestedEvent_WithErrorType()
    {
        var service = new ToastService();
        ToastMessage? receivedToast = null;

        service.ToastRequested += toast => receivedToast = toast;

        service.ShowError("Error message", 5000);

        Assert.NotNull(receivedToast);
        Assert.Equal("Error message", receivedToast.Message);
        Assert.Equal(ToastType.Error, receivedToast.Type);
        Assert.Equal(5000, receivedToast.Duration);
    }

    [Fact]
    public void ShowWarning_RaisesToastRequestedEvent_WithWarningType()
    {
        var service = new ToastService();
        ToastMessage? receivedToast = null;

        service.ToastRequested += toast => receivedToast = toast;

        service.ShowWarning("Warning message");

        Assert.NotNull(receivedToast);
        Assert.Equal("Warning message", receivedToast.Message);
        Assert.Equal(ToastType.Warning, receivedToast.Type);
    }

    [Fact]
    public void ShowInfo_RaisesToastRequestedEvent_WithInfoType()
    {
        var service = new ToastService();
        ToastMessage? receivedToast = null;

        service.ToastRequested += toast => receivedToast = toast;

        service.ShowInfo("Info message");

        Assert.NotNull(receivedToast);
        Assert.Equal("Info message", receivedToast.Message);
        Assert.Equal(ToastType.Info, receivedToast.Type);
    }

    [Fact]
    public void ToastMessage_HasUniqueId()
    {
        var toast1 = new ToastMessage("Message 1", ToastType.Success);
        var toast2 = new ToastMessage("Message 2", ToastType.Success);

        Assert.NotEqual(toast1.Id, toast2.Id);
    }
}
