using System.Collections.ObjectModel;
using Communicator.UX.Analytics.Models;
using Communicator.Core.UX;

namespace Communicator.UX.Analytics.ViewModels;

/// <summary>
/// ViewModel that exposes meeting statistics and announcements to the view.
/// Currently uses static values; replace with service integration when needed.
/// </summary>
public class MeetAnalyticsViewModel : ObservableObject
{
    private MeetStats _meetStats = new()
    {
        UsersPresent = 120,
        UsersLoggedOut = 15,
        PreviousSummary = "The previous meeting was very productive and tasks were assigned."
    };

    /// <summary>
    /// Holds the current meeting statistics for display in the UI.
    /// </summary>
    public MeetStats MeetStats
    {
        get => _meetStats;
        set => SetProperty(ref _meetStats, value);
    }

    /// <summary>
    /// Collection of meeting messages or announcements.
    /// </summary>
    public ObservableCollection<string> Messages { get; } = new()
    {
        "Welcome to the meeting",
        "Design update will be shared soon",
        "Team is progressing well"
    };

    /// <summary>
    /// Initializes the ViewModel with placeholder data.
    /// </summary>
    public MeetAnalyticsViewModel()
    {
        // Static / hardcoded sample data only.
    }
}
