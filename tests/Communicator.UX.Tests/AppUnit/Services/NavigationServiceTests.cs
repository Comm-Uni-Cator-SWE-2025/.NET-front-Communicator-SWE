using System;
using Communicator.App.Services;
using Xunit;

namespace Communicator.App.Tests.Unit.Services;

public sealed class NavigationServiceTests
{
    private readonly NavigationService _service;

    public NavigationServiceTests()
    {
        _service = new NavigationService();
    }

    [Fact]
    public void InitialStateHasNullCurrentView()
    {
        Assert.Null(_service.CurrentView);
        Assert.False(_service.CanGoBack);
    }

    [Fact]
    public void NavigateToSetsCurrentView()
    {
        object view1 = new object();
        bool eventRaised = false;
        _service.NavigationChanged += (s, e) => eventRaised = true;

        _service.NavigateTo(view1);

        Assert.Same(view1, _service.CurrentView);
        Assert.True(eventRaised);
        Assert.False(_service.CanGoBack);
    }

    [Fact]
    public void NavigateToPushesToBackStack()
    {
        object view1 = new object();
        object view2 = new object();

        _service.NavigateTo(view1);
        _service.NavigateTo(view2);

        Assert.Same(view2, _service.CurrentView);
        Assert.True(_service.CanGoBack);
    }

    [Fact]
    public void GoBackRestoresPreviousView()
    {
        object view1 = new object();
        object view2 = new object();

        _service.NavigateTo(view1);
        _service.NavigateTo(view2);

        _service.GoBack();

        Assert.Same(view1, _service.CurrentView);
        Assert.False(_service.CanGoBack);
    }

    [Fact]
    public void GoBackDoesNothingWhenNoHistory()
    {
        object view1 = new object();
        _service.NavigateTo(view1);

        // First GoBack clears the initial view
        Assert.False(_service.CanGoBack);

        // This should do nothing since there's no history
        _service.GoBack();

        Assert.Same(view1, _service.CurrentView);
    }

    [Fact]
    public void ClearHistoryClearsBackStack()
    {
        object view1 = new object();
        object view2 = new object();

        _service.NavigateTo(view1);
        _service.NavigateTo(view2);

        _service.ClearHistory();

        Assert.Same(view2, _service.CurrentView); // Current view remains
        Assert.False(_service.CanGoBack); // History is gone
    }

    [Fact]
    public void ClearHistoryDisposesDisposableViewModels()
    {
        DisposableViewModel disposableVm = new DisposableViewModel();
        object view2 = new object();

        _service.NavigateTo(disposableVm);
        _service.NavigateTo(view2);

        Assert.False(disposableVm.IsDisposed);

        _service.ClearHistory();

        Assert.True(disposableVm.IsDisposed);
    }

    [Fact]
    public void NavigationChangedRaisedOnEveryNavigation()
    {
        int eventCount = 0;
        _service.NavigationChanged += (s, e) => eventCount++;

        _service.NavigateTo(new object());
        _service.NavigateTo(new object());
        _service.GoBack();

        Assert.Equal(3, eventCount);
    }

    [Fact]
    public void MultipleNavigationsAndGoBacksWorkCorrectly()
    {
        object view1 = new object();
        object view2 = new object();
        object view3 = new object();

        _service.NavigateTo(view1);
        _service.NavigateTo(view2);
        _service.NavigateTo(view3);

        Assert.Same(view3, _service.CurrentView);
        Assert.True(_service.CanGoBack);

        _service.GoBack();
        Assert.Same(view2, _service.CurrentView);
        Assert.True(_service.CanGoBack);

        _service.GoBack();
        Assert.Same(view1, _service.CurrentView);
        Assert.False(_service.CanGoBack);
    }

    [Fact]
    public void ClearHistoryOnEmptyStackDoesNotThrow()
    {
        // Should not throw
        _service.ClearHistory();
        Assert.Null(_service.CurrentView);
    }

    [Fact]
    public void GoBackWhenCanGoBackIsFalseDoesNothing()
    {
        Assert.False(_service.CanGoBack);

        // Should not throw
        _service.GoBack();

        Assert.Null(_service.CurrentView);
    }

    private sealed class DisposableViewModel : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
