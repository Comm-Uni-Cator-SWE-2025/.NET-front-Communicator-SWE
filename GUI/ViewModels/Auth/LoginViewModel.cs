using GUI.Core;
using System.Windows.Input;

namespace GUI.ViewModels.Auth
{
    /// <summary>
    /// Handles login flow, including validation and signaling the parent auth view model when login succeeds.
    /// </summary>
    public class LoginViewModel : ObservableObject
    {
        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand GoToSignUpCommand { get; }

        private readonly AuthViewModel _authViewModel;

        /// <summary>
        /// Creates the login view model and initializes commands.
        /// </summary>
        public LoginViewModel(AuthViewModel authViewModel)
        {
            _authViewModel = authViewModel;
            _email = string.Empty;
            _password = string.Empty;
            LoginCommand = new RelayCommand(Login);
            GoToSignUpCommand = new RelayCommand(GoToSignUp);
        }

        /// <summary>
        /// Attempts to authenticate the user and produces toast feedback for success or failure.
        /// </summary>
        private void Login(object? obj)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(Email))
            {
                App.ToastService.ShowError("Please enter your email address");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                App.ToastService.ShowError("Please enter your password");
                return;
            }

            // Attempt login
            if (App.Controller?.Login(Email, Password) ?? false)
            {
                var user = App.Controller?.GetUser();
                if (user != null)
                {
                    App.ToastService.ShowSuccess($"Welcome back, {user.DisplayName}!");
                    _authViewModel.OnLoggedIn(user);
                }
            }
            else
            {
                App.ToastService.ShowError("Invalid email or password. Please try again.");
            }
        }

        /// <summary>
        /// Instructs the auth view model to show the sign-up view.
        /// </summary>
        private void GoToSignUp(object? obj)
        {
            _authViewModel.SwitchViewCommand.Execute(null);
        }
    }
}
