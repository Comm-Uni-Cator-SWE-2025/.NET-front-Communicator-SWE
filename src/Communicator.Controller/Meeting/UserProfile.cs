using System;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;


namespace Communicator.Controller.Meeting;

public class UserProfile
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("role")]
    public ParticipantRole Role { get; set; }

    [JsonPropertyName("logoUrl")]
    public Uri? LogoUrl { get; set; }

    public UserProfile() { }
    public UserProfile(string? email, string? displayName, ParticipantRole role, Uri? logoUrl)
    {
        Email = email;
        DisplayName = displayName;
        Role = role;
        LogoUrl = logoUrl;
    }

    /*
     * Public override string ToString()
    */
    public override string ToString()
    {
        return $"UserProfile{{email='{Email}', displayName='{DisplayName}', role='{Role}', logoUrl='{LogoUrl}'}}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not UserProfile other)
        {
            return false;
        }
        return Email == other.Email &&
               DisplayName == other.DisplayName &&
               Role == other.Role &&
               LogoUrl == other.LogoUrl;

    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Email, DisplayName, Role, LogoUrl);
    }
}
