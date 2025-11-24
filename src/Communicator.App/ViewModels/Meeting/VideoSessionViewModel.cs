/*
 * -----------------------------------------------------------------------------
 *  File: VideoSessionViewModel.cs
 *  Owner: UpdateNamesForEachModule
 *  Roll Number :
 *  Module : 
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Communicator.Controller.Meeting;
using Communicator.Core.RPC;
using Communicator.Core.UX;
using Communicator.Core.UX.Services;
using Communicator.ScreenShare;
using Communicator.App.Services;

namespace Communicator.App.ViewModels.Meeting;

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
public sealed class VideoSessionViewModel : ObservableObject, IDisposable
{
    private int _gridColumns = 1;
    private int _gridRows = 1;
    private VideoViewMode _viewMode = VideoViewMode.Grid;
    private ParticipantViewModel? _focusedParticipant;
    private bool _isMaximized;
    private ParticipantViewModel? _maximizedParticipant;
    private readonly IRPC? _rpc;
    private readonly IRpcEventService? _rpcEventService;

    private MeetingSessionViewModel _meetingSessionViewModel;

    /// <summary>
    /// Initializes the video session view model with the supplied user context,
    /// shared participants collection, and RPC interface for frame updates.
    /// </summary>
    public VideoSessionViewModel(
        UserProfile user,
        ObservableCollection<ParticipantViewModel> participants,
        MeetingSessionViewModel meetingSessionViewModel,
        IRPC? rpc = null,
        IRpcEventService? rpcEventService = null)
    {
        Title = "Meeting";
        CurrentUser = user;
        Participants = participants;
        _meetingSessionViewModel = meetingSessionViewModel;
        _rpc = rpc;
        _rpcEventService = rpcEventService;
        VisibleParticipants = new ObservableCollection<string>();

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
    /// Collection of IDs of currently visible participants.
    /// </summary>
    public ObservableCollection<string> VisibleParticipants { get; }

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
    /// Whether a participant is maximized.
    /// </summary>
    public bool IsMaximized
    {
        get => _isMaximized;
        set => SetProperty(ref _isMaximized, value);
    }

    /// <summary>
    /// The currently maximized participant.
    /// </summary>
    public ParticipantViewModel? MaximizedParticipant
    {
        get => _maximizedParticipant;
        set => SetProperty(ref _maximizedParticipant, value);
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
    /// Maximizes the specified participant.
    /// </summary>
    public void MaximizeParticipant(ParticipantViewModel participant)
    {
        MaximizedParticipant = participant;
        IsMaximized = true;
        ViewMode = VideoViewMode.VideoFocus;
        FocusedParticipant = participant;
    }

    /// <summary>
    /// Restores the grid view.
    /// </summary>
    public void RestoreGridView()
    {
        IsMaximized = false;
        MaximizedParticipant = null;
        ViewMode = VideoViewMode.Grid;
        FocusedParticipant = null;
    }

    /// <summary>
    /// Updates the list of visible participants.
    /// </summary>
    public void UpdateVisibleParticipants(System.Collections.Generic.List<string> visibleIds)
    {
        if (visibleIds == null)
        {
            return;
        }

        // Only update if changed to avoid unnecessary notifications
        if (!visibleIds.SequenceEqual(VisibleParticipants))
        {
            VisibleParticipants.Clear();
            foreach (string id in visibleIds)
            {
                if (!VisibleParticipants.Contains(id))
                {
                    VisibleParticipants.Add(id);
                    // Find IP for this email (id is email)
                    string? ip = _meetingSessionViewModel.IpToMailMap.FirstOrDefault(x => x.Value == id).Key;

                    if (!string.IsNullOrEmpty(ip))
                    {
                        SubscriberPacket subscriberPacket = new SubscriberPacket(ip, true);
                        _rpc?.Call("subscribeAsViewer", subscriberPacket.Serialize());
                    }
                }
            }

            foreach (string existingId in VisibleParticipants.ToList())
            {
                if (!visibleIds.Contains(existingId))
                {
                    VisibleParticipants.Remove(existingId);

                    // Find IP for this email (existingId is email)
                    string? ip = _meetingSessionViewModel.IpToMailMap.FirstOrDefault(x => x.Value == existingId).Key;

                    if (!string.IsNullOrEmpty(ip))
                    {
                        SubscriberPacket subscriberPacket = new SubscriberPacket(ip, true);
                        _rpc?.Call("unSubscribeAsViewer", subscriberPacket.Serialize());
                    }
                }
            }


            Console.WriteLine($"[App] Visible participants updated: {string.Join(", ", visibleIds)}");
            // Notify RPC or other services about visibility change if needed
            // _rpc?.UpdateVisibleParticipants(visibleIds);
        }
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
        if (_rpcEventService == null)
        {
            return;
        }

        // Subscribe to UPDATE_UI to receive video/screen frames
        _rpcEventService.FrameReceived += OnFrameReceived;

        // Subscribe to STOP_SHARE to clear screen frames
        _rpcEventService.StopShareReceived += OnStopShare;
    }

    /// <summary>
    /// Handles incoming video/screen frames from RPC.
    /// </summary>
    private void OnFrameReceived(object? sender, RpcDataEventArgs e)
    {
        try
        {
            RImage rImage = RImage.Deserialize(e.Data.ToArray());
            Application.Current.Dispatcher.Invoke(() => UpdateParticipantFrame(rImage));
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is IndexOutOfRangeException || ex is IOException)
        {
            System.Diagnostics.Debug.WriteLine($"Error processing frame: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles stop share signal from RPC.
    /// </summary>
    private void OnStopShare(object? sender, RpcDataEventArgs e)
    {
        try
        {
            // If data is not empty, try to read IP
            if (e.Data.Length > 0)
            {
                // Placeholder for future logic
            }
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnStopShare: {ex.Message}");
        }
    }

    private void UpdateParticipantFrame(RImage rImage)
    {
        // Find participant by IP (mapped to Email)
        string email = _meetingSessionViewModel.IpToMailMap.GetValueOrDefault(rImage.Ip) ?? string.Empty;

        ParticipantViewModel? participant = Participants.FirstOrDefault(p => p.User.Email == email);

        Console.WriteLine($"[App] UPDATE_UI 3 Updating frame for participant with IP {rImage.Ip} mapped to email {email}" + participant);
        if (participant != null)
        {
            WriteableBitmap? bitmapSource = CreateBitmapSourceFromIntArray(rImage.Image);

            // If participant is marked as screen sharing, update screen frame
            // Otherwise update video frame
            // Note: This logic depends on IsScreenSharing flag being set correctly via other means (e.g. separate RPC call)
            // OR we can infer it.
            Console.WriteLine($"[App] UPDATE_UI 4 Created bitmap source for participant {email} {participant.IsScreenSharing} " + bitmapSource + rImage.Image.Length);

            if (participant.IsScreenSharing)
            {
                Console.WriteLine("[App] UPDATE_UI 5 Updating screen frame for participant " + participant);
                participant.Frame = bitmapSource;
                // Also ensure we switch to screen focus if this is the first frame
                if (ViewMode != VideoViewMode.ScreenFocus && FocusedParticipant == participant)
                {
                    ViewMode = VideoViewMode.ScreenFocus;
                }
            }
            else
            {
                participant.Frame = bitmapSource;
            }
        }
    }

    private static WriteableBitmap? CreateBitmapSourceFromIntArray(int[][] pixels)
    {
        int height = pixels.Length;
        if (height == 0)
        {
            return null;
        }
        int width = pixels[0].Length;

        WriteableBitmap wBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

        // Flatten the array
        int[] flatPixels = new int[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                flatPixels[y * width + x] = pixels[y][x];
            }
        }

        wBitmap.WritePixels(new Int32Rect(0, 0, width, height), flatPixels, width * 4, 0);
        wBitmap.Freeze(); // Make it cross-thread accessible if needed, though we are on UI thread here.
        return wBitmap;
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
        System.Diagnostics.Debug.WriteLine("[App] Updating sorted participants list.");
        Console.WriteLine($"[App] Updating sorted participants list. Count: {Participants.Count}");
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
            if (participant.IsScreenSharing && participant.HasFrame)
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_rpcEventService != null)
            {
                _rpcEventService.FrameReceived -= OnFrameReceived;
                _rpcEventService.StopShareReceived -= OnStopShare;
            }

            Participants.CollectionChanged -= OnParticipantsChanged;
            foreach (ParticipantViewModel participant in Participants)
            {
                participant.PropertyChanged -= OnParticipantPropertyChanged;
            }
        }
    }
}


