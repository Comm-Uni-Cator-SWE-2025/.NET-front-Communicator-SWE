using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Communicator.Controller;
using Communicator.Controller.Serialization;
using Communicator.Core.RPC;
using Communicator.Core.UX;
using Communicator.Core.UX.Services;
using Communicator.UX.Services;

// Alias to avoid ambiguity between Controller.User (mock) and Communicator.Controller.Meeting.UserProfile (RPC)
using MockUser = Controller.User;
using RpcUserProfile = Communicator.Controller.Meeting.UserProfile;

namespace Communicator.UX.ViewModels.Auth;

/// <summary>
/// Handles Google OAuth authentication flow using RPC.
/// Matches Java's LoginViewModel implementation.
/// </summary>
public class AuthViewModel : ObservableObject
{
    private readonly IRPC _rpc;
    private readonly IToastService _toastService;

    public event EventHandler<UserProfileEventArgs>? LoggedIn;

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private RpcUserProfile? _currentUser;
    public RpcUserProfile? CurrentUser
    {
        get => _currentUser;
        set {
            if (SetProperty(ref _currentUser, value) && value != null)
            {
                // Convert RpcUserProfile (from Communicator.Controller.Meeting) to MockUser (from Controller)
                // This maintains compatibility with existing screens that use MockUser
                MockUser mockUser = new(
                    id: value.Email ?? "unknown",
                    username: value.DisplayName ?? "User",
                    displayName: value.DisplayName ?? "User",
                    email: value.Email ?? string.Empty
                );
                LoggedIn?.Invoke(this, new UserProfileEventArgs(mockUser));
            }
        }
    }

    public ICommand SignInWithGoogleCommand { get; }

    public AuthViewModel(IRPC rpc, IToastService toastService)
    {
        _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));

        SignInWithGoogleCommand = new RelayCommand(SignInWithGoogle, _ => !IsLoading);
    }

    /// <summary>
    /// Initiates Google OAuth authentication via RPC.
    /// Matches Java's loginWithGoogle() implementation.
    /// </summary>
    private async void SignInWithGoogle(object? obj)
    {
        System.Diagnostics.Debug.WriteLine("[AuthViewModel] Login with Google");

        // Clear previous errors
        ErrorMessage = string.Empty;
        IsLoading = true;

        try
        {
            System.Diagnostics.Debug.WriteLine($"[AuthViewModel] Calling core/register via RPC: {_rpc}");

            // Call RPC method "core/register" with empty data
            // This matches Java: rpc.call("core/register", new byte[0]).get()
            byte[] responseData = await _rpc.Call("core/register", Array.Empty<byte>()).ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine($"[AuthViewModel] Response data length: {responseData.Length}");

            // Deserialize response to RpcUserProfile
            // This matches Java: DataSerializer.deserialize(data, UserProfile.class)
            RpcUserProfile user = DataSerializer.Deserialize<RpcUserProfile>(responseData);

            // Update current user on UI thread
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => {
                CurrentUser = user;
                IsLoading = false;
                _toastService.ShowSuccess($"Welcome, {user.DisplayName}!");
            });
        }
#pragma warning disable CA1031 // Do not catch general exception types - UI code should gracefully handle all exceptions
        catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            System.Diagnostics.Debug.WriteLine($"[AuthViewModel] Error: {ex.Message}");

            // Update UI on UI thread
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => {
                ErrorMessage = $"Authentication failed: {ex.Message}";
                IsLoading = false;
                _toastService.ShowError($"Authentication failed: {ex.Message}");
            });
        }
    }

    /// <summary>
    /// Resets the login form.
    /// </summary>
    public void Reset()
    {
        ErrorMessage = string.Empty;
        IsLoading = false;
        CurrentUser = null;
    }
}
