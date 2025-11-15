using Controller;
using Communicator.Core.UX;

namespace GUI.ViewModels.Meeting;

/// <summary>
/// Supplies summary data for the dashboard tab within the meeting experience.
/// </summary>
public class MeetingDashboardViewModel : ObservableObject
{
    /// <summary>
    /// Captures the active user for personalization within the dashboard.
    /// </summary>
    public MeetingDashboardViewModel(User user)
    {
        Title = "Dashboard";
        CurrentUser = user;
    }

    public string Title { get; }
    public User CurrentUser { get; }
}

