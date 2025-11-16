using Communicator.Core.UX;
using Controller;

namespace Communicator.UX.ViewModels.Meeting;

/// <summary>
/// Provides AI-powered insights and analytics for the meeting experience.
/// </summary>
public class AIInsightsViewModel : ObservableObject
{
    /// <summary>
    /// Initializes AI Insights with the active user context.
    /// </summary>
    public AIInsightsViewModel(User user)
    {
        Title = "AI Insights";
        CurrentUser = user;
    }

    public string Title { get; }
    public User CurrentUser { get; }
}
