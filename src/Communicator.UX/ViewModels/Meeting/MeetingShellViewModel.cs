using System;
using System.Collections.Generic;
using System.Windows.Input;
using Communicator.Core.UX;
using Communicator.Core.UX.Services;
using Controller;

namespace Communicator.UX.ViewModels.Meeting;

/// <summary>
/// Coordinates meeting sub-tabs, maintains an internal navigation stack, and exposes toolbar state to the shell.
/// </summary>
public class MeetingShellViewModel : ObservableObject, INavigationScope, IDisposable
{
    private readonly MeetingToolbarViewModel _toolbarViewModel;
    private readonly IToastService _toastService;
    private readonly User _user;
    private readonly Stack<MeetingTabViewModel> _backStack = new();
    private readonly Stack<MeetingTabViewModel> _forwardStack = new();
    private MeetingTabViewModel? _currentTab;
    private bool _suppressSelectionNotifications;
    private object? _currentPage;
    private bool _isMuted;
    private bool _isCameraOn = true;
    private bool _isHandRaised;
    private bool _isScreenSharing;

    // Side Panel Support
    private object? _sidePanelContent;
    private bool _isSidePanelOpen;

    // Quick Doubt Feature
    private string _quickDoubtMessage = string.Empty;
    private bool _isQuickDoubtBubbleOpen;
    private DateTime? _quickDoubtTimestamp;
    private string _quickDoubtSentMessage = string.Empty;

    /// <summary>
    /// Builds meeting tabs for the supplied user and initializes navigation state.
    /// Services are now injected via constructor for better testability.
    /// </summary>
    public MeetingShellViewModel(User user, IToastService toastService)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));

        _toolbarViewModel = new MeetingToolbarViewModel(CreateTabs(user));
        _toolbarViewModel.SelectedTabChanged += OnSelectedTabChanged;
        _currentTab = _toolbarViewModel.SelectedTab;
        if (_currentTab != null)
        {
            CurrentPage = _currentTab.ContentViewModel;
        }
        ToggleMuteCommand = new RelayCommand(_ => ToggleMute());
        ToggleCameraCommand = new RelayCommand(_ => ToggleCamera());
        ToggleHandCommand = new RelayCommand(_ => ToggleHandRaised());
        ToggleScreenShareCommand = new RelayCommand(_ => ToggleScreenShare());
        LeaveMeetingCommand = new RelayCommand(_ => LeaveMeeting());
        ToggleChatPanelCommand = new RelayCommand(_ => ToggleChatPanel());
        CloseSidePanelCommand = new RelayCommand(_ => CloseSidePanel());
        SendQuickDoubtCommand = new RelayCommand(_ => SendQuickDoubt(), _ => CanSendQuickDoubt());
        RaiseNavigationStateChanged();
    }

    public MeetingToolbarViewModel Toolbar => _toolbarViewModel;

    public object? CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    /// <summary>
    /// Tracks toolbar selection changes and updates history stacks appropriately.
    /// </summary>
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

    /// <summary>
    /// Creates the default set of meeting tabs for the logged-in user.
    /// </summary>
    private static IEnumerable<MeetingTabViewModel> CreateTabs(User user)
    {
        yield return new MeetingTabViewModel("AI Insights", new AIInsightsViewModel(user));
        yield return new MeetingTabViewModel("Video", new VideoSessionViewModel(user));
        yield return new MeetingTabViewModel("ScreenShare", new ScreenShareViewModel(user));
        yield return new MeetingTabViewModel("Canvas", new WhiteboardViewModel(user));
    }

    public bool CanNavigateBack => _backStack.Count > 0;

    public bool CanNavigateForward => _forwardStack.Count > 0;

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

    public ICommand ToggleMuteCommand { get; }
    public ICommand ToggleCameraCommand { get; }
    public ICommand ToggleHandCommand { get; }
    public ICommand ToggleScreenShareCommand { get; }
    public ICommand LeaveMeetingCommand { get; }
    public ICommand ToggleChatPanelCommand { get; }
    public ICommand CloseSidePanelCommand { get; }
    public ICommand SendQuickDoubtCommand { get; }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    public event EventHandler? NavigationStateChanged;

    private void RaiseNavigationStateChanged()
    {
        NavigationStateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Activates the supplied tab and updates the current page binding.
    /// </summary>
    private void ActivateTab(MeetingTabViewModel tab)
    {
        _currentTab = tab;
        CurrentPage = tab.ContentViewModel;
        RaiseNavigationStateChanged();
    }

    private void ToggleMute()
    {
        IsMuted = !IsMuted;
    }

    private void ToggleCamera()
    {
        IsCameraOn = !IsCameraOn;
    }

    private void ToggleHandRaised()
    {
        IsHandRaised = !IsHandRaised;
        
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
        IsScreenSharing = !IsScreenSharing;
    }

    private bool CanSendQuickDoubt()
    {
        return !string.IsNullOrWhiteSpace(QuickDoubtMessage);
    }

    private void SendQuickDoubt()
    {
        if (string.IsNullOrWhiteSpace(QuickDoubtMessage))
        {
            return;
        }

        // Capture the sent message and timestamp
        QuickDoubtSentMessage = QuickDoubtMessage.Trim();
        QuickDoubtTimestamp = DateTime.Now;

        // Simulate broadcasting to all participants
        
        // In real implementation, this would call:
        // await _meetingService.BroadcastQuickDoubtAsync(QuickDoubtSentMessage);

        // Clear the input field for next message
        QuickDoubtMessage = string.Empty;

        // Keep the bubble open to show the sent message
        // User must click "Lower Hand" to dismiss
    }

    private void ClearQuickDoubt()
    {
        QuickDoubtMessage = string.Empty;
        QuickDoubtSentMessage = string.Empty;
        QuickDoubtTimestamp = null;
        IsQuickDoubtBubbleOpen = false;
    }

    private void LeaveMeeting()
    {
        _toastService.ShowWarning("Leave meeting flow is not implemented yet.");
    }

    /// <summary>
    /// Toggles the chat side panel open/closed.
    /// </summary>
    private void ToggleChatPanel()
    {
        if (IsSidePanelOpen)
        {
            // If panel is open, close it
            CloseSidePanel();
        }
        else
        {
            // Open chat panel with the current user
            var chatViewModel = new ChatViewModel(_user, _toastService);

            SidePanelContent = chatViewModel;
            IsSidePanelOpen = true;
        }
    }

    /// <summary>
    /// Closes the side panel.
    /// </summary>
    private void CloseSidePanel()
    {
        IsSidePanelOpen = false;
        SidePanelContent = null;
    }

    /// <summary>
    /// Restores a tab from history without re-adding it to navigation stacks.
    /// </summary>
    private void ActivateTabFromHistory(MeetingTabViewModel tab)
    {
        _suppressSelectionNotifications = true;
        _toolbarViewModel.SelectedTab = tab;
        _suppressSelectionNotifications = false;
        ActivateTab(tab);
    }

    /// <summary>
    /// Cleans up event subscriptions and history stacks.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Dispose managed resources
            _toolbarViewModel.SelectedTabChanged -= OnSelectedTabChanged;
            _backStack.Clear();
            _forwardStack.Clear();
        }
    }
}

