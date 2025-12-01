/*
 * -----------------------------------------------------------------------------
 *  File: ParticipantsListViewModel.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */

using System.Collections.ObjectModel;
using Communicator.UX.Core;

namespace Communicator.App.ViewModels.Meeting;

/// <summary>
/// ViewModel for the participants list displayed in the side panel.
/// Shows a list of all meeting participants with their status.
/// </summary>
internal sealed class ParticipantsListViewModel : ObservableObject
{
    private int _participantCount;

    /// <summary>
    /// Initializes a new participants list view model with the given participants collection.
    /// </summary>
    /// <param name="participants">The observable collection of participants to display.</param>
    public ParticipantsListViewModel(ObservableCollection<ParticipantViewModel> participants)
    {
        Participants = participants;
        UpdateParticipantCount();

        // Subscribe to collection changes to update count
        Participants.CollectionChanged += (s, e) => UpdateParticipantCount();
    }

    /// <summary>
    /// The collection of participants in the meeting.
    /// </summary>
    public ObservableCollection<ParticipantViewModel> Participants { get; }

    /// <summary>
    /// The total number of participants.
    /// </summary>
    public int ParticipantCount
    {
        get => _participantCount;
        private set => SetProperty(ref _participantCount, value);
    }

    private void UpdateParticipantCount()
    {
        ParticipantCount = Participants?.Count ?? 0;
    }
}


