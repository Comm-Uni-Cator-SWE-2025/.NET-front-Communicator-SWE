using System;
using System.Windows.Input;
using Controller;
using Communicator.UX.Services;
using Communicator.UX.ViewModels.Common;
using Communicator.UX.ViewModels.Home;
using Communicator.UX.ViewModels.Meeting;
using Communicator.UX.ViewModels.Settings;
using Communicator.Core.UX;
using Communicator.Core.UX.Services;

namespace Communicator.UX.ViewModels;

/// <summary>
/// Shell view model that coordinates authentication, navigation, meeting toolbar state, and toast presentation.
/// Refactored to use Dependency Injection and AuthenticationService for better separation of concerns.
/// </summary>
public class MainViewModel : ObservableObject
{
    private Communicator.UX.ViewModels.Auth.AuthViewModel? _authViewModel;
    private readonly INavigationService _navigationService;
    private readonly IAuthenticationService _authenticationService;
    private readonly Func<Communicator.UX.ViewModels.Auth.AuthViewModel> _authViewModelFactory;
    private readonly Func<User, HomePageViewModel> _homePageViewModelFactory;
    private readonly Func<User, SettingsViewModel> _settingsViewModelFactory;

    // Simplified: IsLoggedIn now comes from AuthenticationService
    public bool IsLoggedIn => _authenticationService.IsAuthenticated;

    private object? _currentView;
    public object? CurrentView
    {
        get => _currentView;
        set {
            _currentView = value;
            OnPropertyChanged();
            UpdateTopBarState(_currentView);
            UpdateNavigationScope(_currentView);
        }
    }

    private MeetingToolbarViewModel? _meetingToolbar;
    public MeetingToolbarViewModel? MeetingToolbar
    {
        get => _meetingToolbar;
        private set {
            if (SetProperty(ref _meetingToolbar, value))
            {
                OnPropertyChanged(nameof(IsMeetingActive));
            }
        }
    }

    public bool IsMeetingActive => MeetingToolbar != null;
    public bool ShowBackButton => IsMeetingActive || _navigationService.CanGoBack;
    public bool ShowForwardButton => IsMeetingActive;

    // Simplified: User info now comes from AuthenticationService
    public string? CurrentUserName => _authenticationService.CurrentUser?.DisplayName;
    public string? CurrentUserEmail => _authenticationService.CurrentUser?.Email;

    public ToastContainerViewModel ToastContainerViewModel { get; }

    private INavigationScope? _navigationScope;
    private readonly RelayCommand _goBackCommand;
    private readonly RelayCommand _goForwardCommand;

    public ICommand LogoutCommand { get; }
    public ICommand NavigateToSettingsCommand { get; }
    public ICommand GoBackCommand { get; }
    public ICommand GoForwardCommand { get; }

    public bool CanGoBack => _navigationScope != null ? _navigationScope.CanNavigateBack : _navigationService.CanGoBack;
    public bool CanGoForward => _navigationScope != null ? _navigationScope.CanNavigateForward : _navigationService.CanGoForward;

    /// <summary>
    /// Initializes commands, creates the initial authentication view, and subscribes to navigation events.
    /// Services are injected via constructor for better testability.
    /// </summary>
    public MainViewModel(
        INavigationService navigationService,
        IAuthenticationService authenticationService,
        ToastContainerViewModel toastContainerViewModel,
        Func<Communicator.UX.ViewModels.Auth.AuthViewModel> authViewModelFactory,
        Func<User, HomePageViewModel> homePageViewModelFactory,
        Func<User, SettingsViewModel> settingsViewModelFactory)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        ToastContainerViewModel = toastContainerViewModel ?? throw new ArgumentNullException(nameof(toastContainerViewModel));
        _authViewModelFactory = authViewModelFactory ?? throw new ArgumentNullException(nameof(authViewModelFactory));
        _homePageViewModelFactory = homePageViewModelFactory ?? throw new ArgumentNullException(nameof(homePageViewModelFactory));
        _settingsViewModelFactory = settingsViewModelFactory ?? throw new ArgumentNullException(nameof(settingsViewModelFactory));

        _authViewModel = CreateAuthViewModel();
        LogoutCommand = new RelayCommand(Logout);
        NavigateToSettingsCommand = new RelayCommand(NavigateToSettings);
        _goBackCommand = new RelayCommand(_ => GoBack(), _ => CanGoBack);
        _goForwardCommand = new RelayCommand(_ => GoForward(), _ => CanGoForward);
        GoBackCommand = _goBackCommand;
        GoForwardCommand = _goForwardCommand;

