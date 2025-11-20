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
public sealed class MeetingSessionViewModel : ObservableObject, INavigationScope, IDisposable
{
    private readonly MeetingToolbarViewModel _toolbarViewModel;
    private readonly IToastService _toastService;
    private readonly ICloudMessageService _cloudMessageService;
    private readonly ICloudConfigService _cloudConfig;
    private readonly INavigationService _navigationService;
    private readonly UserProfile _currentUser;
    private readonly Stack<MeetingTabViewModel> _backStack = new();
    private readonly Stack<MeetingTabViewModel> _forwardStack = new();
    private MeetingTabViewModel? _currentTab;
    private bool _suppressSelectionNotifications;
    private object? _currentPage;

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
        IRPC? rpc = null,
        IRpcEventService? rpcEventService = null)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _currentMeeting = meetingSession; // Initialize with passed session if available
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
        _cloudMessageService = cloudMessageService ?? throw new ArgumentNullException(nameof(cloudMessageService));
        _cloudConfig = cloudConfig ?? throw new ArgumentNullException(nameof(cloudConfig));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _rpc = rpc;
        _rpcEventService = rpcEventService;

        // Initialize participants collection
        Participants = new ObservableCollection<ParticipantViewModel>();

        // Create sub-ViewModels with shared participant collection
        VideoSession = new VideoSessionViewModel(_currentUser, Participants, _rpc, _rpcEventService);
        Chat = new ChatViewModel(_currentUser, _toastService, _rpc, _rpcEventService);
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

        RaiseNavigationStateChanged();

        // Start the meeting session
        StartMeeting();

        // Subscribe to RPC events via service (avoids late subscription error)
        if (_rpcEventService != null)
        {
            _rpcEventService.ParticipantJoined += OnParticipantJoined;
            _rpcEventService.ParticipantLeft += OnParticipantLeft;
            _rpcEventService.ParticipantsListUpdated += OnParticipantsListUpdated;
        }

        // Connect to HandWave and RPC
        _ = InitializeServicesAsync();
    }

    private void OnParticipantJoined(object? sender, RpcStringEventArgs e)
    {
        string viewerIP = e.Value;
        UserProfile newUser = new(
            email: $"{viewerIP}@example.com",
            displayName: viewerIP,
            role: ParticipantRole.STUDENT,
            logoUrl: null
        );

        // Update collection on UI thread
        System.Windows.Application.Current.Dispatcher.Invoke(() => AddParticipant(newUser));
    }

    private void OnParticipantLeft(object? sender, RpcStringEventArgs e)
    {
        string viewerIP = e.Value;
        // Update collection on UI thread
        System.Windows.Application.Current.Dispatcher.Invoke(() => {
            // Find participant by email (which we constructed as IP@example.com)
            string email = $"{viewerIP}@example.com";
            RemoveParticipant(email);
        });
    }

    private void OnParticipantsListUpdated(object? sender, RpcStringEventArgs e)
    {
        string participantsJson = e.Value;
        try
        {
            // Deserialize the list of participants
            // Expected format: {"ip": "email", ...} (Map<String, String> from Java)
            Dictionary<string, string>? ipToMailMap = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(participantsJson);

            if (ipToMailMap == null)
            {
                return;
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                // Sync our list with the backend list

                // 1. Add new participants
                foreach (KeyValuePair<string, string> kvp in ipToMailMap)
                {
                    string ip = kvp.Key;
                    string email = kvp.Value;

                    // Handle Java ClientNode.toString() format: ClientNode[hostName=127.0.0.1, port=6945]
                    if (ip.StartsWith("ClientNode[", StringComparison.Ordinal) && ip.Contains("hostName=", StringComparison.Ordinal))
                    {
                        int start = ip.IndexOf("hostName=", StringComparison.Ordinal) + 9;
                        int end = ip.IndexOf(',', start);
                        if (end == -1)
                        {
                            end = ip.IndexOf(']', start);
                        }

                        if (start > 8 && end > start)
                        {
                            ip = ip.Substring(start, end - start);
                        }
                    }

                    string displayName = !string.IsNullOrEmpty(ip) ? ip : email;

                    // Check if we already have this participant
                    if (!Participants.Any(existing => existing.User.Email == email))
                    {
                        UserProfile newUser = new(
                            email: email,
                            displayName: displayName,
                            role: ParticipantRole.STUDENT,
                            logoUrl: null
                        );
                        AddParticipant(newUser);
                    }
                }

                // 2. Remove participants not in the list (except self)
                var emailsInBackend = new HashSet<string>(ipToMailMap.Values);

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
    public ICommand DismissQuickDoubtCommand { get; }

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
            if (_currentMeeting == null)
            {
                // Fallback: Create a new meeting session locally if none provided
                _currentMeeting = new MeetingSession(_currentUser.Email ?? "unknown", SessionMode.CLASS);
                _currentMeeting.AddParticipant(_currentUser);
            }

            // Add current user to participants list if not already there
            AddParticipant(_currentUser);

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
        return !string.IsNullOrWhiteSpace(QuickDoubtMessage) && _cloudMessageService.IsConnected;
    }

    /// <summary>
    /// Sends quick doubt message via cloud messaging service.
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

            // Send via cloud message service
            await _cloudMessageService.SendMessageAsync(
                CloudMessageType.QuickDoubt,
                _currentUser.DisplayName ?? "Unknown User",
                QuickDoubtSentMessage).ConfigureAwait(true);

            // Clear the input field and hide it
            QuickDoubtMessage = string.Empty;

            // Note: We don't show the popup for the sender - only others will see it via SignalR
            // Keep the bubble open to show the sent message (no textbox)
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is System.Net.Http.HttpRequestException)
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
    /// Handler for receiving cloud messages via SignalR.
    /// Routes to appropriate handler based on message type.
    /// </summary>
    private void OnCloudMessageReceived(object? sender, CloudMessageEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[MeetingSession] Received {e.MessageType}: Sender='{e.SenderName}', Message='{e.Message}'");

        switch (e.MessageType)
        {
            case CloudMessageType.UserJoined:
                HandleUserJoined(e.SenderName);
                break;

            case CloudMessageType.QuickDoubt:
                HandleQuickDoubt(e.SenderName, e.Message);
                break;
        }
    }

    /// <summary>
    /// Handles user joined notifications by showing a toast.
    /// </summary>
    private void HandleUserJoined(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            System.Diagnostics.Debug.WriteLine("[MeetingSession] HandleUserJoined: Username is empty, ignoring");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[MeetingSession] HandleUserJoined: Showing toast for '{username}'");

        // Show simple toast notification - no popup
        _toastService.ShowInfo($"{username} joined!");
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
    private async Task InitializeServicesAsync()
    {
        try
        {
            // Connect to cloud messaging service
            await _cloudMessageService.ConnectAsync(_currentUser.DisplayName ?? "Unknown User").ConfigureAwait(true);
            _toastService.ShowSuccess("Connected to cloud messaging service");

            // Notify other participants that this user has joined
            await _cloudMessageService.SendMessageAsync(
                CloudMessageType.UserJoined,
                _currentUser.DisplayName ?? "Unknown User").ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is System.Net.Http.HttpRequestException)
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
                _rpcEventService.ParticipantJoined -= OnParticipantJoined;
            }

            // Unsubscribe from cloud message events
            _cloudMessageService.MessageReceived -= OnCloudMessageReceived;

            // Disconnect from cloud messaging service
            _ = _cloudMessageService.DisconnectAsync();

            // Dispose managed resources
            VideoSession.Dispose();
            _toolbarViewModel.SelectedTabChanged -= OnSelectedTabChanged;
            _backStack.Clear();
            _forwardStack.Clear();
        }
    }

    #endregion
}


