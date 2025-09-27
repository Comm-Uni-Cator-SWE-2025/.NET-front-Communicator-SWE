using System;
using System.Collections.Generic;
using GUI.Core;
using Controller;

namespace GUI.ViewModels.Meeting
{
    public class MeetingShellViewModel : ObservableObject, INavigationScope, IDisposable
    {
        private readonly MeetingToolbarViewModel _toolbarViewModel;
        private readonly Stack<MeetingTabViewModel> _backStack = new();
        private readonly Stack<MeetingTabViewModel> _forwardStack = new();
        private MeetingTabViewModel? _currentTab;
        private bool _suppressSelectionNotifications;
        private object? _currentPage;

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
            RaiseNavigationStateChanged();
        }

        public MeetingToolbarViewModel Toolbar => _toolbarViewModel;

        public object? CurrentPage
        {
            get => _currentPage;
            private set => SetProperty(ref _currentPage, value);
        }

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

        private static IEnumerable<MeetingTabViewModel> CreateTabs(UserProfile user)
        {
            yield return new MeetingTabViewModel("Dashboard", new MeetingDashboardViewModel(user));
            yield return new MeetingTabViewModel("Meeting", new MeetingSessionViewModel(user));
            yield return new MeetingTabViewModel("ScreenShare", new ScreenShareViewModel(user));
            yield return new MeetingTabViewModel("Whiteboard", new WhiteboardViewModel(user));
            yield return new MeetingTabViewModel("Chat", new MeetingChatViewModel(user));
        }

        public bool CanNavigateBack => _backStack.Count > 0;

        public bool CanNavigateForward => _forwardStack.Count > 0;

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

        public void Dispose()
        {
            _toolbarViewModel.SelectedTabChanged -= OnSelectedTabChanged;
            _backStack.Clear();
            _forwardStack.Clear();
        }
    }
}