        // Subscribe to navigation changes
        _navigationService.NavigationChanged += (s, e) => {
            CurrentView = _navigationService.CurrentView;
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoForward));
            OnPropertyChanged(nameof(ShowBackButton));
            OnPropertyChanged(nameof(ShowForwardButton));
            _goBackCommand.RaiseCanExecuteChanged();
            _goForwardCommand.RaiseCanExecuteChanged();
        };

        CurrentView = _authViewModel;
    }

    /// <summary>
    /// Creates the authentication view model and hooks the post-login callback.
    /// Uses injected factory instead of service locator pattern.
    /// </summary>
    private Communicator.UX.ViewModels.Auth.AuthViewModel CreateAuthViewModel()
    {
        Auth.AuthViewModel authViewModel = _authViewModelFactory();
        authViewModel.LoggedIn += OnLoggedIn;
        return authViewModel;
    }

    /// <summary>
    /// Handles successful login by completing authentication and navigating to the home page.
    /// Uses AuthenticationService to manage user state.
    /// </summary>
    private void OnLoggedIn(object? sender, UserProfileEventArgs e)
    {
        if (_authViewModel != null)
        {
            _authViewModel.LoggedIn -= OnLoggedIn;
        }

        // Complete login via authentication service
        _authenticationService.CompleteLogin(e.User);

        // Notify UI that user properties changed
        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(CurrentUserName));
        OnPropertyChanged(nameof(CurrentUserEmail));

        // Clear navigation history and navigate to home
        _navigationService.ClearHistory();
        _navigationService.NavigateTo(_homePageViewModelFactory(e.User));
    }

    /// <summary>
    /// Navigates to the settings page when a user is authenticated.
    /// Uses injected factory to create SettingsViewModel.
    /// </summary>
    private void NavigateToSettings(object? obj)
    {
        User? currentUser = _authenticationService.CurrentUser;
        if (currentUser != null)
        {
            _navigationService.NavigateTo(_settingsViewModelFactory(currentUser));
        }
    }

    /// <summary>
    /// Executes back navigation against the active scope or the global navigation service.
    /// </summary>
    private void GoBack()
    {
        if (_navigationScope != null)
        {
            if (_navigationScope.CanNavigateBack)
            {
                _navigationScope.NavigateBack();
            }
            return;
        }

        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
        }
    }

    /// <summary>
    /// Executes forward navigation against the active scope or the global navigation service.
    /// </summary>
    private void GoForward()
    {
        if (_navigationScope != null)
        {
            if (_navigationScope.CanNavigateForward)
            {
                _navigationScope.NavigateForward();
            }
            return;
        }

        if (_navigationService.CanGoForward)
        {
            _navigationService.GoForward();
        }
    }

    /// <summary>
    /// Clears shell state and returns to the authentication view.
    /// Uses AuthenticationService to clear user session.
    /// </summary>
    private void Logout(object? obj)
    {
        // Cleanup auth view model
        if (_authViewModel != null)
        {
            _authViewModel.LoggedIn -= OnLoggedIn;
        }

        // Logout via authentication service
        _authenticationService.Logout();

        // Notify UI that user properties changed
        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(CurrentUserName));
        OnPropertyChanged(nameof(CurrentUserEmail));

        // Clear navigation history and return to auth
        _navigationService.ClearHistory();
        _authViewModel = CreateAuthViewModel();
        CurrentView = _authViewModel;
    }

    /// <summary>
    /// Updates the current navigation scope, wiring/unwiring events as necessary.
    /// </summary>
    private void UpdateNavigationScope(object? viewModel)
    {
        if (_navigationScope != null)
        {
            _navigationScope.NavigationStateChanged -= OnNavigationScopeStateChanged;
            if (_navigationScope is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _navigationScope = viewModel as INavigationScope;

        if (_navigationScope != null)
        {
            _navigationScope.NavigationStateChanged += OnNavigationScopeStateChanged;
        }

        OnNavigationScopeStateChanged(this, EventArgs.Empty);
    }

    /// <summary>
    /// Refreshes navigation-related properties and command states when scope navigation changes.
    /// </summary>
    private void OnNavigationScopeStateChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoForward));
        OnPropertyChanged(nameof(ShowBackButton));
        OnPropertyChanged(nameof(ShowForwardButton));
        _goBackCommand.RaiseCanExecuteChanged();
        _goForwardCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Determines whether the meeting toolbar should be surfaced based on the active view model.
    /// </summary>
    private void UpdateTopBarState(object? viewModel)
    {
        MeetingToolbar = viewModel is MeetingShellViewModel meetingShell
            ? meetingShell.Toolbar
            : null;
        OnPropertyChanged(nameof(ShowBackButton));
        OnPropertyChanged(nameof(ShowForwardButton));
    }
}

