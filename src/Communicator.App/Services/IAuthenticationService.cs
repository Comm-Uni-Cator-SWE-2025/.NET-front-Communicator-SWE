/*
 * -----------------------------------------------------------------------------
 *  File: IAuthenticationService.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;
using Communicator.Controller.Meeting;

namespace Communicator.App.Services;

/// <summary>
/// Event arguments for user authentication events.
/// </summary>
public sealed class UserProfileEventArgs : EventArgs
{
    public UserProfile User { get; }

    public UserProfileEventArgs(UserProfile user)
    {
        User = user;
    }
}

/// <summary>
/// Manages user authentication state and login/logout operations.
/// Abstracts authentication logic from the main shell ViewModel.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Raised when a user successfully authenticates.
    /// </summary>
    event EventHandler<UserProfileEventArgs>? UserLoggedIn;

    /// <summary>
    /// Raised when a user logs out.
    /// </summary>
    event EventHandler? UserLoggedOut;

    /// <summary>
    /// The currently authenticated user, or null if not logged in.
    /// </summary>
    UserProfile? CurrentUser { get; }

    /// <summary>
    /// Indicates whether a user is currently authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Completes the authentication process with the provided user profile.
    /// </summary>
    void CompleteLogin(UserProfile user);

    /// <summary>
    /// Clears the current user session.
    /// </summary>
    System.Threading.Tasks.Task LogoutAsync();
}


