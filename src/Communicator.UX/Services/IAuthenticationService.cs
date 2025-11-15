using System;
using Controller;

namespace Communicator.UX.Services;

/// <summary>
/// Event arguments for user authentication events.
/// </summary>
public class UserProfileEventArgs : EventArgs
{
    public User User { get; }

    public UserProfileEventArgs(User user)
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
    User? CurrentUser { get; }

    /// <summary>
    /// Indicates whether a user is currently authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Completes the authentication process with the provided user profile.
    /// </summary>
    void CompleteLogin(User user);

    /// <summary>
    /// Clears the current user session.
    /// </summary>
    void Logout();
}
