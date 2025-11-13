// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Controller;
public class Meetings
{
    // Private backing fields for the lists to control access
    private readonly List<User> _participants;
    private readonly List<ChatMessage> _messages;

    /// <summary>
    /// Gets the unique identifier for the meeting.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Gets the title of the meeting.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Gets the time the meeting started.
    /// </summary>
    public DateTime StartTime { get; init; }

    /// <summary>
    /// Gets the time the meeting ended.
    /// This is null if the meeting is still active.
    /// </summary>
    public DateTime? EndTime { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether video is enabled for the user.
    /// </summary>
    public bool VideoEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether screen sharing is enabled.
    /// </summary>
    public bool ScreenSharingEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether audio is enabled for the user.
    /// </summary>
    public bool AudioEnabled { get; set; }

    /// <summary>
    /// Gets a *copy* of the list of participants.
    /// </summary>
    public List<User> Participants => new List<User>(_participants);

    /// <summary>
    /// Gets a *copy* of the list of chat messages.
    /// </summary>
    public List<ChatMessage> Messages => new List<ChatMessage>(_messages);

    /// <summary>
    /// Gets a value indicating whether the meeting is currently active.
    /// </summary>
    public bool IsActive => EndTime == null;

    /// <summary>
    /// Initializes a new instance of the Meeting class.
    /// </summary>
    /// <param name="title">The title of the meeting.</param>
    public Meetings(string title)
    {
        // C#'s Guid.NewGuid() is the equivalent of Java's UUID.randomUUID()
        Id = Guid.NewGuid().ToString();
        Title = title;
        // C#'s DateTime.Now is the equivalent of Java's LocalDateTime.now()
        StartTime = DateTime.Now;

        _participants = new List<User>();
        _messages = new List<ChatMessage>();

        VideoEnabled = false;
        ScreenSharingEnabled = false;
        AudioEnabled = false; // Initialized for clarity
    }

    // --- Methods ---

    /// <summary>
    /// Adds a participant to the meeting if they are not already present.
    /// </summary>
    /// <param name="user">The user to add.</param>
    public void AddParticipant(User? user)
    {
        if (!_participants.Contains(user))
        {
            _participants.Add(user);
        }
    }

    /// <summary>
    /// Removes a participant from the meeting.
    /// </summary>
    /// <param name="user">The user to remove.</param>
    public void RemoveParticipant(User user)
    {
        _participants.Remove(user);
    }

    /// <summary>
    /// Adds a chat message to the meeting's history.
    /// </summary>
    /// <param name="message">The message to add.</param>
    public void AddMessage(ChatMessage message)
    {
        _messages.Add(message);
    }

    /// <summary>
    /// Ends the meeting by setting the EndTime to the current time.
    /// </summary>
    public void EndMeeting()
    {
        if (IsActive) // Only set if not already ended
        {
            EndTime = DateTime.Now;
        }
    }

    /// <summary>
    /// Represents a chat message within a meeting.
    /// (C# equivalent of the nested ChatMessage class)
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Gets the user who sent the message.
        /// </summary>
        public User Sender { get; init; }

        /// <summary>
        /// Gets the content of the message.
        /// </summary>
        public string Content { get; init; }

        /// <summary>
        /// Gets the time the message was sent.
        /// </summary>
        public DateTime Timestamp { get; init; }

        /// <summary>
        /// Initializes a new instance of the ChatMessage class.
        /// </summary>
        /// <param name="sender">The user sending the message.</param>
        /// <param name="content">The content of the message.</param>
        public ChatMessage(User sender, string content)
        {
            Sender = sender;
            Content = content;
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// Returns a string representation of the chat message.
        /// </summary>
        /// <returns>A formatted string.</returns>
        public override string ToString()
        {
            // C# string interpolation
            return $"{Sender.Username}: {Content}";
        }
    }
}
