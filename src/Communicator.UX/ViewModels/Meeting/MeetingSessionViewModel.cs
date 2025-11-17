using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Communicator.Controller.Meeting;
using Communicator.Core.RPC;
using Communicator.Core.UX;
using Communicator.Core.UX.Services;
using Communicator.ScreenShare;
using Communicator.UX.Services;

namespace Communicator.UX.ViewModels.Meeting;

/// <summary>
/// Unified view model that manages the entire meeting session, including participants,
/// sub-features (video, screenshare, chat, whiteboard), navigation, and toolbar state.
/// Merges the functionality of MeetingShellViewModel and MeetingViewModel.
/// </summary>
public class MeetingSessionViewModel : ObservableObject, INavigationScope, IDisposable
{
    private readonly MeetingToolbarViewModel _toolbarViewModel;
    private readonly IToastService _toastService;
    private readonly IHandWaveService _handWaveService;
    private readonly ICloudConfigService _cloudConfig;
    private readonly UserProfile _currentUser;
    private readonly Stack<MeetingTabViewModel> _backStack = new();
    private readonly Stack<MeetingTabViewModel> _forwardStack = new();
    private MeetingTabViewModel? _currentTab;
    private bool _suppressSelectionNotifications;
    private object? _currentPage;
    
    // Meeting Session State
    private MeetingSession? _currentMeeting;
    private readonly IRPC? _rpc;

    // Toolbar State
    private bool _isMuted;
    private bool _isCameraOn = true;
    private bool _isHandRaised;
    private bool _isScreenSharing;

    // Side Panel Support
    private object? _sidePanelContent;
    private bool _isSidePanelOpen;
    private string _sidePanelTitle = string.Empty;

    // Quick Doubt Feature
    private string _quickDoubtMessage = string.Empty;
    private bool _isQuickDoubtBubbleOpen;
    private DateTime? _quickDoubtTimestamp;
    private string _quickDoubtSentMessage = string.Empty;

    // Meeting State
    private bool _isMeetingActive;

    /// <summary>
    /// Builds meeting tabs for the supplied user and initializes navigation state.
    /// Services are injected via constructor for better testability.
    /// </summary>
    public MeetingSessionViewModel(
        UserProfile currentUser,
        IToastService toastService,
        IHandWaveService handWaveService,
        ICloudConfigService cloudConfig,
        IRPC? rpc = null)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
        _handWaveService = handWaveService ?? throw new ArgumentNullException(nameof(handWaveService));
        _cloudConfig = cloudConfig ?? throw new ArgumentNullException(nameof(cloudConfig));
        _rpc = rpc;

        // Initialize participants collection
        Participants = new ObservableCollection<ParticipantViewModel>();

        // Create sub-ViewModels with shared participant collection
        VideoSession = new VideoSessionViewModel(_currentUser, Participants, _rpc);
        Chat = new ChatViewModel(_currentUser, _toastService);
        Whiteboard = new WhiteboardViewModel(_currentUser);
        AIInsights = new AIInsightsViewModel(_currentUser);

        // Create toolbar with tabs
        _toolbarViewModel = new MeetingToolbarViewModel(CreateTabs());
        _toolbarViewModel.SelectedTabChanged += OnSelectedTabChanged;
        _currentTab = _toolbarViewModel.SelectedTab;
        if (_currentTab != null)
        {
            CurrentPage = _currentTab.ContentViewModel;
        }

        // Subscribe to HandWave messages
        _handWaveService.QuickDoubtReceived += OnQuickDoubtReceived;

        // Initialize commands
        ToggleMuteCommand = new RelayCommand(_ => ToggleMute());
        ToggleCameraCommand = new RelayCommand(_ => ToggleCamera());
        ToggleHandCommand = new RelayCommand(_ => ToggleHandRaised());
        ToggleScreenShareCommand = new RelayCommand(_ => ToggleScreenShare());
        LeaveMeetingCommand = new RelayCommand(async _ => await LeaveMeetingAsync());
        ToggleChatPanelCommand = new RelayCommand(_ => ToggleChatPanel());
        ToggleParticipantsPanelCommand = new RelayCommand(_ => ToggleParticipantsPanel());
        CloseSidePanelCommand = new RelayCommand(_ => CloseSidePanel());
        SendQuickDoubtCommand = new RelayCommand(async _ => await SendQuickDoubtAsync(), _ => CanSendQuickDoubt());

