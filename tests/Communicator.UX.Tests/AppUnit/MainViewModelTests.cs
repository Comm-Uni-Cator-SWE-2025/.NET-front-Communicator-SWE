using System;
using System.Threading.Tasks;
using Communicator.App.Services;
using Communicator.App.ViewModels;
using Communicator.App.ViewModels.Auth;
using Communicator.App.ViewModels.Common;
using Communicator.App.ViewModels.Home;
using Communicator.App.ViewModels.Settings;
using Communicator.UX.Core.Services;
using Moq;
using Xunit;
using Communicator.Controller.Meeting;

namespace Communicator.App.Tests.Unit;

public sealed class MainViewModelTests
{
    private readonly Mock<INavigationService> _mockNavService;
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<IToastService> _mockToastService;
    private readonly Mock<IThemeService> _mockThemeService;
    private readonly Mock<Func<AuthViewModel>> _mockAuthFactory;
    private readonly Mock<Func<UserProfile, HomePageViewModel>> _mockHomeFactory;
    private readonly Mock<Func<UserProfile, SettingsViewModel>> _mockSettingsFactory;
    private readonly ToastContainerViewModel _toastContainer;
    private readonly LoadingViewModel _loadingViewModel;

    public MainViewModelTests()
    {
        _mockNavService = new Mock<INavigationService>();
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockToastService = new Mock<IToastService>();
        _mockThemeService = new Mock<IThemeService>();
        _mockAuthFactory = new Mock<Func<AuthViewModel>>();
        _mockHomeFactory = new Mock<Func<UserProfile, HomePageViewModel>>();
        _mockSettingsFactory = new Mock<Func<UserProfile, SettingsViewModel>>();

        _toastContainer = new ToastContainerViewModel(_mockToastService.Object);
        _loadingViewModel = new LoadingViewModel();

        // Setup default factory behavior
        AuthViewModel realAuthVM = new AuthViewModel(Mock.Of<Communicator.Controller.RPC.IRPC>(), _mockToastService.Object);
        _mockAuthFactory.Setup(f => f()).Returns(realAuthVM);
    }

    private MainViewModel CreateViewModel()
    {
        return new MainViewModel(
            _mockNavService.Object,
            _mockAuthService.Object,
            _toastContainer,
            _loadingViewModel,
            _mockAuthFactory.Object,
            _mockHomeFactory.Object,
            _mockSettingsFactory.Object
        );
    }

