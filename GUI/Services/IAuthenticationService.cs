using System;
using Controller;

namespace GUI.Services;

/// <summary>
/// Manages user authentication state and login/logout operations.
/// Abstracts authentication logic from the main shell ViewModel.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Raised when a user successfully authenticates.
    /// </summary>
    event Action<UserProfile>? UserLoggedIn;
    
    /// <summary>
    /// Raised when a user logs out.
    /// </summary>
    event Action? UserLoggedOut;
    
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
    void Logout();
}
