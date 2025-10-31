using System;
using System.Collections.Generic;

namespace GUI.Services
{
    public class NavigationService : INavigationService
    {
        private readonly Stack<object> _backStack = new();
        private readonly Stack<object> _forwardStack = new();
        private object? _currentView;

        public event EventHandler? NavigationChanged;

        public bool CanGoBack => _backStack.Count > 0;
        public bool CanGoForward => _forwardStack.Count > 0;

        public object? CurrentView
        {
            get => _currentView;
            private set
            {
                _currentView = value;
                NavigationChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void NavigateTo(object viewModel)
        {
            if (_currentView != null)
            {
                _backStack.Push(_currentView);
            }

            _forwardStack.Clear();
            CurrentView = viewModel;
        }

        public void GoBack()
        {
            if (!CanGoBack) return;

            if (_currentView != null)
            {
                _forwardStack.Push(_currentView);
            }

            CurrentView = _backStack.Pop();
        }

        public void GoForward()
        {
            if (!CanGoForward) return;

            if (_currentView != null)
            {
                _backStack.Push(_currentView);
            }

            CurrentView = _forwardStack.Pop();
        }

        public void ClearHistory()
        {
            _backStack.Clear();
            _forwardStack.Clear();
        }
    }
}
