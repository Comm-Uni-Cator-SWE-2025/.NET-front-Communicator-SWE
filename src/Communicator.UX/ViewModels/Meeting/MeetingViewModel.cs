// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using System.Collections.ObjectModel; // For ObservableCollection
using System.Windows.Input;
using Communicator.Controller.Meeting;
using Communicator.Core.UX;
using Communicator.ScreenShare;
using Communicator.UX;
using Communicator.UX.Services;
using Communicator.UX.ViewModels.Meeting;

namespace Communicator.UX.ViewModels.Meeting;
/// <summary>
/// This is the C# equivalent of your MeetingViewModel.java.
/// It inherits from your ObservableObject (which has an empty constructor).
/// It implements INavigationScope (which is for back/forward).
/// </summary>
public class MeetingViewModel : ObservableObject, INavigationScope
{
    // --- Private Fields ---
    private readonly AbstractRPC _rpc;
    private readonly UserProfile? _currentUser;
    private MeetingSession? _currentMeeting;

    // --- Bindable Properties (from your Java VM) ---
    // C# equivalent of 'BindableProperty<List<User>> participants'
    public ObservableCollection<UserProfile> Participants { get; }

    // C# equivalent of 'BindableProperty<Boolean> isVideoEnabled'
    private bool _isVideoEnabled;
    public bool IsVideoEnabled
    {
        get => _isVideoEnabled;
        // SetProperty is from your ObservableObject.cs
        set => SetProperty(ref _isVideoEnabled, value);
    }

    private bool _isAudioEnabled;
    public bool IsAudioEnabled
    {
        get => _isAudioEnabled;
        set => SetProperty(ref _isAudioEnabled, value);
    }

    private bool _isScreenShareEnabled;
    public bool IsScreenShareEnabled
    {
        get => _isScreenShareEnabled;
        set => SetProperty(ref _isScreenShareEnabled, value);
    }

    private bool _isMeetingActive;
    public bool IsMeetingActive
    {
        get => _isMeetingActive;
        set => SetProperty(ref _isMeetingActive, value);
    }
    public ICommand ToggleVideoCommand { get; }
    public ICommand ToggleAudioCommand { get; }
    public ICommand ToggleScreenShareCommand { get; }
    public ICommand EndMeetingCommand { get; }

    // --- Sub-ViewModels (Required by .NET Repo) ---
    public MeetingToolbarViewModel Toolbar { get; }
    public ChatViewModel Chat { get; }
    public ScreenShareViewModel ScreenShare { get; }
    public WhiteboardViewModel Whiteboard { get; }
    public VideoSessionViewModel VideoSession { get; }

    // --- Constructor ---
    public MeetingViewModel(
        IAuthenticationService authService, // We need this to get CurrentUser
        AbstractRPC rpc,
        MeetingToolbarViewModel toolbar,
        ChatViewModel chat,
        ScreenShareViewModel screenShare,
        WhiteboardViewModel whiteboard,
        VideoSessionViewModel videoSession)
    // NO base() call is needed, constructor is empty
    {
        _rpc = rpc;
        _currentUser = authService.CurrentUser; // Get the logged-in user

        // Store Sub-ViewModels
        Toolbar = toolbar;
        Chat = chat;
        ScreenShare = screenShare;
        Whiteboard = whiteboard;
        VideoSession = videoSession;

        // Initialize Properties
        Participants = new ObservableCollection<UserProfile>();

        // Initialize Commands (using RelayCommand from UX.Core)
        ToggleVideoCommand = new RelayCommand(_ => ToggleVideo());
        ToggleAudioCommand = new RelayCommand(_ => ToggleAudio());
        ToggleScreenShareCommand = new RelayCommand(_ => ToggleScreenSharing());
        EndMeetingCommand = new RelayCommand(_ => EndMeeting());

        // Start meeting right away (from your Java logic)
        StartMeeting();

        // Subscribe to RPC for new participants
        _rpc.Subscribe(Utils.SUBSCRIBE_AS_VIEWER, (args) => {
            string viewerIP = System.Text.Encoding.UTF8.GetString(args);
            var newUser = new UserProfile(
                email: $"{viewerIP}@example.com",
                displayName: "New User",
                role: ParticipantRole.Student,
                logoUrl: null
            );

            // Update collection on UI thread
            App.Current.Dispatcher.Invoke(() => AddParticipant(newUser));
            return Array.Empty<byte>();
        });
    }

