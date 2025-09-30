using System;
using System.Collections.Generic;

namespace GUI.Services
{
    /// <summary>
    /// Stack-based navigation service that tracks back and forward history for shell-wide view model transitions.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly Stack<object> _backStack = new();
        private readonly Stack<object> _forwardStack = new();
        private object? _currentView;

        public event EventHandler? NavigationChanged;

        /// <inheritdoc />
        public bool CanGoBack => _backStack.Count > 0;

        /// <inheritdoc />
        public bool CanGoForward => _forwardStack.Count > 0;

        /// <inheritdoc />
        public object? CurrentView
        {
            get => _currentView;
            private set
            {
                _currentView = value;
                NavigationChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc />
        public void NavigateTo(object viewModel)
        {
            if (_currentView != null)
            {
                _backStack.Push(_currentView);
            }

            _forwardStack.Clear();
            CurrentView = viewModel;
        }

        /// <inheritdoc />
        public void GoBack()
        {
            if (!CanGoBack) return;

            if (_currentView != null)
            {
                _forwardStack.Push(_currentView);
            }

            CurrentView = _backStack.Pop();
        }

        /// <inheritdoc />
        public void GoForward()
        {
            if (!CanGoForward) return;

            if (_currentView != null)
            {
                _backStack.Push(_currentView);
            }

            CurrentView = _forwardStack.Pop();
        }

        /// <inheritdoc />
        public void ClearHistory()
        {
            _backStack.Clear();
            _forwardStack.Clear();
        }
    }
}
