using System;

namespace Controller;

public class User : IEquatable<User>
{
    /// <summary>
    /// Gets the user's unique identifier.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Gets the user's unique username.
    /// </summary>
    public string Username { get; init; }

    /// <summary>
    /// Gets or sets the user's display name.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the URL for the user's profile image.
    /// </summary>
    public string ProfileImageUrl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is online.
    /// </summary>
    public bool IsOnline { get; set; }

    /// <summary>
    /// Creates a new User instance.
    /// </summary>
    /// <param name="id">The user's unique ID.</param>
    /// <param name="username">The user's username.</param>
    /// <param name="displayName">The user's display name. If null, username is used.</param>
    /// <param name="email">The user's email address.</param>
    public User(string id, string username, string displayName, string email)
    {
        Id = id;
        Username = username;
        // C#'s null-coalescing operator (??) is the
        // equivalent of Java's 'displayName != null ? displayName : username'
        DisplayName = displayName ?? username;
        Email = email;
        IsOnline = false;
        ProfileImageUrl = null; // Default value, same as Java
    }

    /// <summary>
    /// Returns a string that represents the current user.
    /// </summary>
    /// <returns>A string representation of the user.</returns>
    public override string ToString()
    {
        // C# string interpolation ($"...") is a clean way to format strings.
        return $"User {{ Id = '{Id}', Username = '{Username}', DisplayName = '{DisplayName}', Email = '{Email}', IsOnline = {IsOnline} }}";
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current user,
    /// based on the user's Id.
    /// </summary>
    /// <param name="obj">The object to compare with the current user.</param>
    /// <returns>True if the specified object is a User with the same Id; otherwise, false.</returns>
    public override bool Equals(object obj)
    {
        // This pattern handles null and type checking.
        return Equals(obj as User);
    }

    /// <summary>
    /// Determines whether the specified User is equal to the current user,
    /// based on the user's Id.
    /// </summary>
    /// <param name="other">The user to compare with the current user.</param>
    /// <returns>True if the users have the same Id; otherwise, false.</returns>
    public bool Equals(User other)
    {
        // If 'other' is null, 'other is null' is true, returns false.
        // If 'other' is not null, it compares Id.
        return other is not null && Id == other.Id;
    }

    /// <summary>
    // Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current user, based on the Id.</returns>
    public override int GetHashCode()
    {
        // The hash code should be based on the same field(s) as Equals().
        return Id.GetHashCode();
    }
}
