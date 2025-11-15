// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Controller;

public interface IController
{
    void AddUser(ClientNode deviceNode, ClientNode clientNode);
    User? GetUser();

    /// <summary>
    /// Authenticates user via Google OAuth flow.
    /// </summary>
    /// <param name="authorizationCode">Authorization code from Google OAuth redirect</param>
    /// <returns>True if authentication successful, false otherwise</returns>
    bool LoginWithGoogle(string authorizationCode);
}