        RaiseNavigationStateChanged();

        // Start the meeting session
        StartMeeting();

        // Connect to HandWave and RPC
        _ = InitializeServicesAsync();
    }

    #region Properties

    public MeetingToolbarViewModel Toolbar => _toolbarViewModel;

    public object? CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    /// <summary>
    /// Collection of all participants in the meeting with their UI state.
    /// </summary>
    public ObservableCollection<ParticipantViewModel> Participants { get; }

    /// <summary>
    /// Sub-ViewModel for video session functionality (includes screen share).
    /// </summary>
    public VideoSessionViewModel VideoSession { get; }

    /// <summary>
    /// Sub-ViewModel for chat functionality.
    /// </summary>
    public ChatViewModel Chat { get; }

    /// <summary>
    /// Sub-ViewModel for whiteboard/canvas functionality.
    /// </summary>
    public WhiteboardViewModel Whiteboard { get; }

    /// <summary>
    /// Sub-ViewModel for AI insights functionality.
    /// </summary>
    public AIInsightsViewModel AIInsights { get; }

    public bool IsMeetingActive
    {
        get => _isMeetingActive;
        private set => SetProperty(ref _isMeetingActive, value);
    }

    public bool IsMuted
    {
        get => _isMuted;
        private set => SetProperty(ref _isMuted, value);
    }

    public bool IsCameraOn
    {
        get => _isCameraOn;
        private set => SetProperty(ref _isCameraOn, value);
    }

    public bool IsHandRaised
    {
        get => _isHandRaised;
        private set => SetProperty(ref _isHandRaised, value);
    }

    public bool IsScreenSharing
    {
        get => _isScreenSharing;
        private set => SetProperty(ref _isScreenSharing, value);
    }

    public object? SidePanelContent
    {
        get => _sidePanelContent;
        private set => SetProperty(ref _sidePanelContent, value);
    }

    public bool IsSidePanelOpen
    {
        get => _isSidePanelOpen;
        private set => SetProperty(ref _isSidePanelOpen, value);
    }

    public string SidePanelTitle
    {
        get => _sidePanelTitle;
        private set => SetProperty(ref _sidePanelTitle, value);
    }

    // Quick Doubt Properties
    public string QuickDoubtMessage
    {
        get => _quickDoubtMessage;
        set => SetProperty(ref _quickDoubtMessage, value);
    }

    public bool IsQuickDoubtBubbleOpen
    {
        get => _isQuickDoubtBubbleOpen;
        private set => SetProperty(ref _isQuickDoubtBubbleOpen, value);
    }

    public DateTime? QuickDoubtTimestamp
    {
        get => _quickDoubtTimestamp;
        private set => SetProperty(ref _quickDoubtTimestamp, value);
    }

    public string QuickDoubtSentMessage
    {
        get => _quickDoubtSentMessage;
        private set => SetProperty(ref _quickDoubtSentMessage, value);
    }

    #endregion

    #region Commands

    public ICommand ToggleMuteCommand { get; }
    public ICommand ToggleCameraCommand { get; }
    public ICommand ToggleHandCommand { get; }
    public ICommand ToggleScreenShareCommand { get; }
    public ICommand LeaveMeetingCommand { get; }
    public ICommand ToggleChatPanelCommand { get; }
    public ICommand ToggleParticipantsPanelCommand { get; }
    public ICommand CloseSidePanelCommand { get; }
    public ICommand SendQuickDoubtCommand { get; }

    #endregion

    #region Navigation

    public bool CanNavigateBack => _backStack.Count > 0;

    public bool CanNavigateForward => _forwardStack.Count > 0;

    public event EventHandler? NavigationStateChanged;

    public void NavigateBack()
    {
        if (!CanNavigateBack || _currentTab == null)
        {
            return;
        }

        MeetingTabViewModel target = _backStack.Pop();
        _forwardStack.Push(_currentTab);
        ActivateTabFromHistory(target);
    }

    public void NavigateForward()
    {
        if (!CanNavigateForward || _currentTab == null)
        {
            return;
        }

        MeetingTabViewModel target = _forwardStack.Pop();
        _backStack.Push(_currentTab);
        ActivateTabFromHistory(target);
    }

    private void RaiseNavigationStateChanged()
    {
        NavigationStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnSelectedTabChanged(object? sender, TabChangedEventArgs e)
    {
        MeetingTabViewModel? tab = e.Tab;
        if (_suppressSelectionNotifications || tab == null || tab == _currentTab)
        {
            return;
        }

        if (_currentTab != null)
        {
            _backStack.Push(_currentTab);
        }

        _forwardStack.Clear();
        ActivateTab(tab);
    }

    private void ActivateTab(MeetingTabViewModel tab)
    {
        _currentTab = tab;
        CurrentPage = tab.ContentViewModel;
        RaiseNavigationStateChanged();
    }

    private void ActivateTabFromHistory(MeetingTabViewModel tab)
    {
        _suppressSelectionNotifications = true;
        _toolbarViewModel.SelectedTab = tab;
        _suppressSelectionNotifications = false;
        ActivateTab(tab);
    }

    private IEnumerable<MeetingTabViewModel> CreateTabs()
    {
        yield return new MeetingTabViewModel("AI Insights", AIInsights);
        yield return new MeetingTabViewModel("Meeting", VideoSession);
        yield return new MeetingTabViewModel("Canvas", Whiteboard);
    }

    #endregion

    #region Meeting Management

    /// <summary>
    /// Starts a new meeting session with the current user as the first participant.
    /// </summary>
    private void StartMeeting()
    {
        if (_currentUser != null)
        {
            // Create a new meeting session
            _currentMeeting = new MeetingSession(_currentUser.Email ?? "unknown", SessionMode.Class);
            _currentMeeting.AddParticipant(_currentUser);

            // Add current user to participants list
            AddParticipant(_currentUser);

            // TODO: Call RPC to create meeting on backend
            // Example: await _rpc?.Call("createMeeting", SerializeMeetingInfo(_currentMeeting));
            // Backend should return meeting ID and initial state
        }

        IsMeetingActive = true;
    }

    /// <summary>
    /// Adds a participant to the meeting.
    /// </summary>
    private void AddParticipant(UserProfile user)
    {
        if (_currentMeeting == null)
        {
            return;
        }

        // Check if participant already exists
        if (Participants.Any(p => p.User.Email == user.Email))
        {
            return;
        }

        // Add to meeting session
        _currentMeeting.AddParticipant(user);

        // Create ViewModel wrapper and add to UI collection
        var participantVM = new ParticipantViewModel(user);
        Participants.Add(participantVM);

        _toastService.ShowInfo($"{user.DisplayName ?? user.Email} joined the meeting");

        // TODO: Notify backend of new participant (if current user is host)
        // Example: await _rpc?.Call("addParticipant", SerializeUserProfile(user));
    }

    /// <summary>
    /// Removes a participant from the meeting.
    /// </summary>
    private void RemoveParticipant(string email)
    {
        ParticipantViewModel? participant = Participants.FirstOrDefault(p => p.User.Email == email);
        if (participant != null)
        {
            Participants.Remove(participant);
            _toastService.ShowInfo($"{participant.DisplayName} left the meeting");

            // TODO: Notify backend of participant removal
            // Example: await _rpc?.Call("removeParticipant", Encoding.UTF8.GetBytes(email));
        }
    }

    #endregion

    #region Toolbar Actions

    private void ToggleMute()
    {
        if (_currentMeeting == null || _rpc == null)
        {
            IsMuted = !IsMuted;
            return;
        }

        IsMuted = !IsMuted;

        // TODO: Call RPC to toggle audio
        // _rpc.CallAsync(IsMuted ? Utils.STOP_AUDIO_CAPTURE : "START_AUDIO_CAPTURE", Array.Empty<byte>());

        // Update current user's participant state
        ParticipantViewModel? currentParticipant = Participants.FirstOrDefault(p => p.User.Email == _currentUser.Email);
        if (currentParticipant != null)
        {
            currentParticipant.IsMuted = IsMuted;
        }
    }

    private void ToggleCamera()
    {
        if (_currentMeeting == null || _rpc == null)
        {
            IsCameraOn = !IsCameraOn;
            return;
        }

        IsCameraOn = !IsCameraOn;

        // TODO: Call RPC to toggle video
        // _rpc.CallAsync(IsCameraOn ? Utils.START_VIDEO_CAPTURE : Utils.STOP_VIDEO_CAPTURE, Array.Empty<byte>());

        // Update current user's participant state
        ParticipantViewModel? currentParticipant = Participants.FirstOrDefault(p => p.User.Email == _currentUser.Email);
        if (currentParticipant != null)
        {
            currentParticipant.IsCameraOn = IsCameraOn;
        }
    }

    private void ToggleHandRaised()
    {
        IsHandRaised = !IsHandRaised;

        // Update current user's participant state
        ParticipantViewModel? currentParticipant = Participants.FirstOrDefault(p => p.User.Email == _currentUser.Email);
        if (currentParticipant != null)
        {
            currentParticipant.IsHandRaised = IsHandRaised;
        }

        if (IsHandRaised)
        {
            // Open the quick doubt bubble
            IsQuickDoubtBubbleOpen = true;
        }
        else
        {
            // Close the bubble and clear any doubt data
            ClearQuickDoubt();
        }
    }

    private void ToggleScreenShare()
    {
        if (_currentMeeting == null || _rpc == null)
        {
            IsScreenSharing = !IsScreenSharing;
            return;
        }

        IsScreenSharing = !IsScreenSharing;

        // TODO: Call RPC to toggle screen share
        // _rpc.CallAsync(IsScreenSharing ? Utils.START_SCREEN_CAPTURE : Utils.STOP_SCREEN_CAPTURE, Array.Empty<byte>());

        // Update current user's participant state
        ParticipantViewModel? currentParticipant = Participants.FirstOrDefault(p => p.User.Email == _currentUser.Email);
        if (currentParticipant != null)
        {
            currentParticipant.IsScreenSharing = IsScreenSharing;
        }
    }

    #endregion

    #region Side Panel Management

    /// <summary>
    /// Toggles the chat side panel open/closed.
    /// </summary>
    private void ToggleChatPanel()
    {
        if (IsSidePanelOpen && SidePanelContent is ChatViewModel)
        {
            // If chat panel is already open, close it
            CloseSidePanel();
        }
        else
        {
            // Open chat panel
            SidePanelContent = Chat;
            SidePanelTitle = "Chat";
            IsSidePanelOpen = true;
        }
    }

    /// <summary>
    /// Toggles the participants side panel open/closed.
    /// </summary>
    private void ToggleParticipantsPanel()
    {
        if (IsSidePanelOpen && SidePanelContent is ParticipantsListViewModel)
        {
            // If participants panel is already open, close it
            CloseSidePanel();
        }
        else
        {
            // Open participants panel
            var participantsListVM = new ParticipantsListViewModel(Participants);
            
            // Subscribe to participant count changes to update the title dynamically
            participantsListVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ParticipantsListViewModel.ParticipantCount))
                {
                    UpdateParticipantsPanelTitle(participantsListVM.ParticipantCount);
                }
            };
            
            SidePanelContent = participantsListVM;
            UpdateParticipantsPanelTitle(participantsListVM.ParticipantCount);
            IsSidePanelOpen = true;
        }
    }

    /// <summary>
    /// Updates the side panel title with the current participant count.
    /// </summary>
    private void UpdateParticipantsPanelTitle(int count)
    {
        SidePanelTitle = $"Participants ({count})";
    }

    /// <summary>
    /// Closes the side panel.
    /// </summary>
    private void CloseSidePanel()
    {
        IsSidePanelOpen = false;
        SidePanelContent = null;
        SidePanelTitle = string.Empty;
    }

    #endregion

    #region Quick Doubt / HandWave

    private bool CanSendQuickDoubt()
    {
        return !string.IsNullOrWhiteSpace(QuickDoubtMessage) && _handWaveService.IsConnected;
    }

    /// <summary>
    /// Sends quick doubt message via HandWave cloud function.
    /// </summary>
    private async Task SendQuickDoubtAsync()
    {
        if (string.IsNullOrWhiteSpace(QuickDoubtMessage))
        {
            return;
        }

        try
        {
            // Capture the sent message and timestamp
            QuickDoubtSentMessage = QuickDoubtMessage.Trim();
            QuickDoubtTimestamp = DateTime.Now;

            // Send via cloud function
            await _handWaveService.SendQuickDoubtAsync(_currentUser.DisplayName ?? "Unknown User", QuickDoubtSentMessage).ConfigureAwait(false);

            // Clear the input field for next message
            QuickDoubtMessage = string.Empty;

            // Keep the bubble open to show the sent message
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
        {
            _toastService.ShowError($"Failed to send quick doubt: {ex.Message}");
            // Restore the message if sending failed
            QuickDoubtMessage = QuickDoubtSentMessage;
            QuickDoubtSentMessage = string.Empty;
            QuickDoubtTimestamp = null;
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    /// <summary>
    /// Handler for receiving quick doubt messages from cloud via SignalR.
    /// </summary>
    private void OnQuickDoubtReceived(string message)
    {
        // Display received doubt in UI thread
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            _toastService.ShowInfo($"Quick Doubt: {message}");
        });
    }

    private void ClearQuickDoubt()
    {
        QuickDoubtMessage = string.Empty;
        QuickDoubtSentMessage = string.Empty;
        QuickDoubtTimestamp = null;
        IsQuickDoubtBubbleOpen = false;
    }

    #endregion

    #region Service Initialization

    /// <summary>
    /// Initializes RPC and HandWave cloud services.
    /// </summary>
    private async Task InitializeServicesAsync()
    {
        try
        {
            // Connect to HandWave cloud service
            await _handWaveService.ConnectAsync(_currentUser.DisplayName ?? "Unknown User").ConfigureAwait(false);
            _toastService.ShowSuccess("Connected to HandWave service");

            // Subscribe to RPC events for new participants
            _rpc?.Subscribe(Utils.SUBSCRIBE_AS_VIEWER, (args) =>
            {
                string viewerIP = System.Text.Encoding.UTF8.GetString(args);
                UserProfile newUser = new(
                    email: $"{viewerIP}@example.com",
                    displayName: viewerIP,
                    role: ParticipantRole.Student,
                    logoUrl: null
                );

                // Update collection on UI thread
                System.Windows.Application.Current.Dispatcher.Invoke(() => AddParticipant(newUser));
                return Array.Empty<byte>();
            });
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
        {
            _toastService.ShowError($"Failed to initialize services: {ex.Message}");
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    /// <summary>
    /// Leaves the meeting and disconnects from all services.
    /// </summary>
    private async Task LeaveMeetingAsync()
    {
        try
        {
            // TODO: Stop all RPC streaming features before leaving
            // if (_rpc != null)
            // {
            //     // Stop video capture if enabled
            //     if (IsCameraOn)
            //     {
            //         await _rpc.Call(Utils.STOP_VIDEO_CAPTURE, Array.Empty<byte>());
            //     }
            //
            //     // Stop screen share if enabled
            //     if (IsScreenSharing)
            //     {
            //         await _rpc.Call(Utils.STOP_SCREEN_CAPTURE, Array.Empty<byte>());
            //     }
            //
            //     // Stop audio capture if enabled
            //     if (!IsMuted)
            //     {
            //         await _rpc.Call(Utils.STOP_AUDIO_CAPTURE, Array.Empty<byte>());
            //     }
            //
            //     // Notify backend of leaving
            //     await _rpc.Call("leaveMeeting", Encoding.UTF8.GetBytes(_currentUser.Email ?? ""));
            // }

            // Disconnect from HandWave cloud service
            await _handWaveService.DisconnectAsync().ConfigureAwait(false);

            // Clear meeting state
            _currentMeeting = null;
            IsMeetingActive = false;
            Participants.Clear();

            _toastService.ShowSuccess("Left the meeting");
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
        {
            _toastService.ShowError($"Error leaving meeting: {ex.Message}");
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Unsubscribe from HandWave events
            _handWaveService.QuickDoubtReceived -= OnQuickDoubtReceived;

            // Disconnect from HandWave
            _ = _handWaveService.DisconnectAsync();

            // Dispose managed resources
            _toolbarViewModel.SelectedTabChanged -= OnSelectedTabChanged;
            _backStack.Clear();
            _forwardStack.Clear();
        }
    }

    #endregion
}
