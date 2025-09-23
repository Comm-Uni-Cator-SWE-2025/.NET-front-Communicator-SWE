using System;
using System.Collections.Generic;

namespace Controller
{
    public class AuthService
    {
        private readonly Dictionary<string, UserProfile> _users = new Dictionary<string, UserProfile>(StringComparer.OrdinalIgnoreCase);

        public UserProfile? Register(string email, string password, string displayName)
        {
            if (_users.ContainsKey(email))
            {
                return null;
            }

            var role = ResolveRoleFromEmail(email);
            if (role == null)
            {
                return null;
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new UserProfile(email, displayName, hashedPassword, role);
            _users.Add(email, user);
            return user;
        }

        public UserProfile? Login(string email, string password)
        {
            if (!_users.TryGetValue(email, out var user))
            {
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }

            return user;
        }

        private static string? ResolveRoleFromEmail(string email)
        {
            if (email.EndsWith("@iitpkd.ac.in", StringComparison.OrdinalIgnoreCase))
            {
                return "instructor";
            }

            if (email.EndsWith("@smail.iitpkd.ac.in", StringComparison.OrdinalIgnoreCase))
            {
                return "student";
            }

            return null;
        }
    }
}
