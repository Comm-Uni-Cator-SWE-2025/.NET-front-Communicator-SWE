using System;
using Controller;
using UX.Core;

namespace GUI.Services;

/// <summary>
/// Implementation of authentication service that manages user session state.
/// </summary>
public class AuthenticationService : ObservableObject, IAuthenticationService
{
    private UserProfile? _currentUser;
    
    public event Action<UserProfile>? UserLoggedIn;
    public event Action? UserLoggedOut;
    
    public UserProfile? CurrentUser
    {
        get => _currentUser;
        private set
        {
            if (SetProperty(ref _currentUser, value))
            {
                OnPropertyChanged(nameof(IsAuthenticated));
            }
        }
    }
    
    public bool IsAuthenticated => CurrentUser != null;
    
    public void CompleteLogin(UserProfile user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
            
        CurrentUser = user;
        UserLoggedIn?.Invoke(user);
    }
    
    public void Logout()
    {
        CurrentUser = null;
        UserLoggedOut?.Invoke();
    }
}
