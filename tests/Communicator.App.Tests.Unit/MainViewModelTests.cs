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

public class MainViewModelTests
{
    private readonly Mock<INavigationService> _mockNavService;
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<IToastService> _mockToastService;
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
        _mockAuthFactory = new Mock<Func<AuthViewModel>>();
        _mockHomeFactory = new Mock<Func<UserProfile, HomePageViewModel>>();
        _mockSettingsFactory = new Mock<Func<UserProfile, SettingsViewModel>>();
        
        _toastContainer = new ToastContainerViewModel(_mockToastService.Object);
        _loadingViewModel = new LoadingViewModel();

        // Setup default factory behavior
        // AuthViewModel is sealed, so we use a real instance with mocked dependencies
        var realAuthVM = new AuthViewModel(Mock.Of<Communicator.Core.RPC.IRPC>(), _mockToastService.Object);
        _mockAuthFactory.Setup(f => f()).Returns(realAuthVM);
    }

    [Fact]
    public void Constructor_Initializes_WithAuthView()
    {
        var vm = CreateViewModel();
        Assert.NotNull(vm.CurrentView);
        Assert.IsAssignableFrom<AuthViewModel>(vm.CurrentView);
    }

    [Fact]
    public void LogoutCommand_Calls_AuthService_Logout()
    {
        var vm = CreateViewModel();
        vm.LogoutCommand.Execute(null);
        _mockAuthService.Verify(s => s.LogoutAsync(), Times.Once);
    }

    [Fact]
    public void NavigateToSettingsCommand_Navigates_IfUserLoggedIn()
    {
        var user = new UserProfile { DisplayName = "Test" };
        _mockAuthService.Setup(s => s.CurrentUser).Returns(user);
        
        // Mock settings VM
        var settingsVM = new SettingsViewModel(user, _mockThemeService.Object, _mockAuthService.Object);
        _mockSettingsFactory.Setup(f => f(user)).Returns(settingsVM);

        var vm = CreateViewModel();
        vm.NavigateToSettingsCommand.Execute(null);

        _mockNavService.Verify(n => n.NavigateTo(settingsVM), Times.Once);
    }

    private readonly Mock<IThemeService> _mockThemeService = new Mock<IThemeService>();

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
}
