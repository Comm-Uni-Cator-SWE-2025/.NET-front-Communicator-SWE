using System;

namespace Communicator.Core.UX;

/// <summary>
/// Abstraction for view models that manage their own navigation history (e.g., meeting tabs) so the shell can delegate back/forward behavior.
/// </summary>
public interface INavigationScope
{
    /// <summary>
    /// Indicates whether the scope can move backward within its local navigation stack.
    /// </summary>
    bool CanNavigateBack { get; }

    /// <summary>
    /// Indicates whether the scope can move forward within its local navigation stack.
    /// </summary>
    bool CanNavigateForward { get; }

    /// <summary>
    /// Requests a backward navigation operation within the scope.
    /// </summary>
    void NavigateBack();

    /// <summary>
    /// Requests a forward navigation operation within the scope.
    /// </summary>
    void NavigateForward();

    /// <summary>
    /// Raised when the scope's navigation capabilities change so the shell can refresh command states.
    /// </summary>
    event EventHandler? NavigationStateChanged;
}
