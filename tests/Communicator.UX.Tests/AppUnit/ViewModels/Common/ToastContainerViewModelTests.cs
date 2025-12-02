using System;
using System.Collections.ObjectModel;
using Communicator.App.ViewModels.Common;
using Communicator.UX.Core.Models;
using Communicator.UX.Core.Services;
using Moq;
using Xunit;

namespace Communicator.App.Tests.Unit.ViewModels.Common;

public sealed class ToastContainerViewModelTests
{
    [Fact]
    public void ConstructorInitializesToastsCollection()
    {
        Mock<IToastService> mockToastService = new Mock<IToastService>();

        ToastContainerViewModel vm = new ToastContainerViewModel(mockToastService.Object);

        Assert.NotNull(vm.Toasts);
        Assert.Empty(vm.Toasts);
    }

    [Fact]
    public void ConstructorSubscribesToToastRequested()
    {
        Mock<IToastService> mockToastService = new Mock<IToastService>();

        _ = new ToastContainerViewModel(mockToastService.Object);

        // Verify ToastRequested event is subscribed
        mockToastService.VerifyAdd(
            ts => ts.ToastRequested += It.IsAny<EventHandler<ToastRequestedEventArgs>>(),
            Times.Once);
    }

    [Fact]
    public void ToastsCollectionIsObservable()
    {
        Mock<IToastService> mockToastService = new Mock<IToastService>();
        ToastContainerViewModel vm = new ToastContainerViewModel(mockToastService.Object);

        Assert.IsType<ObservableCollection<ToastMessage>>(vm.Toasts);
    }

    // Note: Tests for OnToastRequested and RemoveToast require Application.Current.Dispatcher
    // which is only available in a WPF application context or STA thread.
    // These would need to be integration tests or use a test dispatcher.
}
