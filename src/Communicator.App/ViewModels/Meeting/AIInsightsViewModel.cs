/*
 * -----------------------------------------------------------------------------
 *  File: AIInsightsViewModel.cs
 *  Owner: UpdateNamesForEachModule
 *  Roll Number :
 *  Module : 
 *
 * -----------------------------------------------------------------------------
 */
using Communicator.Controller.Meeting;
using Communicator.Core.UX;

namespace Communicator.App.ViewModels.Meeting;

/// <summary>
/// Provides AI-powered insights and analytics for the meeting experience.
/// </summary>
public sealed class AIInsightsViewModel : ObservableObject
{
    /// <summary>
    /// Initializes AI Insights with the active user context.
    /// </summary>
    public AIInsightsViewModel(UserProfile user)
    {
        Title = "AI Insights";
        CurrentUser = user;
    }

    public string Title { get; }
    public UserProfile CurrentUser { get; }
}


