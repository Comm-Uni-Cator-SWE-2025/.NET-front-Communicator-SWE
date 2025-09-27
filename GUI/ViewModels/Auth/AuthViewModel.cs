using GUI.Core;
using System;
using System.Windows.Input;
using Controller;

namespace GUI.ViewModels.Auth
{
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

        public AuthViewModel()
        {
            LoginViewModel = new LoginViewModel(this);
            SignUpViewModel = new SignUpViewModel(this);
            SwitchViewCommand = new RelayCommand(SwitchView);
        }

        private void SwitchView(object? obj)
        {
            IsLoginView = !IsLoginView;
        }

        public void OnLoggedIn(UserProfile user)
        {
            LoggedIn?.Invoke(user);
        }
    }
}