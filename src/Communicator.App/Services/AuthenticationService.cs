/*
 * -----------------------------------------------------------------------------
 *  File: AuthenticationService.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.Threading.Tasks;
using Communicator.Controller.Meeting;
using Communicator.Core.RPC;
using Communicator.Core.UX;

namespace Communicator.App.Services;

/// <summary>
/// Implementation of authentication service that manages user session state.
/// </summary>
public sealed class AuthenticationService : ObservableObject, IAuthenticationService
{
    private readonly IRPC _rpc;
    private UserProfile? _currentUser;

    public event EventHandler<UserProfileEventArgs>? UserLoggedIn;
    public event EventHandler? UserLoggedOut;

    public AuthenticationService(IRPC rpc)
    {
        _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
    }

    public UserProfile? CurrentUser
    {
        get => _currentUser;
        private set {
            if (SetProperty(ref _currentUser, value))
            {
                OnPropertyChanged(nameof(IsAuthenticated));
            }
        }
    }

    public bool IsAuthenticated => CurrentUser != null;

    public void CompleteLogin(UserProfile user)
    {
        CurrentUser = user ?? throw new ArgumentNullException(nameof(user));
        UserLoggedIn?.Invoke(this, new UserProfileEventArgs(user));
    }

    public async Task LogoutAsync()
    {
        try
        {
            // Notify backend of logout
            // We send empty byte array or user email if needed, but backend seems to use context
            // Init.java: rpc.subscribe("core/logout", (byte[] userData) -> ...
            await _rpc.Call("core/logout", Array.Empty<byte>()).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthenticationService] Error calling core/logout: {ex.Message}");
            // Continue with local logout even if RPC fails
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthenticationService] Unexpected error calling core/logout: {ex.Message}");
        }
#pragma warning restore CA1031 // Do not catch general exception types

        CurrentUser = null;
        UserLoggedOut?.Invoke(this, EventArgs.Empty);
    }
}


