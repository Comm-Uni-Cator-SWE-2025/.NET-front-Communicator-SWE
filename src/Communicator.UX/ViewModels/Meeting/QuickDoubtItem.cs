using System;

namespace Communicator.UX.ViewModels.Meeting;

/// <summary>
/// Represents a single Quick Doubt message item.
/// </summary>
public class QuickDoubtItem
{
    /// <summary>
    /// Unique identifier for this doubt.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Name of the person who raised the doubt.
    /// </summary>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// The doubt message content.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// When the doubt was raised.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
