using System;

namespace GUI.Services
{
    public interface INavigationService
    {
        event EventHandler? NavigationChanged;
        
        object? CurrentView { get; }
        bool CanGoBack { get; }
        bool CanGoForward { get; }
        
        void NavigateTo(object viewModel);
        void GoBack();
        void GoForward();
        void ClearHistory();
    }
}
