using AnalyticsApp.Models;

namespace AnalyticsApp.Services;

/// <summary>
/// Service responsible for generating grouped AI messages
/// in a cyclic (round-robin) order.
/// </summary>
public class AIMessageService
{
    /// <summary>
    /// Predefined groups of related messages that will be returned sequentially.
    /// </summary>
    private readonly List<string[]> _messageGroups = new()
    {
        new[]
        {
            "Developer 1 will handle the backend deployment scripts.",
            "Developer 2 will update the UI today."
        },
        new[]
        {
            "QA will begin regression testing soon."
        },
        new[]
        {
            "Sprint plan approved.",
            "Database migration ongoing.",
            "Designer preparing dashboard mockups."
        },
        new[]
        {
            "Code review tomorrow at 2 PM.",
            "Update your task in the tracking system."
        }
    };

    /// <summary>
    /// Tracks the current position in the message groups (cyclic index).
    /// </summary>
    private int _index = 0;

    /// <summary>
    /// Retrieves the next set of messages as a list of <see cref="AIMessageData"/>.
    /// The groups are returned one by one in sequence (round-robin).
    /// </summary>
    /// <returns>A list of AIMessageData items containing timestamped messages.</returns>
    public List<AIMessageData> GetNext()
    {
        string[] messages = _messageGroups[_index];

        // Move index forward cyclically
        _index = (_index + 1) % _messageGroups.Count;

        var result = new List<AIMessageData>();

        foreach (string text in messages)
        {
            result.Add(new AIMessageData
            {
                Time = DateTime.Now,
                Message = text
            });
        }

        return result;
    }
}
