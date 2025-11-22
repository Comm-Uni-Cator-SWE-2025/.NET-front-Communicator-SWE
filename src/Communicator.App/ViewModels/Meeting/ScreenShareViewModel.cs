/*
 * -----------------------------------------------------------------------------
 *  File: ScreenShareViewModel.cs
 *  Owner: UpdateNamesForEachModule
 *  Roll Number :
 *  Module : 
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Communicator.Controller.Meeting;
using Communicator.Core.UX;

namespace Communicator.App.ViewModels.Meeting;

/// <summary>
/// Describes the screen share view, displaying participants and their screen share streams.
/// Manages dynamic grid layout for screen sharing participants.
/// </summary>
public sealed class ScreenShareViewModel : ObservableObject
{
    private int _gridColumns = 1;
    private int _gridRows = 1;

    /// <summary>
    /// Initializes screen share view model with the active user context
    /// and shared participants collection.
    /// </summary>
    public ScreenShareViewModel(UserProfile user, ObservableCollection<ParticipantViewModel> participants)
    {
        Title = "ScreenShare";
        CurrentUser = user;
        Participants = participants;

        // Subscribe to collection changes to update grid layout
        Participants.CollectionChanged += (s, e) => UpdateGridLayout();
        UpdateGridLayout();
    }

    public string Title { get; }
    public UserProfile CurrentUser { get; }

    /// <summary>
    /// Collection of all participants with their screen share streams.
    /// This is a shared reference from MeetingSessionViewModel.
    /// </summary>
    public ObservableCollection<ParticipantViewModel> Participants { get; }

    /// <summary>
    /// Number of columns in the screen share grid.
    /// </summary>
    public int GridColumns
    {
        get => _gridColumns;
        private set => SetProperty(ref _gridColumns, value);
    }

    /// <summary>
    /// Number of rows in the screen share grid.
    /// </summary>
    public int GridRows
    {
        get => _gridRows;
        private set => SetProperty(ref _gridRows, value);
    }

    /// <summary>
    /// Updates grid layout based on participant count.
    /// </summary>
    private void UpdateGridLayout()
    {
        int count = Participants?.Count ?? 0;
        (int columns, int rows) layout = CalculateGridLayout(count);
        GridColumns = layout.columns;
        GridRows = layout.rows;
    }

    /// <summary>
    /// Calculates optimal grid layout for the given participant count.
    /// </summary>
    private static (int columns, int rows) CalculateGridLayout(int count)
    {
        if (count == 0)
        {
            return (1, 1);
        }

        if (count == 1)
        {
            return (1, 1);
        }

        if (count == 2)
        {
            return (2, 1);
        }

        if (count <= 4)
        {
            return (2, 2);
        }

        if (count <= 6)
        {
            return (3, 2);
        }

        if (count <= 9)
        {
            return (3, 3);
        }

        if (count <= 12)
        {
            return (4, 3);
        }

        if (count <= 16)
        {
            return (4, 4);
        }

        // For larger counts, calculate dynamically
        int columns = (int)Math.Ceiling(Math.Sqrt(count));
        int rows = (int)Math.Ceiling((double)count / columns);
        return (columns, rows);
    }
}



