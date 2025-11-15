// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Controller;

public class MockController : IController
{
    private User? _currentUser;

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

        var googleUser = new User(
            id: $"{Math.Abs(authorizationCode.GetHashCode()) % 1000}",
            username: $"User {Math.Abs(authorizationCode.GetHashCode()) % 1000}",
            email: $"user{Math.Abs(authorizationCode.GetHashCode()) % 1000}@iitpkd.ac.in",
            displayName: $"User {Math.Abs(authorizationCode.GetHashCode()) % 1000}"
        );

        _currentUser = googleUser;
        return true;
    }

    public User? GetUser()
    {
        return _currentUser;
    }

    public void AddUser(ClientNode deviceNode, ClientNode clientNode)
    {
        // Dummy implementation
    }
}
