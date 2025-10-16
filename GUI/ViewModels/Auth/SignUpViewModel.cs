using UX.Core;
using System.Windows.Input;

namespace GUI.ViewModels.Auth
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

        /// <summary>
        /// Creates the sign-up view model and initializes commands for submission and navigation.
        /// </summary>
        public SignUpViewModel(AuthViewModel authViewModel)
        {
            _authViewModel = authViewModel;
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
                App.ToastService.ShowError("Please enter your name");
                return;
            }

            // Validate email
            if (string.IsNullOrWhiteSpace(Email))
            {
                App.ToastService.ShowError("Please enter your email address");
                return;
            }

            // Basic email validation
            if (!Email.Contains("@") || !Email.Contains("."))
            {
                App.ToastService.ShowError("Please enter a valid email address");
                return;
            }

            // Validate password
            if (string.IsNullOrWhiteSpace(Password))
            {
                App.ToastService.ShowError("Please enter a password");
                return;
            }

            if (Password.Length < 6)
            {
                App.ToastService.ShowWarning("Password should be at least 6 characters long");
                return;
            }

            // Validate password confirmation
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                App.ToastService.ShowError("Please confirm your password");
                return;
            }

            if (Password != ConfirmPassword)
            {
                App.ToastService.ShowError("Passwords do not match. Please try again.");
                return;
            }

            // Attempt signup
            if (App.Controller?.SignUp(DisplayName, Email, Password) ?? false)
            {
                var user = App.Controller?.GetUser();
                if (user != null)
                {
                    App.ToastService.ShowSuccess($"Account created successfully! Welcome, {user.DisplayName}!");
                    _authViewModel.OnLoggedIn(user);
                }
            }
            else
            {
                App.ToastService.ShowError("Unable to create account. Email may already be in use.");
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