    // --- Logic translated from your Java MeetingViewModel.java ---

    public void StartMeeting()
    {
        if (_currentUser != null)
        {
            // Create a new meeting session (placeholder logic)
            _currentMeeting = new MeetingSession(_currentUser.Email ?? "unknown", SessionMode.Class);
            _currentMeeting.AddParticipant(_currentUser);
        }

        IsMeetingActive = true;
        UpdateParticipants();
        //AddSystemMessage($"Meeting started with ID: {newMeetingId}");
    }

    private void ToggleVideo()
    {
        if (_currentMeeting == null)
        {
            return;
        }

        IsVideoEnabled = !IsVideoEnabled;
        _rpc.CallAsync(IsVideoEnabled ? Utils.START_VIDEO_CAPTURE : Utils.STOP_VIDEO_CAPTURE, Array.Empty<byte>());
        // TODO: Update via RPC when meeting service is ready
        //AddSystemMessage($"Video {(IsVideoEnabled ? "enabled" : "disabled")}");
    }

    private void ToggleAudio()
    {
        if (_currentMeeting == null)
        {
            return;
        }

        IsAudioEnabled = !IsAudioEnabled;
        // Make sure these are in your C# Utils.cs
        _rpc.CallAsync(IsAudioEnabled ? "START_AUDIO_CAPTURE" : "STOP_AUDIO_CAPTURE", Array.Empty<byte>());
        // TODO: Update via RPC when meeting service is ready
        //AddSystemMessage($"Audio {(IsAudioEnabled ? "enabled" : "disabled")}");
    }

    private void ToggleScreenSharing()
    {
        if (_currentMeeting == null)
        {
            return;
        }

        IsScreenShareEnabled = !IsScreenShareEnabled;
        _rpc.CallAsync(IsScreenShareEnabled ? Utils.START_SCREEN_CAPTURE : Utils.STOP_SCREEN_CAPTURE, Array.Empty<byte>());
        // TODO: Update via RPC when meeting service is ready
        //AddSystemMessage($"Screen sharing {(IsScreenShareEnabled ? "enabled" : "disabled")}");
    }

    private void EndMeeting()
    {
        if (_currentMeeting == null)
        {
            return;
        }

        // TODO: Implement proper meeting end logic via RPC
        _currentMeeting = null;
        IsMeetingActive = false;
        //AddSystemMessage("Meeting ended.");
    }

    private void AddParticipant(UserProfile user)
    {
        if (_currentMeeting != null && !Participants.Contains(user))
        {
            Participants.Add(user); // ObservableCollection updates UI
            //AddSystemMessage($"{user.DisplayName} joined the meeting.");
        }
    }

    private void UpdateParticipants()
    {
        if (_currentMeeting != null)
        {
            Participants.Clear();
            foreach (UserProfile participant in _currentMeeting.Participants.Values)
            {
                Participants.Add(participant);
            }
        }
    }

    //private void AddSystemMessage(string message)
    //{
    //    Chat.AddSystemMessage(message); // Send to existing Chat VM
    //}

    // --- INavigationScope Implementation (as you provided) ---
    // Your Java logic has no back/forward, so we return false.

    public bool CanNavigateBack => false;
    public bool CanNavigateForward => false;
    public void NavigateBack() { }
    public void NavigateForward() { }
    public event EventHandler? NavigationStateChanged;
}
