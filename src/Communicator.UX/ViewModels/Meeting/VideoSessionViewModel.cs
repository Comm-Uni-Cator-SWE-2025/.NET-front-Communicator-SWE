using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Communicator.Controller.Meeting;
using Communicator.Core.RPC;
using Communicator.Core.UX;

namespace Communicator.UX.ViewModels.Meeting;

/// <summary>
/// View mode for the video session: Grid view, Video focus, or Screen focus.
/// </summary>
public enum VideoViewMode
{
    Grid,           // All participants in grid layout
    VideoFocus,     // One participant's video in focus
    ScreenFocus     // One participant's screen share in focus
}

/// <summary>
/// Represents the video session view, displaying all participants with their video/screen streams.
/// Manages view modes (Grid, VideoFocus, ScreenFocus), participant sorting, and RPC frame updates.
/// </summary>
public class VideoSessionViewModel : ObservableObject
{
    private int _gridColumns = 1;
    private int _gridRows = 1;
    private VideoViewMode _viewMode = VideoViewMode.Grid;
    private ParticipantViewModel? _focusedParticipant;
    private readonly IRPC? _rpc;

    /// <summary>
    /// Initializes the video session view model with the supplied user context,
    /// shared participants collection, and RPC interface for frame updates.
    /// </summary>
    public VideoSessionViewModel(UserProfile user, ObservableCollection<ParticipantViewModel> participants, IRPC? rpc = null)
    {
        Title = "Meeting";
        CurrentUser = user;
        Participants = participants;
        _rpc = rpc;

        // Create sorted view with screen sharers first
        SortedParticipants = new ObservableCollection<ParticipantViewModel>(
            participants.OrderByDescending(p => p.IsScreenSharing)
        );

        // Subscribe to collection changes to update grid layout and sorting
        Participants.CollectionChanged += OnParticipantsChanged;
        
        // Subscribe to each participant's property changes for re-sorting
        foreach (ParticipantViewModel participant in Participants)
        {
            participant.PropertyChanged += OnParticipantPropertyChanged;
        }

        // Initialize RPC subscriptions
        InitializeRpcSubscriptions();

        UpdateGridLayout();

        // Commands
        ParticipantClickCommand = new RelayCommand(ExecuteParticipantClick);
    }

    public string Title { get; }
    public UserProfile CurrentUser { get; }

    /// <summary>
    /// Collection of all participants (raw, unsorted).
    /// This is a shared reference from MeetingSessionViewModel.
    /// </summary>
    public ObservableCollection<ParticipantViewModel> Participants { get; }

    /// <summary>
    /// Sorted collection with screen sharers appearing first.
    /// Used for grid view display.
    /// </summary>
    public ObservableCollection<ParticipantViewModel> SortedParticipants { get; }

    /// <summary>
    /// Current view mode (Grid, VideoFocus, ScreenFocus).
    /// </summary>
    public VideoViewMode ViewMode
    {
        get => _viewMode;
        private set => SetProperty(ref _viewMode, value);
    }

    /// <summary>
    /// The participant currently in focus (null in Grid mode).
    /// </summary>
    public ParticipantViewModel? FocusedParticipant
    {
        get => _focusedParticipant;
        private set => SetProperty(ref _focusedParticipant, value);
    }

    /// <summary>
    /// Command to handle participant tile clicks.
    /// </summary>
    public ICommand ParticipantClickCommand { get; }

    /// <summary>
    /// Number of columns in the participant grid.
    /// </summary>
    public int GridColumns
    {
        get => _gridColumns;
        private set => SetProperty(ref _gridColumns, value);
    }

    /// <summary>
    /// Number of rows in the participant grid.
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

    /// <summary>
    /// Initializes RPC subscriptions for video/screen frame updates.
    /// </summary>
    private void InitializeRpcSubscriptions()
    {
        if (_rpc == null)
        {
            return;
        }

        // TODO: Subscribe to UPDATE_UI to receive video/screen frames
        // _rpc.Subscribe(Utils.UPDATE_UI, OnFrameReceived);

        // TODO: Subscribe to STOP_SHARE to clear screen frames
        // _rpc.Subscribe(Utils.STOP_SHARE, OnStopShare);
    }

    /// <summary>
    /// Handles participant collection changes (add/remove).
    /// </summary>
    private void OnParticipantsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Subscribe to new participants
        if (e.NewItems != null)
        {
            foreach (ParticipantViewModel participant in e.NewItems)
            {
                participant.PropertyChanged += OnParticipantPropertyChanged;
            }
        }

        // Unsubscribe from removed participants
        if (e.OldItems != null)
        {
            foreach (ParticipantViewModel participant in e.OldItems)
            {
                participant.PropertyChanged -= OnParticipantPropertyChanged;
            }
        }

        UpdateSortedParticipants();
        UpdateGridLayout();
    }

    /// <summary>
    /// Handles participant property changes (e.g., IsScreenSharing) to re-sort.
    /// </summary>
    private void OnParticipantPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ParticipantViewModel.IsScreenSharing))
        {
            UpdateSortedParticipants();
        }
    }

    /// <summary>
    /// Updates sorted participants list (screen sharers first).
    /// </summary>
    private void UpdateSortedParticipants()
    {
        var sorted = Participants.OrderByDescending(p => p.IsScreenSharing).ToList();
        
        SortedParticipants.Clear();
        foreach (ParticipantViewModel participant in sorted)
        {
            SortedParticipants.Add(participant);
        }
    }

    /// <summary>
    /// Handles participant tile click for focus mode switching.
    /// </summary>
    private void OnParticipantClick(ParticipantViewModel? participant)
    {
        if (participant == null)
        {
            return;
        }

        // Click behavior based on current state
        if (ViewMode == VideoViewMode.Grid)
        {
            // Grid → VideoFocus
            FocusedParticipant = participant;
            ViewMode = VideoViewMode.VideoFocus;
        }
        else if (ViewMode == VideoViewMode.VideoFocus && FocusedParticipant == participant)
        {
            // Same participant clicked in VideoFocus
            if (participant.IsScreenSharing && participant.HasScreenFrame)
            {
                // Has screen → Switch to ScreenFocus
                ViewMode = VideoViewMode.ScreenFocus;
            }
            else
            {
                // No screen → Back to Grid
                FocusedParticipant = null;
                ViewMode = VideoViewMode.Grid;
            }
        }
        else if (ViewMode == VideoViewMode.ScreenFocus && FocusedParticipant == participant)
        {
            // Same participant clicked in ScreenFocus → Back to Grid
            FocusedParticipant = null;
            ViewMode = VideoViewMode.Grid;
        }
        else
        {
            // Different participant clicked → Switch focus to them
            FocusedParticipant = participant;
            ViewMode = VideoViewMode.VideoFocus;
        }
    }

    /// <summary>
    /// Command wrapper for participant click (parameter comes from CommandParameter binding).
    /// </summary>
    private void ExecuteParticipantClick(object? parameter)
    {
        if (parameter is ParticipantViewModel participant)
        {
            OnParticipantClick(participant);
        }
    }
}
