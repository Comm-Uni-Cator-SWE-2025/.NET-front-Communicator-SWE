namespace Controller;

public interface IController
{
    void AddUser(ClientNode deviceNode, ClientNode clientNode);
    UserProfile? GetUser();

    /// <summary>
    /// Authenticates user via Google OAuth flow.
    /// </summary>
    /// <param name="authorizationCode">Authorization code from Google OAuth redirect</param>
    /// <returns>True if authentication successful, false otherwise</returns>
    bool LoginWithGoogle(string authorizationCode);
}
