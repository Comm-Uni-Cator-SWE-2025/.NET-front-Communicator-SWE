namespace Communicator.UX.Analytics.Models;

/// <summary>
/// Small DTO that holds meeting statistics and the previous meeting summary.
/// </summary>
public class MeetStats
{
    public int UsersPresent { get; set; }
    public int UsersLoggedOut { get; set; }

    /// <summary>
    /// Plain text summary from previous meeting.
    /// </summary>
    public string PreviousSummary { get; set; } = string.Empty;
}
