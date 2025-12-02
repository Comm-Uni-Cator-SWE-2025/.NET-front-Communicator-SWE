using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Communicator.App.ViewModels.Auth;
using Communicator.Controller.Serialization;
using Communicator.Controller.RPC;
using Communicator.UX.Core.Services;
using Moq;
using Xunit;
using Communicator.Controller.Meeting;

namespace Communicator.App.Tests.Unit;

public sealed class AuthViewModelTests
{
    private readonly Mock<IRPC> _mockRpc;
    private readonly Mock<IToastService> _mockToastService;

    public AuthViewModelTests()
    {
        _mockRpc = new Mock<IRPC>();
        _mockToastService = new Mock<IToastService>();
    }

    [Fact]
    public void ConstructorWithNullRpcThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AuthViewModel(null!, _mockToastService.Object));
    }

    [Fact]
    public void ConstructorWithNullToastServiceThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AuthViewModel(_mockRpc.Object, null!));
    }

    [Fact]
    public void ConstructorInitializesDefaultState()
    {
        AuthViewModel vm = new AuthViewModel(_mockRpc.Object, _mockToastService.Object);

        Assert.False(vm.IsLoading);
        Assert.Equal(string.Empty, vm.ErrorMessage);
        Assert.Null(vm.CurrentUser);
    }

    [Fact]
    public void SignInWithGoogleCommandIsNotNull()
    {
        AuthViewModel vm = new AuthViewModel(_mockRpc.Object, _mockToastService.Object);

        Assert.NotNull(vm.SignInWithGoogleCommand);
    }

    [Fact]
    public void DebugLoginCommandIsNotNull()
    {
        AuthViewModel vm = new AuthViewModel(_mockRpc.Object, _mockToastService.Object);

        Assert.NotNull(vm.DebugLoginCommand);
    }

    [Fact]
    public void ResetClearsState()
    {
        AuthViewModel vm = new AuthViewModel(_mockRpc.Object, _mockToastService.Object);

        // Set some state manually using reflection or public setters
        vm.Reset();

        Assert.Empty(vm.ErrorMessage);
        Assert.False(vm.IsLoading);
        Assert.Null(vm.CurrentUser);
    }

    [Fact]
    public void IsLoadingPropertyChangesRaisePropertyChanged()
    {
        AuthViewModel vm = new AuthViewModel(_mockRpc.Object, _mockToastService.Object);
        bool propertyChanged = false;

        vm.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(vm.IsLoading))
            {
                propertyChanged = true;
            }
        };

        vm.IsLoading = true;

        Assert.True(propertyChanged);
        Assert.True(vm.IsLoading);
    }

    [Fact]
    public void ErrorMessagePropertyChangesRaisePropertyChanged()
    {
        AuthViewModel vm = new AuthViewModel(_mockRpc.Object, _mockToastService.Object);
        bool propertyChanged = false;

        vm.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(vm.ErrorMessage))
            {
                propertyChanged = true;
            }
        };

        vm.ErrorMessage = "Test Error";

        Assert.True(propertyChanged);
        Assert.Equal("Test Error", vm.ErrorMessage);
    }

    [Fact]
    public void CurrentUserSetRaisesLoggedInEvent()
    {
        AuthViewModel vm = new AuthViewModel(_mockRpc.Object, _mockToastService.Object);
        bool eventRaised = false;
        UserProfile? receivedUser = null;

        vm.LoggedIn += (s, e) => {
            eventRaised = true;
            receivedUser = e.User;
        };

        UserProfile user = new UserProfile {
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        vm.CurrentUser = user;

        Assert.True(eventRaised);
        Assert.Equal(user, receivedUser);
    }

    [Fact]
    public void CurrentUserSetToNullDoesNotRaiseEvent()
    {
        AuthViewModel vm = new AuthViewModel(_mockRpc.Object, _mockToastService.Object);
        bool eventRaised = false;

        vm.LoggedIn += (s, e) => eventRaised = true;

        vm.CurrentUser = null;

        Assert.False(eventRaised);
    }

    [Fact]
    public void IsDebugModeReturnsTrueInDebugBuild()
    {
#if DEBUG
        Assert.True(AuthViewModel.IsDebugMode);
#else
        Assert.False(AuthViewModel.IsDebugMode);
#endif
    }

    [Fact]
    public void SignInWithGoogleCommandCanExecuteReturnsTrueWhenNotLoading()
    {
        AuthViewModel vm = new AuthViewModel(_mockRpc.Object, _mockToastService.Object);

        Assert.True(vm.SignInWithGoogleCommand.CanExecute(null));
    }

    [Fact]
    public void SignInWithGoogleCommandCanExecuteReturnsFalseWhenLoading()
    {
        AuthViewModel vm = new AuthViewModel(_mockRpc.Object, _mockToastService.Object) {
            IsLoading = true
        };

        Assert.False(vm.SignInWithGoogleCommand.CanExecute(null));
    }

    [Fact]
    public void DebugLoginCommandCanExecuteReturnsTrueWhenNotLoading()
    {
        AuthViewModel vm = new AuthViewModel(_mockRpc.Object, _mockToastService.Object);

        Assert.True(vm.DebugLoginCommand.CanExecute(null));
    }

    [Fact]
    public void DebugLoginCommandCanExecuteReturnsFalseWhenLoading()
    {
        AuthViewModel vm = new AuthViewModel(_mockRpc.Object, _mockToastService.Object) {
            IsLoading = true
        };

        Assert.False(vm.DebugLoginCommand.CanExecute(null));
    }

    [Fact]
    public void ResetClearsAllState()
    {
        AuthViewModel vm = new AuthViewModel(_mockRpc.Object, _mockToastService.Object) {
            // Set state
            ErrorMessage = "Some error",
            IsLoading = true
        };

        // Reset
        vm.Reset();

        Assert.Empty(vm.ErrorMessage);
        Assert.False(vm.IsLoading);
        Assert.Null(vm.CurrentUser);
    }
}
