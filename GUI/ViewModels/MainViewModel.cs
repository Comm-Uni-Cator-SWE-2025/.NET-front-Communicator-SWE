using System;
using System.Windows.Input;
using Controller;
using GUI.Core;

namespace GUI.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private AuthViewModel? _authViewModel;
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
    public bool ShowBackButton => IsMeetingActive || App.NavigationService.CanGoBack;
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

    public bool CanGoBack => _navigationScope != null ? _navigationScope.CanNavigateBack : App.NavigationService.CanGoBack;
    public bool CanGoForward => _navigationScope != null ? _navigationScope.CanNavigateForward : App.NavigationService.CanGoForward;

        public MainViewModel()
        {
            _authViewModel = CreateAuthViewModel();
            LogoutCommand = new RelayCommand(Logout);
            NavigateToSettingsCommand = new RelayCommand(NavigateToSettings);
            _goBackCommand = new RelayCommand(_ => GoBack(), _ => CanGoBack);
            _goForwardCommand = new RelayCommand(_ => GoForward(), _ => CanGoForward);
            GoBackCommand = _goBackCommand;
            GoForwardCommand = _goForwardCommand;

            // Subscribe to navigation changes
            App.NavigationService.NavigationChanged += (s, e) =>
            {
                CurrentView = App.NavigationService.CurrentView;
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(CanGoForward));
                OnPropertyChanged(nameof(ShowBackButton));
                OnPropertyChanged(nameof(ShowForwardButton));
                _goBackCommand.RaiseCanExecuteChanged();
                _goForwardCommand.RaiseCanExecuteChanged();
            };

            CurrentView = _authViewModel;
        }

        private AuthViewModel CreateAuthViewModel()
        {
            var authViewModel = new AuthViewModel();
            authViewModel.LoggedIn += OnLoggedIn;
            return authViewModel;
        }

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
            App.NavigationService.ClearHistory();
            App.NavigationService.NavigateTo(new HomePageViewModel(user));
        }

        private void NavigateToSettings(object? obj)
        {
            if (CurrentUser != null)
            {
                var settingsViewModel = new SettingsViewModel(CurrentUser, App.ThemeService);
                App.NavigationService.NavigateTo(settingsViewModel);
            }
        }

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

            if (App.NavigationService.CanGoBack)
            {
                App.NavigationService.GoBack();
            }
        }

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

            if (App.NavigationService.CanGoForward)
            {
                App.NavigationService.GoForward();
            }
        }

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
            App.NavigationService.ClearHistory();
            _authViewModel = CreateAuthViewModel();
            CurrentView = _authViewModel;
        }

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

        private void OnNavigationScopeStateChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoForward));
            OnPropertyChanged(nameof(ShowBackButton));
            OnPropertyChanged(nameof(ShowForwardButton));
            _goBackCommand.RaiseCanExecuteChanged();
            _goForwardCommand.RaiseCanExecuteChanged();
        }

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
