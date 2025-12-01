/*
 * -----------------------------------------------------------------------------
 *  File: AuthViewModel.cs
 *  Owner: Pramodh Sai
 *  Roll Number : 112201029
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Communicator.Controller;
using Communicator.Controller.Meeting;
using Communicator.Controller.Serialization;
using Communicator.Controller.RPC;
using Communicator.UX.Core;
using Communicator.UX.Core.Services;
using Communicator.App.Services;

namespace Communicator.App.ViewModels.Auth;

/// <summary>
/// Handles Google OAuth authentication flow using RPC.
/// Matches Java's LoginViewModel implementation.
/// </summary>
public sealed class AuthViewModel : ObservableObject
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

    private UserProfile? _currentUser;
    public UserProfile? CurrentUser
    {
        get => _currentUser;
        set {
            if (SetProperty(ref _currentUser, value) && value != null)
            {
                // Raise event to notify that user has logged in
                LoggedIn?.Invoke(this, new UserProfileEventArgs(value));
            }
        }
    }

    public ICommand SignInWithGoogleCommand { get; }
    public ICommand DebugLoginCommand { get; }

    public static bool IsDebugMode =>
#if DEBUG
            true;
#else
            false;
#endif


    public AuthViewModel(IRPC rpc, IToastService toastService)
    {
        _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));

        SignInWithGoogleCommand = new RelayCommand(SignInWithGoogle, _ => !IsLoading);
        DebugLoginCommand = new RelayCommand(DebugLogin, _ => !IsLoading);
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
            byte[] responseData = await _rpc.Call("core/register", Array.Empty<byte>()).ConfigureAwait(true);

            System.Diagnostics.Debug.WriteLine($"[AuthViewModel] Response data length: {responseData.Length}");

            // Deserialize response to UserProfile
            // This matches Java: DataSerializer.deserialize(data, UserProfile.class)
            UserProfile user = DataSerializer.Deserialize<UserProfile>(responseData);

            // Update current user on UI thread
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => {
                CurrentUser = user;
                IsLoading = false;
            }).Task.ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is System.Text.Json.JsonException)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthViewModel] Error: {ex.Message}");

            // Update UI on UI thread
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => {
                ErrorMessage = $"Authentication failed: {ex.Message}";
                IsLoading = false;
                _toastService.ShowError($"Authentication failed: {ex.Message}");
            }).Task.ConfigureAwait(true);
        }
    }

    private void DebugLogin(object? obj)
    {
#if DEBUG
        System.Diagnostics.Debug.WriteLine("[AuthViewModel] Debug Login");
        var dummyUser = new UserProfile {
            Email = "debug@example.com",
            DisplayName = "Debug User",
            Role = ParticipantRole.INSTRUCTOR,
            LogoUrl = new Uri("https://via.placeholder.com/150")
        };
        CurrentUser = dummyUser;
#endif
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


