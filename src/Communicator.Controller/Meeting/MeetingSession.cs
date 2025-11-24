using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Communicator.Controller.Meeting;

public class MeetingSession
{
    [JsonPropertyName("meetingId")]
    public string MeetingId { get; set; }

    [JsonPropertyName("createdBy")]
    public string CreatedBy { get; set; }

    [JsonPropertyName("createdAt")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("sessionMode")]
    public SessionMode SessionMode { get; set; }

    [JsonPropertyName("participants")]
    public ConcurrentDictionary<string, UserProfile> Participants { get; } = new();

    /// <summary>
    /// Default constructor for serialization.
    /// </summary>
    public MeetingSession()
    {
        MeetingId = string.Empty;
        CreatedBy = string.Empty;
    }

    /// <summary>
    /// Creates a new meeting with a unique ID.
    /// </summary>
    public MeetingSession(string createdBy, SessionMode sessionMode)
    {
        SessionMode = sessionMode;
        MeetingId = Guid.NewGuid().ToString();
        CreatedBy = createdBy;
        CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public MeetingSession(
        string meetingId,
        string createdBy,
        long createdAt,
        SessionMode sessionMode,
        ConcurrentDictionary<string, UserProfile>? participants = null)
    {
        MeetingId = meetingId;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        SessionMode = sessionMode;

        if (participants != null)
        {
            Participants = participants;
        }
    }

    public UserProfile? GetParticipant(string emailId)
    {
        Participants.TryGetValue(emailId, out UserProfile? user);
        return user;
    }

    public void AddParticipant(UserProfile participant)
    {
        if (participant?.Email != null)
        {
            Participants[participant.Email] = participant;
        }
    }

}
