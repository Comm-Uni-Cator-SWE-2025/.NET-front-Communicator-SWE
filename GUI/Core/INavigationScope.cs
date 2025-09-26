using System;

namespace GUI.Core;

public interface INavigationScope
{
    bool CanNavigateBack { get; }
    bool CanNavigateForward { get; }
    void NavigateBack();
    void NavigateForward();
    event EventHandler? NavigationStateChanged;
}
