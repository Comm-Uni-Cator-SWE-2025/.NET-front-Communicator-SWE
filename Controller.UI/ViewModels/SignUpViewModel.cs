using System;
using System.Windows.Input;
using Controller;
using UX.Core;
using UX.Core.Services;

namespace Controller.UI.ViewModels
{
    /// <summary>
    /// Manages the registration form, validating user input and creating accounts via the controller.
    /// </summary>
    public class SignUpViewModel : ObservableObject
    {
        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value;
                OnPropertyChanged();
            }
        }

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

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
            }
        }

        public ICommand SignUpCommand { get; }
        public ICommand GoToLoginCommand { get; }

        private readonly AuthViewModel _authViewModel;
        private readonly IController _controller;
        private readonly IToastService _toastService;

        /// <summary>
        /// Creates the sign-up view model and initializes commands for submission and navigation.
        /// </summary>
        public SignUpViewModel(AuthViewModel authViewModel, IController controller, IToastService toastService)
        {
            _authViewModel = authViewModel ?? throw new ArgumentNullException(nameof(authViewModel));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));

            _displayName = string.Empty;
            _email = string.Empty;
            _password = string.Empty;
            _confirmPassword = string.Empty;
            SignUpCommand = new RelayCommand(SignUp);
            GoToLoginCommand = new RelayCommand(GoToLogin);
        }

        /// <summary>
        /// Validates input and attempts to register a new account, surfacing toast feedback along the way.
        /// </summary>
        private void SignUp(object? obj)
        {
            // Validate display name
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                _toastService.ShowError("Please enter your name");
                return;
            }

            // Validate email
            if (string.IsNullOrWhiteSpace(Email))
            {
                _toastService.ShowError("Please enter your email address");
                return;
            }

            // Basic email validation
            if (!Email.Contains("@") || !Email.Contains("."))
            {
                _toastService.ShowError("Please enter a valid email address");
                return;
            }

            // Validate password
            if (string.IsNullOrWhiteSpace(Password))
            {
                _toastService.ShowError("Please enter a password");
                return;
            }

            if (Password.Length < 6)
            {
                _toastService.ShowWarning("Password should be at least 6 characters long");
                return;
            }

            // Validate password confirmation
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                _toastService.ShowError("Please confirm your password");
                return;
            }

            if (Password != ConfirmPassword)
            {
                _toastService.ShowError("Passwords do not match. Please try again.");
                return;
            }

            // Attempt signup
            if (_controller.SignUp(DisplayName, Email, Password))
            {
                var user = _controller.GetUser();
                if (user != null)
                {
                    _toastService.ShowSuccess($"Account created successfully! Welcome, {user.DisplayName}!");
                    _authViewModel.OnLoggedIn(user);
                }
            }
            else
            {
                _toastService.ShowError("Unable to create account. Email may already be in use.");
            }
        }

        /// <summary>
        /// Switches to the login form via the parent auth view model.
        /// </summary>
        private void GoToLogin(object? obj)
        {
            _authViewModel.SwitchViewCommand.Execute(null);
        }
    }
}
