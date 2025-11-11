using System;

namespace Controller;

public class MockController : IController
{
    private UserProfile? _currentUser;

    public MockController()
    {
    }

    /// <summary>
    /// Mock Google OAuth login - generates user from authorization code.
    /// </summary>
    public bool LoginWithGoogle(string authorizationCode)
    {
        // Simulate network delay
        System.Threading.Thread.Sleep(500);

        if (string.IsNullOrWhiteSpace(authorizationCode))
        {
            return false;
        }

        var googleUser = new UserProfile(
            email: $"user{Math.Abs(authorizationCode.GetHashCode()) % 1000}@iitpkd.ac.in",
            displayName: $"User {Math.Abs(authorizationCode.GetHashCode()) % 1000}",
            passwordHash: "GOOGLE_OAUTH_NO_PASSWORD",
            userRole: "User"
        );

        _currentUser = googleUser;
        return true;
    }

    public UserProfile? GetUser()
    {
        return _currentUser;
    }

    public void AddUser(ClientNode deviceNode, ClientNode clientNode)
    {
        // Dummy implementation
    }
}
