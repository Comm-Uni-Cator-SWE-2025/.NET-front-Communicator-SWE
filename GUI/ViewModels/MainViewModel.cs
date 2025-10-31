using GUI.Core;
using System.Windows.Input;
using Controller;

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
            }
        }

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

        public ICommand LogoutCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand GoForwardCommand { get; }

        public bool CanGoBack => App.NavigationService.CanGoBack;
        public bool CanGoForward => App.NavigationService.CanGoForward;

        public MainViewModel()
        {
            _authViewModel = CreateAuthViewModel();
            CurrentView = _authViewModel;
            LogoutCommand = new RelayCommand(Logout);
            NavigateToSettingsCommand = new RelayCommand(NavigateToSettings);
            GoBackCommand = new RelayCommand(_ => GoBack(), _ => CanGoBack);
            GoForwardCommand = new RelayCommand(_ => GoForward(), _ => CanGoForward);

            // Subscribe to navigation changes
            App.NavigationService.NavigationChanged += (s, e) =>
            {
                CurrentView = App.NavigationService.CurrentView;
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(CanGoForward));
            };
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
                var backCommand = new RelayCommand(_ => GoBack());
                var settingsViewModel = new SettingsViewModel(CurrentUser, App.ThemeService, backCommand);
                App.NavigationService.NavigateTo(settingsViewModel);
            }
        }

        private void GoBack()
        {
            App.NavigationService.GoBack();
        }

        private void GoForward()
        {
            App.NavigationService.GoForward();
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
    }
}
