/*
 * -----------------------------------------------------------------------------
 *  File: TestHelpers.cs
 *  Owner: Devansh Manoj Kesan
 *  Roll Number :142201017
 *  Module : ScreenShare
 *
 * -----------------------------------------------------------------------------
 */

using System.Collections.ObjectModel;
using Communicator.App.ViewModels.Meeting;
using Communicator.Controller.Meeting;

namespace Communicator.ScreenShare.Tests.Helpers;

public static class TestHelpers
{
     // Quick helper to build a user profile for test data.
    public static UserProfile CreateUserProfile(string email, string displayName, ParticipantRole role = ParticipantRole.STUDENT)
        => new()
        {
            Email = email,
            DisplayName = displayName,
            Role = role
        };

     // Creates a ParticipantViewModel with optional screen-sharing state.
    public static ParticipantViewModel CreateParticipant(string email, string displayName, bool isScreenSharing = false)
    {
        var user = CreateUserProfile(email, displayName);
        return new ParticipantViewModel(user) { IsScreenSharing = isScreenSharing };
    }

     // Empty collection used when we want zero participants.
    public static ObservableCollection<ParticipantViewModel> CreateEmptyParticipants()
        => new();

     // Builds a list of participants so we can test layout thresholds quickly.
    public static ObservableCollection<ParticipantViewModel> CreateParticipants(int count, bool screenSharing = false)
    {
        var participants = new ObservableCollection<ParticipantViewModel>();
        for (int i = 1; i <= count; i++)
        {
            participants.Add(CreateParticipant($"user{i}@test.com", $"User {i}", screenSharing));
        }
        return participants;
    }
}

