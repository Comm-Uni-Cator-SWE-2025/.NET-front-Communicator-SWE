using System;
using System.Windows.Input;
using Controller;
using Microsoft.Extensions.DependencyInjection;
using UX.Core;
using UX.Core.Services;
using GUI.ViewModels.Common;
using GUI.ViewModels.Home;
using GUI.ViewModels.Meeting;
using GUI.ViewModels.Settings;

namespace GUI.ViewModels
{
    /// <summary>
    /// Shell view model that coordinates authentication, navigation, meeting toolbar state, and toast presentation.
    /// Refactored to use Dependency Injection for better testability and maintainability.
    /// </summary>
    public class MainViewModel : ObservableObject
    {
        private Controller.UI.ViewModels.AuthViewModel? _authViewModel;
        private readonly INavigationService _navigationService;
        private readonly IThemeService _themeService;
        private readonly IToastService _toastService;
        private readonly IController _controller;

        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set
            {
                _isLoggedIn = value;
                OnPropertyChanged();
            }
        }

        private object? _currentView;
        public object? CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
                UpdateTopBarState(_currentView);
                UpdateNavigationScope(_currentView);
            }
        }

        private MeetingToolbarViewModel? _meetingToolbar;
        public MeetingToolbarViewModel? MeetingToolbar
        {
            get { return _meetingToolbar; }
            private set
            {
                if (SetProperty(ref _meetingToolbar, value))
                {
                    OnPropertyChanged(nameof(IsMeetingActive));
                }
            }
        }

    public bool IsMeetingActive => MeetingToolbar != null;
    public bool ShowBackButton => IsMeetingActive || _navigationService.CanGoBack;
    public bool ShowForwardButton => IsMeetingActive;

        private string? _currentUserName;
        public string? CurrentUserName
        {
            get { return _currentUserName; }
            set
            {
                _currentUserName = value;
                OnPropertyChanged();
            }
        }

        private string? _currentUserEmail;
        public string? CurrentUserEmail
        {
            get { return _currentUserEmail; }
            set
            {
                _currentUserEmail = value;
                OnPropertyChanged();
            }
        }

        private UserProfile? _currentUser;
        private UserProfile? CurrentUser
        {
            get { return _currentUser; }
            set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        private ToastContainerViewModel? _toastContainerViewModel;
        public ToastContainerViewModel? ToastContainerViewModel
        {
            get { return _toastContainerViewModel; }
            set
            {
                _toastContainerViewModel = value;
                OnPropertyChanged();
            }
        }

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
            IThemeService themeService,
            IToastService toastService,
            IController controller)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));

            _authViewModel = CreateAuthViewModel();
            LogoutCommand = new RelayCommand(Logout);
            NavigateToSettingsCommand = new RelayCommand(NavigateToSettings);
            _goBackCommand = new RelayCommand(_ => GoBack(), _ => CanGoBack);
            _goForwardCommand = new RelayCommand(_ => GoForward(), _ => CanGoForward);
            GoBackCommand = _goBackCommand;
            GoForwardCommand = _goForwardCommand;

            // Subscribe to navigation changes
            _navigationService.NavigationChanged += (s, e) =>
            {
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
        /// </summary>
        private Controller.UI.ViewModels.AuthViewModel CreateAuthViewModel()
        {
            var authViewModel = App.Services.GetRequiredService<Controller.UI.ViewModels.AuthViewModel>();
            authViewModel.LoggedIn += OnLoggedIn;
            return authViewModel;
        }

        /// <summary>
        /// Handles successful login by capturing user details and navigating to the home page.
        /// </summary>
        private void OnLoggedIn(UserProfile user)
        {
            if (_authViewModel != null)
            {
                _authViewModel.LoggedIn -= OnLoggedIn;
            }

            IsLoggedIn = true;
            CurrentUserName = user.DisplayName;
            CurrentUserEmail = user.Email;
            CurrentUser = user;
            
            // Clear navigation history and navigate to home
            _navigationService.ClearHistory();
            _navigationService.NavigateTo(new HomePageViewModel(user, _toastService, _navigationService));
        }

        /// <summary>
        /// Navigates to the settings page when a user is authenticated.
        /// </summary>
        private void NavigateToSettings(object? obj)
        {
            if (CurrentUser != null)
            {
                var settingsViewModel = new SettingsViewModel(CurrentUser, _themeService);
                _navigationService.NavigateTo(settingsViewModel);
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
        /// </summary>
        private void Logout(object? obj)
        {
            IsLoggedIn = false;
            CurrentUserName = null;
            CurrentUserEmail = null;
            CurrentUser = null;
            if (_authViewModel != null)
            {
                _authViewModel.LoggedIn -= OnLoggedIn;
            }

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
}

