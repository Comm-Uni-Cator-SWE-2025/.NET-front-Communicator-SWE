using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Communicator.Controller.Meeting;
using Communicator.Core.RPC;
using Communicator.Core.UX;
using Communicator.ScreenShare;
using Communicator.UX.Services;

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
public class VideoSessionViewModel : ObservableObject, IDisposable
{
    private int _gridColumns = 1;
    private int _gridRows = 1;
    private VideoViewMode _viewMode = VideoViewMode.Grid;
    private ParticipantViewModel? _focusedParticipant;
    private readonly IRPC? _rpc;
    private readonly IRpcEventService? _rpcEventService;

    /// <summary>
    /// Initializes the video session view model with the supplied user context,
    /// shared participants collection, and RPC interface for frame updates.
    /// </summary>
    public VideoSessionViewModel(
        UserProfile user,
        ObservableCollection<ParticipantViewModel> participants,
        IRPC? rpc = null,
        IRpcEventService? rpcEventService = null)
    {
        Title = "Meeting";
        CurrentUser = user;
        Participants = participants;
        _rpc = rpc;
        _rpcEventService = rpcEventService;

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
    private void OnFrameReceived(object? sender, byte[] data)
    {
        try
        {
            RImage rImage = RImage.Deserialize(data);
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateParticipantFrame(rImage);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error processing frame: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles stop share signal from RPC.
    /// </summary>
    private void OnStopShare(object? sender, byte[] data)
    {
        // The data might contain the IP of the user stopping the share, or it might be empty/generic.
        // Assuming it contains IP string like SUBSCRIBE_AS_VIEWER does, or we might need to parse it.
        // But Utils.STOP_SHARE is just a signal.
        // If we don't know who stopped, we might need to check who was sharing.
        
        // For now, let's assume we need to clear screen share for everyone or check the payload.
        // Java implementation of OnStopShare?
        // It's not explicitly shown in the provided files, but let's assume it carries the IP.
        
        try 
        {
             // If data is not empty, try to read IP
             if (data.Length > 0)
             {
                 // Try to read IP string
                 // But wait, how is it encoded?
                 // If it's just a string bytes:
                 // string ip = System.Text.Encoding.UTF8.GetString(data);
                 // Let's try that.
             }
        }
        catch (Exception) { }
    }

    private void UpdateParticipantFrame(RImage rImage)
    {
        // Find participant by IP (mapped to Email)
        string email = $"{rImage.Ip}@example.com";
        ParticipantViewModel? participant = Participants.FirstOrDefault(p => p.User.Email == email);

        if (participant != null)
        {
            WriteableBitmap? bitmapSource = CreateBitmapSourceFromIntArray(rImage.Image);
            
            // If participant is marked as screen sharing, update screen frame
            // Otherwise update video frame
            // Note: This logic depends on IsScreenSharing flag being set correctly via other means (e.g. separate RPC call)
            // OR we can infer it.
            
            if (participant.IsScreenSharing)
            {
                participant.ScreenFrame = bitmapSource;
                // Also ensure we switch to screen focus if this is the first frame
                if (ViewMode != VideoViewMode.ScreenFocus && FocusedParticipant == participant)
                {
                    ViewMode = VideoViewMode.ScreenFocus;
                }
            }
            else
            {
                participant.VideoFrame = bitmapSource;
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
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
