using System;
using System.Collections.Generic;
using System.Windows.Input;
using UX.Core;
using Controller;

namespace GUI.ViewModels.Meeting
{
    /// <summary>
    /// Coordinates meeting sub-tabs, maintains an internal navigation stack, and exposes toolbar state to the shell.
    /// </summary>
    public class MeetingShellViewModel : ObservableObject, INavigationScope, IDisposable
    {
        private readonly MeetingToolbarViewModel _toolbarViewModel;
        private readonly Stack<MeetingTabViewModel> _backStack = new();
        private readonly Stack<MeetingTabViewModel> _forwardStack = new();
        private MeetingTabViewModel? _currentTab;
        private bool _suppressSelectionNotifications;
        private object? _currentPage;
        private bool _isMuted;
        private bool _isCameraOn = true;
        private bool _isHandRaised;
        private bool _isScreenSharing;

        /// <summary>
        /// Builds meeting tabs for the supplied user and initializes navigation state.
        /// </summary>
        public MeetingShellViewModel(UserProfile user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

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
        private void OnSelectedTabChanged(object? sender, MeetingTabViewModel? tab)
        {
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
        private static IEnumerable<MeetingTabViewModel> CreateTabs(UserProfile user)
        {
            yield return new MeetingTabViewModel("Dashboard", new MeetingDashboardViewModel(user));
            yield return new MeetingTabViewModel("Video", new VideoSessionViewModel(user));
            yield return new MeetingTabViewModel("ScreenShare", new ScreenShareViewModel(user));
            yield return new MeetingTabViewModel("Whiteboard", new WhiteboardViewModel(user));
            yield return new MeetingTabViewModel("Chat", new MeetingChatViewModel(user));
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

        public ICommand ToggleMuteCommand { get; }
        public ICommand ToggleCameraCommand { get; }
        public ICommand ToggleHandCommand { get; }
        public ICommand ToggleScreenShareCommand { get; }
        public ICommand LeaveMeetingCommand { get; }

        /// <inheritdoc />
        public void NavigateBack()
        {
            if (!CanNavigateBack || _currentTab == null)
            {
                return;
            }

            var target = _backStack.Pop();
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

            var target = _forwardStack.Pop();
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
            App.ToastService.ShowInfo(IsMuted ? "Microphone muted." : "Microphone unmuted.");
        }

        private void ToggleCamera()
        {
            IsCameraOn = !IsCameraOn;
            App.ToastService.ShowInfo(IsCameraOn ? "Camera enabled." : "Camera disabled.");
        }

        private void ToggleHandRaised()
        {
            IsHandRaised = !IsHandRaised;
            App.ToastService.ShowInfo(IsHandRaised ? "Hand raised." : "Hand lowered.");
        }

        private void ToggleScreenShare()
        {
            IsScreenSharing = !IsScreenSharing;
            App.ToastService.ShowInfo(IsScreenSharing ? "Screen sharing on." : "Screen sharing off.");
        }

        private void LeaveMeeting()
        {
            App.ToastService.ShowWarning("Leave meeting flow is not implemented yet.");
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
            _toolbarViewModel.SelectedTabChanged -= OnSelectedTabChanged;
            _backStack.Clear();
            _forwardStack.Clear();
        }
    }
}

