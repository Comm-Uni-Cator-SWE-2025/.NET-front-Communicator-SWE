using System;

namespace Controller;

public class UserProfile
{
    public string UserId { get; }
    public string Email { get; }
    public string DisplayName { get; set; }
    public string Role { get; }
    public string PasswordHash { get; }

    public UserProfile(string email, string displayName, string passwordHash, string userRole)
    {
        UserId = Guid.NewGuid().ToString();
        Email = email;
        DisplayName = displayName;
        Role = userRole;
        PasswordHash = passwordHash;
    }
}