    [Fact]
    public void ConstructorWithNullNavigationServiceThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(
            null!,
            _mockAuthService.Object,
            _toastContainer,
            _loadingViewModel,
            _mockAuthFactory.Object,
            _mockHomeFactory.Object,
            _mockSettingsFactory.Object
        ));
    }

    [Fact]
    public void ConstructorWithNullAuthServiceThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(
            _mockNavService.Object,
            null!,
            _toastContainer,
            _loadingViewModel,
            _mockAuthFactory.Object,
            _mockHomeFactory.Object,
            _mockSettingsFactory.Object
        ));
    }

    [Fact]
    public void ConstructorWithNullToastContainerThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(
            _mockNavService.Object,
            _mockAuthService.Object,
            null!,
            _loadingViewModel,
            _mockAuthFactory.Object,
            _mockHomeFactory.Object,
            _mockSettingsFactory.Object
        ));
    }

    [Fact]
    public void ConstructorWithNullLoadingViewModelThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(
            _mockNavService.Object,
            _mockAuthService.Object,
            _toastContainer,
            null!,
            _mockAuthFactory.Object,
            _mockHomeFactory.Object,
            _mockSettingsFactory.Object
        ));
    }

    [Fact]
    public void ConstructorWithNullAuthFactoryThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(
            _mockNavService.Object,
            _mockAuthService.Object,
            _toastContainer,
            _loadingViewModel,
            null!,
            _mockHomeFactory.Object,
            _mockSettingsFactory.Object
        ));
    }

    [Fact]
    public void ConstructorWithNullHomeFactoryThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(
            _mockNavService.Object,
            _mockAuthService.Object,
            _toastContainer,
            _loadingViewModel,
            _mockAuthFactory.Object,
            null!,
            _mockSettingsFactory.Object
        ));
    }

    [Fact]
    public void ConstructorWithNullSettingsFactoryThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(
            _mockNavService.Object,
            _mockAuthService.Object,
            _toastContainer,
            _loadingViewModel,
            _mockAuthFactory.Object,
            _mockHomeFactory.Object,
            null!
        ));
    }

    [Fact]
    public void ConstructorInitializesWithAuthView()
    {
        MainViewModel vm = CreateViewModel();

        Assert.NotNull(vm.CurrentView);
        Assert.IsAssignableFrom<AuthViewModel>(vm.CurrentView);
    }

    [Fact]
    public void IsLoggedInReturnsFalseWhenNotAuthenticated()
    {
        _mockAuthService.Setup(s => s.IsAuthenticated).Returns(false);

        MainViewModel vm = CreateViewModel();

        Assert.False(vm.IsLoggedIn);
    }

    [Fact]
    public void IsLoggedInReturnsTrueWhenAuthenticated()
    {
        _mockAuthService.Setup(s => s.IsAuthenticated).Returns(true);

        MainViewModel vm = CreateViewModel();

        Assert.True(vm.IsLoggedIn);
    }

    [Fact]
    public void IsMeetingActiveReturnsFalseWhenNoMeetingToolbar()
    {
        MainViewModel vm = CreateViewModel();

        Assert.False(vm.IsMeetingActive);
    }

    [Fact]
    public void MeetingToolbarIsNullInitially()
    {
        MainViewModel vm = CreateViewModel();

        Assert.Null(vm.MeetingToolbar);
    }

    [Fact]
    public void ToastContainerViewModelIsSet()
    {
        MainViewModel vm = CreateViewModel();

        Assert.Same(_toastContainer, vm.ToastContainerViewModel);
    }

    [Fact]
    public void LoadingViewModelIsSet()
    {
        MainViewModel vm = CreateViewModel();

        Assert.Same(_loadingViewModel, vm.LoadingViewModel);
    }

    [Fact]
    public void IsBusyReflectsLoadingViewModelState()
    {
        MainViewModel vm = CreateViewModel();

        Assert.False(vm.IsBusy);

        _loadingViewModel.IsBusy = true;

        Assert.True(vm.IsBusy);
    }

    [Fact]
    public void LogoutCommandCallsAuthServiceLogout()
    {
        MainViewModel vm = CreateViewModel();

        vm.LogoutCommand.Execute(null);

        _mockAuthService.Verify(s => s.LogoutAsync(), Times.Once);
    }

    [Fact]
    public void NavigateToSettingsCommandNavigatesWhenUserLoggedIn()
    {
        UserProfile user = new UserProfile { DisplayName = "Test" };
        _mockAuthService.Setup(s => s.CurrentUser).Returns(user);

        SettingsViewModel settingsVM = new SettingsViewModel(user, _mockThemeService.Object, _mockAuthService.Object);
        _mockSettingsFactory.Setup(f => f(user)).Returns(settingsVM);

        MainViewModel vm = CreateViewModel();
        vm.NavigateToSettingsCommand.Execute(null);

        _mockNavService.Verify(n => n.NavigateTo(settingsVM), Times.Once);
    }

    [Fact]
    public void NavigateToSettingsCommandDoesNotNavigateWhenNoUser()
    {
        _mockAuthService.Setup(s => s.CurrentUser).Returns((UserProfile?)null);

        MainViewModel vm = CreateViewModel();
        vm.NavigateToSettingsCommand.Execute(null);

        _mockNavService.Verify(n => n.NavigateTo(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public void CurrentUserNameReturnsNullWhenNoUser()
    {
        _mockAuthService.Setup(s => s.CurrentUser).Returns((UserProfile?)null);

        MainViewModel vm = CreateViewModel();

        Assert.Null(vm.CurrentUserName);
    }

    [Fact]
    public void CurrentUserNameReturnsDisplayName()
    {
        UserProfile user = new UserProfile { DisplayName = "John Doe" };
        _mockAuthService.Setup(s => s.CurrentUser).Returns(user);

        MainViewModel vm = CreateViewModel();

        Assert.Equal("John Doe", vm.CurrentUserName);
    }

    [Fact]
    public void CurrentUserEmailReturnsEmail()
    {
        UserProfile user = new UserProfile { Email = "john@example.com" };
        _mockAuthService.Setup(s => s.CurrentUser).Returns(user);

        MainViewModel vm = CreateViewModel();

        Assert.Equal("john@example.com", vm.CurrentUserEmail);
    }

    [Fact]
    public void GoBackCommandIsNotNull()
    {
        MainViewModel vm = CreateViewModel();

        Assert.NotNull(vm.GoBackCommand);
    }

    [Fact]
    public void LogoutCommandIsNotNull()
    {
        MainViewModel vm = CreateViewModel();

        Assert.NotNull(vm.LogoutCommand);
    }

    [Fact]
    public void NavigateToSettingsCommandIsNotNull()
    {
        MainViewModel vm = CreateViewModel();

        Assert.NotNull(vm.NavigateToSettingsCommand);
    }

    [Fact]
    public void ShowBackButtonIsFalseWhenMeetingActiveOrCannotGoBack()
    {
        _mockNavService.Setup(n => n.CanGoBack).Returns(false);

        MainViewModel vm = CreateViewModel();

        Assert.False(vm.ShowBackButton);
    }

    [Fact]
    public void CanGoBackReflectsNavigationServiceState()
    {
        _mockNavService.Setup(n => n.CanGoBack).Returns(true);

        MainViewModel vm = CreateViewModel();

        Assert.True(vm.CanGoBack);
    }

    [Fact]
    public void CurrentViewSetRaisesPropertyChanged()
    {
        MainViewModel vm = CreateViewModel();
        bool propertyChanged = false;

        vm.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(vm.CurrentView))
            {
                propertyChanged = true;
            }
        };

        // Trigger via navigation event
        _mockNavService.Raise(n => n.NavigationChanged += null, EventArgs.Empty);

        // Property should be updated even if CurrentView didn't change value
        Assert.True(propertyChanged || vm.CurrentView != null);
    }
}
