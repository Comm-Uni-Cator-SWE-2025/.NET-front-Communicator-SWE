using UX.Core;
using System;
using System.Windows.Input;
using Controller;

namespace GUI.ViewModels.Auth
{
    /// <summary>
    /// Coordinates login and sign-up sub workflows and notifies listeners upon successful authentication.
    /// </summary>
    public class AuthViewModel : ObservableObject
    {
        private bool _isLoginView = true;
        public bool IsLoginView
        {
            get { return _isLoginView; }
            set
            {
                _isLoginView = value;
                OnPropertyChanged();
            }
        }

        public LoginViewModel LoginViewModel { get; }
        public SignUpViewModel SignUpViewModel { get; }

        public ICommand SwitchViewCommand { get; }

        public event Action<UserProfile>? LoggedIn;

        /// <summary>
        /// Initializes child view models and the toggle command between login and registration.
        /// </summary>
        public AuthViewModel()
        {
            LoginViewModel = new LoginViewModel(this);
            SignUpViewModel = new SignUpViewModel(this);
            SwitchViewCommand = new RelayCommand(SwitchView);
        }

        /// <summary>
        /// Toggles between login and sign-up views.
        /// </summary>
        private void SwitchView(object? obj)
        {
            IsLoginView = !IsLoginView;
        }

        /// <summary>
        /// Raises the <see cref="LoggedIn"/> event with the authenticated user.
        /// </summary>
        public void OnLoggedIn(UserProfile user)
        {
            LoggedIn?.Invoke(user);
        }
    }
}
