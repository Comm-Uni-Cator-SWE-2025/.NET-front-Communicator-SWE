/*
 * -----------------------------------------------------------------------------
 *  File: INavigationService.cs
 *  Owner: Pramodh Sai
 *  Roll Number : 112201029
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;

namespace Communicator.UX.Core.Services;

/// <summary>
/// Provides a shell-wide navigation stack for view models, allowing forward/back traversal and change notifications.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Raised whenever the current view or navigation history changes so listeners can react.
    /// </summary>
    event EventHandler? NavigationChanged;

    /// <summary>
    /// The view model currently displayed in the shell.
    /// </summary>
    object? CurrentView { get; }

    /// <summary>
    /// Indicates whether the service can navigate backward in its stack.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Pushes a new view model onto the navigation stack and activates it immediately.
    /// </summary>
    void NavigateTo(object viewModel);

    /// <summary>
    /// Moves to the previous entry in the navigation stack when available.
    /// </summary>
    void GoBack();

    /// <summary>
    /// Clears the navigation history and resets the forward/back stacks.
    /// </summary>
    void ClearHistory();
}

