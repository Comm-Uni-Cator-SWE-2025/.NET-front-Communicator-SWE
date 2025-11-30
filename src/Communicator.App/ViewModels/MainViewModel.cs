/*
 * -----------------------------------------------------------------------------
 *  File: MainViewModel.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.Windows.Input;
using Communicator.Controller.Meeting;
using Communicator.UX.Core;
using Communicator.UX.Core.Services;
using Communicator.App.Services;
using Communicator.App.ViewModels.Common;
using Communicator.App.ViewModels.Home;
using Communicator.App.ViewModels.Meeting;
using Communicator.App.ViewModels.Settings;

namespace Communicator.App.ViewModels;

/// <summary>
/// Shell view model that coordinates authentication, navigation, meeting toolbar state, and toast presentation.
/// Refactored to use Dependency Injection and AuthenticationService for better separation of concerns.
/// </summary>
public sealed class MainViewModel : ObservableObject
{
    private Communicator.App.ViewModels.Auth.AuthViewModel? _authViewModel;
    private readonly INavigationService _navigationService;
    private readonly IAuthenticationService _authenticationService;
    private readonly Func<Communicator.App.ViewModels.Auth.AuthViewModel> _authViewModelFactory;
    private readonly Func<UserProfile, HomePageViewModel> _homePageViewModelFactory;
    private readonly Func<UserProfile, SettingsViewModel> _settingsViewModelFactory;

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
    public bool ShowBackButton => !IsMeetingActive && _navigationService.CanGoBack;

    // Simplified: User info now comes from AuthenticationService
    public string? CurrentUserName => _authenticationService.CurrentUser?.DisplayName;
    public string? CurrentUserEmail => _authenticationService.CurrentUser?.Email;

    public ToastContainerViewModel ToastContainerViewModel { get; }
    public LoadingViewModel LoadingViewModel { get; }

    public bool IsBusy => LoadingViewModel.IsBusy;

    private INavigationScope? _navigationScope;
    private readonly RelayCommand _goBackCommand;

    public ICommand LogoutCommand { get; }
    public ICommand NavigateToSettingsCommand { get; }
    public ICommand GoBackCommand { get; }

    public bool CanGoBack => _navigationScope != null ? _navigationScope.CanNavigateBack : _navigationService.CanGoBack;

    /// <summary>
    /// Initializes commands, creates the initial authentication view, and subscribes to navigation events.
    /// Services are injected via constructor for better testability.
    /// </summary>
    public MainViewModel(
        INavigationService navigationService,
        IAuthenticationService authenticationService,
        ToastContainerViewModel toastContainerViewModel,
        LoadingViewModel loadingViewModel,
        Func<Communicator.App.ViewModels.Auth.AuthViewModel> authViewModelFactory,
        Func<UserProfile, HomePageViewModel> homePageViewModelFactory,
        Func<UserProfile, SettingsViewModel> settingsViewModelFactory)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        ToastContainerViewModel = toastContainerViewModel ?? throw new ArgumentNullException(nameof(toastContainerViewModel));
        LoadingViewModel = loadingViewModel ?? throw new ArgumentNullException(nameof(loadingViewModel));
        _authViewModelFactory = authViewModelFactory ?? throw new ArgumentNullException(nameof(authViewModelFactory));
        _homePageViewModelFactory = homePageViewModelFactory ?? throw new ArgumentNullException(nameof(homePageViewModelFactory));
        _settingsViewModelFactory = settingsViewModelFactory ?? throw new ArgumentNullException(nameof(settingsViewModelFactory));

        // Subscribe to loading state changes
        LoadingViewModel.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(LoadingViewModel.IsBusy))
            {
                OnPropertyChanged(nameof(IsBusy));
            }
        };

        // Subscribe to authentication events
        _authenticationService.UserLoggedOut += OnUserLoggedOut;

        _authViewModel = CreateAuthViewModel();
        LogoutCommand = new RelayCommand(Logout);
        NavigateToSettingsCommand = new RelayCommand(NavigateToSettings);
        _goBackCommand = new RelayCommand(_ => GoBack(), _ => CanGoBack);
        GoBackCommand = _goBackCommand;

        // Subscribe to navigation changes
        _navigationService.NavigationChanged += (s, e) => {
            CurrentView = _navigationService.CurrentView;
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(ShowBackButton));
            _goBackCommand.RaiseCanExecuteChanged();
        };

        CurrentView = _authViewModel;
    }

    /// <summary>
    /// Creates the authentication view model and hooks the post-login callback.
    /// Uses injected factory instead of service locator pattern.
    /// </summary>
    private Communicator.App.ViewModels.Auth.AuthViewModel CreateAuthViewModel()
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
        UserProfile? currentUser = _authenticationService.CurrentUser;
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
    /// Clears shell state and returns to the authentication view.
    /// Uses AuthenticationService to clear user session.
    /// </summary>
    private async void Logout(object? obj)
    {
        // Logout via authentication service
        // This will trigger OnUserLoggedOut via event
        // ConfigureAwait(false) to avoid deadlocks, but OnUserLoggedOut handles dispatching
        await _authenticationService.LogoutAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Handles user logout event from AuthenticationService.
    /// Resets the UI to the authentication screen.
    /// </summary>
    private void OnUserLoggedOut(object? sender, EventArgs e)
    {
        // Ensure UI updates happen on the UI thread
        System.Windows.Application.Current.Dispatcher.Invoke(() => {
            // Cleanup auth view model if it was attached
            if (_authViewModel != null)
            {
                _authViewModel.LoggedIn -= OnLoggedIn;
            }

            // Notify UI that user properties changed
            OnPropertyChanged(nameof(IsLoggedIn));
            OnPropertyChanged(nameof(CurrentUserName));
            OnPropertyChanged(nameof(CurrentUserEmail));

            // Clear navigation history and return to auth
            _navigationService.ClearHistory();
            _authViewModel = CreateAuthViewModel();
            CurrentView = _authViewModel;
        });
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
        OnPropertyChanged(nameof(ShowBackButton));
        _goBackCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Determines whether the meeting toolbar should be surfaced based on the active view model.
    /// </summary>
    private void UpdateTopBarState(object? viewModel)
    {
        MeetingToolbar = viewModel is MeetingSessionViewModel meetingSession
            ? meetingSession.Toolbar
            : null;
        OnPropertyChanged(nameof(ShowBackButton));
    }
}



