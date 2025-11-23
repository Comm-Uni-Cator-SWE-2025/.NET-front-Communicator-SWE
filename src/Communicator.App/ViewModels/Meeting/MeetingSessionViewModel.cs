/*
 * -----------------------------------------------------------------------------
 *  File: MeetingSessionViewModel.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Communicator.App.Services;
using Communicator.Controller.Meeting;
using Communicator.Controller.Serialization;
using Communicator.Core.RPC;
using Communicator.Core.UX;
using Communicator.Core.UX.Services;
using Communicator.Networking;
using Communicator.ScreenShare;
using Communicator.UX.Analytics.ViewModels;
using Communicator.UX.Canvas.ViewModels;

namespace Communicator.App.ViewModels.Meeting;

/// <summary>
/// Unified view model that manages the entire meeting session, including participants,
/// sub-features (video, screenshare, chat, whiteboard), navigation, and toolbar state.
/// Merges the functionality of MeetingShellViewModel and MeetingViewModel.
/// </summary>
public sealed class MeetingSessionViewModel : ObservableObject, IDisposable
{
    private readonly MeetingToolbarViewModel _toolbarViewModel;
    private readonly IToastService _toastService;
    private readonly ICloudMessageService _cloudMessageService;
    private readonly ICloudConfigService _cloudConfig;
    private readonly INavigationService _navigationService;
    private readonly IThemeService _themeService;
    private readonly INetworking _networking;
    private readonly UserProfile _currentUser;
    private MeetingTabViewModel? _currentTab;
    private object? _currentPage;

    private Dictionary<string, string> _ipToMailMap = new Dictionary<string, string>();

    // Meeting Session State
    private MeetingSession? _currentMeeting;
    private readonly IRPC? _rpc;
    private readonly IRpcEventService? _rpcEventService;

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

    // Collection of active Quick Doubts for stacking
    public ObservableCollection<QuickDoubtItem> ActiveQuickDoubts { get; } = new ObservableCollection<QuickDoubtItem>();

    // Meeting State
    private bool _isMeetingActive;

    /// <summary>
    /// Builds meeting tabs for the supplied user and initializes navigation state.
    /// Services are injected via constructor for better testability.
    /// </summary>
    public MeetingSessionViewModel(
        UserProfile currentUser,
        MeetingSession? meetingSession,
        IToastService toastService,
        ICloudMessageService cloudMessageService,
        ICloudConfigService cloudConfig,
        INavigationService navigationService,
        IThemeService themeService,
        INetworking networking,
        IRPC? rpc = null,
        IRpcEventService? rpcEventService = null)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _currentMeeting = meetingSession; // Initialize with passed session if available
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
        _cloudMessageService = cloudMessageService ?? throw new ArgumentNullException(nameof(cloudMessageService));
        _cloudConfig = cloudConfig ?? throw new ArgumentNullException(nameof(cloudConfig));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _networking = networking ?? throw new ArgumentNullException(nameof(networking));
        _rpc = rpc;
        _rpcEventService = rpcEventService;

        // Initialize participants collection
        Participants = new ObservableCollection<ParticipantViewModel>();

        // Create sub-ViewModels with shared participant collection
        VideoSession = new VideoSessionViewModel(_currentUser, Participants, this, _rpc, _rpcEventService);
        Chat = new ChatViewModel(_currentUser, _toastService, _rpc, _rpcEventService);

        // Initialize Whiteboard (Canvas)
        bool isHost = _currentMeeting != null && _currentMeeting.CreatedBy == _currentUser.Email;
        if (isHost)
        {
            Whiteboard = new HostViewModel(_networking, _rpc!);
        }
        else
        {
            var clientVM = new ClientViewModel(_networking, _rpc!);
            Whiteboard = clientVM;
            // Initialize client VM after creation (but ideally after joining meeting)
            // Since we are in MeetingSessionViewModel, we assume we are joining/starting
            clientVM.Initialize();
        }

        AIInsights = new AnalyticsViewModel(_themeService);

        // Create toolbar with tabs
        _toolbarViewModel = new MeetingToolbarViewModel(CreateTabs());
        _toolbarViewModel.SelectedTabChanged += OnSelectedTabChanged;
        _currentTab = _toolbarViewModel.SelectedTab;
        if (_currentTab != null)
        {
            CurrentPage = _currentTab.ContentViewModel;
        }

        // Subscribe to cloud message events
        _cloudMessageService.MessageReceived += OnCloudMessageReceived;

        // Initialize commands
        ToggleMuteCommand = new RelayCommand(_ => ToggleMute());
        ToggleCameraCommand = new RelayCommand(_ => ToggleCamera());
        ToggleHandCommand = new RelayCommand(_ => ToggleHandRaised());
        ToggleScreenShareCommand = new RelayCommand(_ => ToggleScreenShare());
        LeaveMeetingCommand = new RelayCommand(async _ => await LeaveMeetingAsync().ConfigureAwait(true));
        ToggleChatPanelCommand = new RelayCommand(_ => ToggleChatPanel());
        ToggleParticipantsPanelCommand = new RelayCommand(_ => ToggleParticipantsPanel());
        CloseSidePanelCommand = new RelayCommand(_ => CloseSidePanel());
        SendQuickDoubtCommand = new RelayCommand(async _ => await SendQuickDoubtAsync().ConfigureAwait(true), _ => CanSendQuickDoubt());
        DismissQuickDoubtCommand = new RelayCommand(param => DismissQuickDoubt(param as string));

        // Start the meeting session
        StartMeeting();

        // Subscribe to RPC events via service (avoids late subscription error)
        if (_rpcEventService != null)
        {
            _rpcEventService.ParticipantsListUpdated += OnParticipantsListUpdated;
            _rpcEventService.Logout += OnLogout;
            _rpcEventService.EndMeeting += OnEndMeeting;
        }

        // Connect to HandWave and RPC
        System.Diagnostics.Debug.WriteLine("[MeetingSession] Constructor: Calling InitializeServicesAsync");
        _ = InitializeServicesAsync();
    }

    public Dictionary<string, string> IpToMailMap => _ipToMailMap;

    private void OnLogout(object? sender, RpcStringEventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.InvokeAsync(async () => {
            _toastService.ShowInfo($"Logged out: {e.Value}");
            await CleanupAndNavigateBackAsync().ConfigureAwait(false);
        });
    }

    private void OnEndMeeting(object? sender, RpcStringEventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.InvokeAsync(async () => {
            _toastService.ShowInfo($"Meeting ended: {e.Value}");
            await CleanupAndNavigateBackAsync().ConfigureAwait(false);
        });
    }

    private void OnParticipantsListUpdated(object? sender, RpcStringEventArgs e)
    {
        string participantsJson = e.Value;
        try
        {
            // Deserialize the list of participants
            // Expected format: {"host:port": {"email": "...", "displayName": "...", "role": "..."}}
            // Map<String, UserProfile> from Java (Key is "IP:Port")
            System.Diagnostics.Debug.WriteLine($"[MeetingSession] ParticipantsListUpdated JSON: {participantsJson}");
            Dictionary<string, UserProfile>? nodeToProfileMap = DataSerializer.Deserialize<Dictionary<string, UserProfile>>(Encoding.UTF8.GetBytes(participantsJson));

            if (nodeToProfileMap == null)
            {
                return;
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                // Sync our list with the backend list

                // 1. Add new participants or update existing ones
                foreach (KeyValuePair<string, UserProfile> kvp in nodeToProfileMap)
                {
                    UserProfile profile = kvp.Value;
                    string ipPort = kvp.Key;
                    string ip = ipPort;
                    int colonIndex = ipPort.IndexOf(':', StringComparison.Ordinal);
                    if (colonIndex >= 0)
                    {
                        ip = ipPort.Substring(0, colonIndex);
                    }

                    _ipToMailMap[ip] = profile.Email ?? "";
                    // Check if we already have this participant by Email
                    ParticipantViewModel? existingParticipant = Participants.FirstOrDefault(p => p.User.Email == profile.Email);
                    if (existingParticipant == null)
                    {
                        // Ensure role is set if missing (default to STUDENT)
                        if (profile.Role == 0) // Assuming 0 is default/unknown
                        {
                            profile.Role = ParticipantRole.STUDENT;
                        }
                        AddParticipant(profile);
                    }
                    else
                    {
                        // Update display name if changed
                        if (existingParticipant.User.DisplayName != profile.DisplayName)
                        {
                            existingParticipant.User.DisplayName = profile.DisplayName;
                        }
                    }
                }

                // 2. Remove participants not in the list (except self)
                var emailsInBackend = new HashSet<string>(nodeToProfileMap.Values.Select(p => p.Email).Where(e => e != null)!);

                // Don't remove ourselves
                if (_currentUser.Email != null)
                {
                    emailsInBackend.Add(_currentUser.Email);
                }

                var toRemove = Participants
                    .Where(p => p.User.Email != null && !emailsInBackend.Contains(p.User.Email))
                    .ToList();

                foreach (ParticipantViewModel p in toRemove)
                {
                    if (p.User.Email != null)
                    {
                        RemoveParticipant(p.User.Email);
                    }
                }
            });
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is System.Text.Json.JsonException)
        {
            System.Diagnostics.Debug.WriteLine($"[MeetingSession] Error parsing participant list: {ex.Message}");
        }
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
    public CanvasViewModel Whiteboard { get; }

    /// <summary>
    /// Sub-ViewModel for AI insights functionality (Powered by Analytics).
    /// </summary>
    public AnalyticsViewModel AIInsights { get; }

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
    public ICommand DismissQuickDoubtCommand { get; }

    #endregion

    private void OnSelectedTabChanged(object? sender, TabChangedEventArgs e)
    {
        MeetingTabViewModel? tab = e.Tab;
        if (tab == null || tab == _currentTab)
        {
            return;
        }

        ActivateTab(tab);
    }

    private void ActivateTab(MeetingTabViewModel tab)
    {
        _currentTab = tab;
        CurrentPage = tab.ContentViewModel;
    }

    private IEnumerable<MeetingTabViewModel> CreateTabs()
    {
        yield return new MeetingTabViewModel("AI Insights", AIInsights);
        yield return new MeetingTabViewModel("Meeting", VideoSession);
        yield return new MeetingTabViewModel("Canvas", Whiteboard);
    }

    #region Meeting Management

    /// <summary>
    /// Starts a new meeting session with the current user as the first participant.
    /// </summary>
    private void StartMeeting()
    {
        if (_currentUser != null)
        {
            if (_currentMeeting == null)
            {
                // Fallback: Create a new meeting session locally if none provided
                _currentMeeting = new MeetingSession(_currentUser.Email ?? "unknown", SessionMode.CLASS);
                _currentMeeting.AddParticipant(_currentUser);
            }

            // Add current user to participants list if not already there
            AddParticipant(_currentUser);
            _ipToMailMap[Utils.GetSelfIP() ?? ""] = _currentUser.Email ?? "";

            // Add existing participants from the session (if any)
            foreach (UserProfile participant in _currentMeeting.Participants.Values)
            {
                AddParticipant(participant);
            }
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
        ParticipantViewModel participantVM = new ParticipantViewModel(user);
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

            // Check if the host left the meeting
            if (_currentMeeting != null && !string.IsNullOrEmpty(_currentMeeting.CreatedBy) && email == _currentMeeting.CreatedBy)
            {
                // If the host left, the meeting is over for everyone
                System.Windows.Application.Current.Dispatcher.InvokeAsync(async () => {
                    _toastService.ShowWarning("Host has ended the meeting.");
                    await CleanupAndNavigateBackAsync().ConfigureAwait(false);
                });
            }
        }
    }

    #endregion

    #region Toolbar Actions

    private async void ToggleMute()
    {
        if (_currentMeeting == null || _rpc == null)
        {
            IsMuted = !IsMuted;
            return;
        }

        IsMuted = !IsMuted;

        // Call RPC to toggle audio
        await _rpc.Call(IsMuted ? Utils.STOP_AUDIO_CAPTURE : Utils.START_AUDIO_CAPTURE, Array.Empty<byte>()).ConfigureAwait(true);

        // Update current user's participant state
        ParticipantViewModel? currentParticipant = Participants.FirstOrDefault(p => p.User.Email == _currentUser.Email);
        if (currentParticipant != null)
        {
            currentParticipant.IsMuted = IsMuted;
        }
    }

    private async void ToggleCamera()
    {
        if (_currentMeeting == null || _rpc == null)
        {
            IsCameraOn = !IsCameraOn;
            return;
        }

        IsCameraOn = !IsCameraOn;

        // Call RPC to toggle video
        await _rpc.Call(IsCameraOn ? Utils.START_VIDEO_CAPTURE : Utils.STOP_VIDEO_CAPTURE, Array.Empty<byte>()).ConfigureAwait(true);

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

    private async void ToggleScreenShare()
    {
        if (_currentMeeting == null || _rpc == null)
        {
            IsScreenSharing = !IsScreenSharing;
            return;
        }

        IsScreenSharing = !IsScreenSharing;

        // Call RPC to toggle screen share
        await _rpc.Call(IsScreenSharing ? Utils.START_SCREEN_CAPTURE : Utils.STOP_SCREEN_CAPTURE, Array.Empty<byte>()).ConfigureAwait(true);

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
            participantsListVM.PropertyChanged += (s, e) => {
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

    #region Quick Doubt / Cloud Messaging

    private bool CanSendQuickDoubt()
    {
        // Allow command to execute even if disconnected, so we can show an error toast
        return !string.IsNullOrWhiteSpace(QuickDoubtMessage);
    }

    /// <summary>
    /// Sends quick doubt message via cloud messaging service.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We need to log all send errors")]
    private async Task SendQuickDoubtAsync()
    {
        System.Diagnostics.Debug.WriteLine($"[MeetingSession] SendQuickDoubtAsync called. Message='{QuickDoubtMessage}'");

        if (string.IsNullOrWhiteSpace(QuickDoubtMessage))
        {
            System.Diagnostics.Debug.WriteLine("[MeetingSession] SendQuickDoubtAsync: Message is empty, aborting.");
            return;
        }

        if (!_cloudMessageService.IsConnected)
        {
            System.Diagnostics.Debug.WriteLine("[MeetingSession] SendQuickDoubtAsync: Not connected to cloud service.");
            _toastService.ShowError("Not connected to chat server. Trying to reconnect...");

            // Try to reconnect
            try
            {
                string meetingId = _currentMeeting?.MeetingId ?? "default-meeting";
                string username = _currentUser.DisplayName ?? "Unknown User";
                await _cloudMessageService.ConnectAsync(meetingId, username).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MeetingSession] Auto-reconnect failed: {ex.Message}");
            }

            if (!_cloudMessageService.IsConnected)
            {
                return;
            }
        }

        try
        {
            // Capture the sent message and timestamp
            QuickDoubtSentMessage = QuickDoubtMessage.Trim();
            QuickDoubtTimestamp = DateTime.Now;

            string meetingId = _currentMeeting?.MeetingId ?? "default-meeting";
            string username = _currentUser.DisplayName ?? "Unknown User";

            System.Diagnostics.Debug.WriteLine($"[MeetingSession] Sending QuickDoubt: '{QuickDoubtSentMessage}' to meeting '{meetingId}'");

            // Send via cloud message service
            await _cloudMessageService.SendMessageAsync(
                CloudMessageType.QuickDoubt,
                meetingId,
                username,
                QuickDoubtSentMessage).ConfigureAwait(true);

            // Clear the input field and hide it
            QuickDoubtMessage = string.Empty;

            // Note: We don't show the popup for the sender - only others will see it via SignalR
            // Keep the bubble open to show the sent message (no textbox)
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MeetingSession] SendQuickDoubtAsync FAILED: {ex}");
            _toastService.ShowError($"Failed to send quick doubt: {ex.Message}");
            // Restore the message if sending failed
            QuickDoubtMessage = QuickDoubtSentMessage;
            QuickDoubtSentMessage = string.Empty;
            QuickDoubtTimestamp = null;
        }
    }

    /// <summary>
    /// Handler for receiving cloud messages via SignalR.
    /// Routes to appropriate handler based on message type.
    /// </summary>
    private void OnCloudMessageReceived(object? sender, CloudMessageEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[MeetingSession] Received {e.MessageType}: Sender='{e.SenderName}', Message='{e.Message}'");

        switch (e.MessageType)
        {
            case CloudMessageType.QuickDoubt:
                HandleQuickDoubt(e.SenderName, e.Message);
                break;
        }
    }

    /// <summary>
    /// Handles quick doubt messages by showing popup notification.
    /// </summary>
    private void HandleQuickDoubt(string senderName, string message)
    {
        System.Diagnostics.Debug.WriteLine($"[MeetingSession] HandleQuickDoubt: Sender='{senderName}', Message='{message}'");

        if (string.IsNullOrWhiteSpace(senderName))
        {
            senderName = "Unknown";
            System.Diagnostics.Debug.WriteLine("[MeetingSession] HandleQuickDoubt: Sender name was empty, using 'Unknown'");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            System.Diagnostics.Debug.WriteLine("[MeetingSession] HandleQuickDoubt: WARNING - Message is empty!");
            message = "(no message)";
        }

        System.Diagnostics.Debug.WriteLine($"[MeetingSession] HandleQuickDoubt: Adding doubt item to ActiveQuickDoubts collection");

        // Add to collection in UI thread
        System.Windows.Application.Current.Dispatcher.Invoke(() => {
            var doubtItem = new QuickDoubtItem {
                Id = Guid.NewGuid().ToString(),
                SenderName = senderName,
                Message = message,
                Timestamp = DateTime.Now
            };
            ActiveQuickDoubts.Add(doubtItem);
            System.Diagnostics.Debug.WriteLine($"[MeetingSession] HandleQuickDoubt: Successfully added doubt. Total active doubts: {ActiveQuickDoubts.Count}");
        });
    }

    private void ClearQuickDoubt()
    {
        QuickDoubtMessage = string.Empty;
        QuickDoubtSentMessage = string.Empty;
        QuickDoubtTimestamp = null;
        IsQuickDoubtBubbleOpen = false;
    }

    private void DismissQuickDoubt(string? doubtId)
    {
        if (string.IsNullOrEmpty(doubtId))
        {
            return;
        }

        QuickDoubtItem? doubtToRemove = ActiveQuickDoubts.FirstOrDefault(d => d.Id == doubtId);
        if (doubtToRemove != null)
        {
            ActiveQuickDoubts.Remove(doubtToRemove);
        }
    }

    #endregion

    #region Service Initialization

    /// <summary>
    /// Initializes RPC and cloud messaging services.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We need to log all startup errors")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Logging for debug")]
    private async Task InitializeServicesAsync()
    {
        Console.WriteLine("[MeetingSession] InitializeServicesAsync: Started");
        System.Diagnostics.Debug.WriteLine("[MeetingSession] InitializeServicesAsync: Started");
        try
        {
            string meetingId = _currentMeeting?.MeetingId ?? "default-meeting";
            string username = _currentUser.DisplayName ?? "Unknown User";

            Console.WriteLine($"[MeetingSession] Connecting to cloud with MeetingId={meetingId}, Username={username}");
            System.Diagnostics.Debug.WriteLine($"[MeetingSession] Connecting to cloud with MeetingId={meetingId}, Username={username}");

            // Connect to cloud messaging service
            await _cloudMessageService.ConnectAsync(meetingId, username).ConfigureAwait(true);

            if (_cloudMessageService.IsConnected)
            {
                Console.WriteLine("[MeetingSession] CloudMessageService connected successfully");
                System.Diagnostics.Debug.WriteLine("[MeetingSession] CloudMessageService connected successfully");
                _toastService.ShowSuccess("Connected to cloud messaging service");
            }
            else
            {
                Console.WriteLine("[MeetingSession] CloudMessageService.ConnectAsync returned but IsConnected is FALSE");
                System.Diagnostics.Debug.WriteLine("[MeetingSession] CloudMessageService.ConnectAsync returned but IsConnected is FALSE");
                _toastService.ShowError("Cloud service failed to connect (IsConnected=false)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MeetingSession] InitializeServicesAsync FAILED: {ex}");
            System.Diagnostics.Debug.WriteLine($"[MeetingSession] InitializeServicesAsync FAILED: {ex}");
            _toastService.ShowError($"Failed to initialize services: {ex.Message}");
        }
    }

    /// <summary>
    /// Leaves the meeting and disconnects from all services.
    /// </summary>
    private async Task LeaveMeetingAsync()
    {
        try
        {
            // Stop all RPC streaming features before leaving
            if (_rpc != null)
            {
                // Stop video capture if enabled
                if (IsCameraOn)
                {
                    await _rpc.Call(Utils.STOP_VIDEO_CAPTURE, Array.Empty<byte>()).ConfigureAwait(true);
                }

                // Stop screen share if enabled
                if (IsScreenSharing)
                {
                    await _rpc.Call(Utils.STOP_SCREEN_CAPTURE, Array.Empty<byte>()).ConfigureAwait(true);
                }

                // Stop audio capture if enabled
                if (!IsMuted)
                {
                    await _rpc.Call(Utils.STOP_AUDIO_CAPTURE, Array.Empty<byte>()).ConfigureAwait(true);
                }

                // Notify backend of leaving
                await _rpc.Call("core/leaveMeeting", Array.Empty<byte>()).ConfigureAwait(true);
            }

            await CleanupAndNavigateBackAsync().ConfigureAwait(true);
            _toastService.ShowSuccess("Left the meeting");
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
        {
            _toastService.ShowError($"Error leaving meeting: {ex.Message}");
            // Even if error occurs, try to cleanup locally
            await CleanupAndNavigateBackAsync().ConfigureAwait(true);
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    private async Task CleanupAndNavigateBackAsync()
    {
        try
        {
            // Disconnect from cloud messaging service
            await _cloudMessageService.DisconnectAsync().ConfigureAwait(true);

            // Clear meeting state
            _currentMeeting = null;
            IsMeetingActive = false;

            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                Participants.Clear();
                // Navigate back to home
                if (_navigationService.CanGoBack)
                {
                    _navigationService.GoBack();
                }
                else
                {
                    // Fallback if no history (shouldn't happen if flow is correct)
                    // We might need to navigate explicitly to Home, but GoBack is safer for now
                }
            });
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is TimeoutException)
        {
            System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Unsubscribe from RPC events
            if (_rpcEventService != null)
            {
                _rpcEventService.ParticipantsListUpdated -= OnParticipantsListUpdated;
                _rpcEventService.Logout -= OnLogout;
                _rpcEventService.EndMeeting -= OnEndMeeting;
            }

            // Unsubscribe from cloud message events
            _cloudMessageService.MessageReceived -= OnCloudMessageReceived;

            // Disconnect from cloud messaging service
            _ = _cloudMessageService.DisconnectAsync();

            // Dispose managed resources
            VideoSession.Dispose();
            _toolbarViewModel.SelectedTabChanged -= OnSelectedTabChanged;
        }
    }

    #endregion
}


