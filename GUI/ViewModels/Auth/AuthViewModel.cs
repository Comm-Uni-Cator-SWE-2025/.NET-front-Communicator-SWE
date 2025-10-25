using System;
using System.Windows.Input;
using Controller;
using UX.Core;
using UX.Core.Services;

namespace GUI.ViewModels.Auth
{
    /// <summary>
    /// Handles Google OAuth authentication flow.
    /// </summary>
    public class AuthViewModel : ObservableObject
    {
        private readonly IController _controller;
        private readonly IToastService _toastService;

        public event Action<UserProfile>? LoggedIn;

        public ICommand SignInWithGoogleCommand { get; }

        public AuthViewModel(IController controller, IToastService toastService)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
            
            SignInWithGoogleCommand = new RelayCommand(SignInWithGoogle);
        }

        /// <summary>
        /// Initiates Google OAuth authentication.
        /// </summary>
        private async void SignInWithGoogle(object? obj)
        {
            try
            {
                // Simulate OAuth popup/redirect delay
                await Task.Delay(800);

                // Generate mock authorization code
                string authCode = $"MOCK_AUTH_{DateTime.Now.Ticks}";
                
                // Authenticate via Controller
                bool success = await Task.Run(() => 
                    _controller.LoginWithGoogle(authCode));

                if (success)
                {
                    var user = _controller.GetUser();
                    if (user != null)
                    {
                        _toastService.ShowSuccess($"Welcome, {user.DisplayName}!");
                        LoggedIn?.Invoke(user);
                    }
                    else
                    {
                        _toastService.ShowError("Failed to retrieve user profile");
                    }
                }
                else
                {
                    _toastService.ShowError("Authentication failed");
                }
            }
            catch (Exception ex)
            {
                _toastService.ShowError($"Authentication error: {ex.Message}");
            }
        }
    }
}
