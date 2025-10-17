using System;
using System.Windows.Input;
using Controller;
using UX.Core;
using UX.Core.Services;

namespace Controller.UI.ViewModels
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
        private readonly IController _controller;
        private readonly IToastService _toastService;

        /// <summary>
        /// Creates the login view model and initializes commands.
        /// </summary>
        public LoginViewModel(AuthViewModel authViewModel, IController controller, IToastService toastService)
        {
            _authViewModel = authViewModel ?? throw new ArgumentNullException(nameof(authViewModel));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));

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
                _toastService.ShowError("Please enter your email address");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                _toastService.ShowError("Please enter your password");
                return;
            }

            // Attempt login
            if (_controller.Login(Email, Password))
            {
                var user = _controller.GetUser();
                if (user != null)
                {
                    _toastService.ShowSuccess($"Welcome back, {user.DisplayName}!");
                    _authViewModel.OnLoggedIn(user);
                }
            }
            else
            {
                _toastService.ShowError("Invalid email or password. Please try again.");
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
