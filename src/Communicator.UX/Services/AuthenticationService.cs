using System;
using Controller;
using Communicator.Core.UX;

namespace Communicator.UX.Services;

/// <summary>
/// Implementation of authentication service that manages user session state.
/// </summary>
public class AuthenticationService : ObservableObject, IAuthenticationService
{
    private User? _currentUser;

    public event EventHandler<UserProfileEventArgs>? UserLoggedIn;
    public event EventHandler? UserLoggedOut;

    public User? CurrentUser
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

    public void CompleteLogin(User user)
    {
        CurrentUser = user ?? throw new ArgumentNullException(nameof(user));
        UserLoggedIn?.Invoke(this, new UserProfileEventArgs(user));
    }

    public void Logout()
    {
        CurrentUser = null;
        UserLoggedOut?.Invoke(this, EventArgs.Empty);
    }
}
