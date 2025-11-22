/*
 * -----------------------------------------------------------------------------
 *  File: NavigationService.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.Collections.Generic;
using Communicator.Core.UX.Services;

namespace Communicator.App.Services;

/// <summary>
/// Stack-based navigation service that tracks back and forward history for shell-wide view model transitions.
/// </summary>
public sealed class NavigationService : Communicator.Core.UX.Services.INavigationService
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
        private set {
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

        // Clear forward stack and dispose ViewModels
        DisposeStack(_forwardStack);
        _forwardStack.Clear();

        CurrentView = viewModel;
    }

    /// <inheritdoc />
    public void GoBack()
    {
        if (!CanGoBack)
        {
            return;
        }

        if (_currentView != null)
        {
            _forwardStack.Push(_currentView);
        }

        CurrentView = _backStack.Pop();
    }

    /// <inheritdoc />
    public void GoForward()
    {
        if (!CanGoForward)
        {
            return;
        }

        if (_currentView != null)
        {
            _backStack.Push(_currentView);
        }

        CurrentView = _forwardStack.Pop();
    }

    /// <inheritdoc />
    public void ClearHistory()
    {
        DisposeStack(_backStack);
        DisposeStack(_forwardStack);
        _backStack.Clear();
        _forwardStack.Clear();
    }

    /// <summary>
    /// Disposes all IDisposable ViewModels in a stack without clearing it.
    /// </summary>
    private static void DisposeStack(Stack<object> stack)
    {
        foreach (object viewModel in stack)
        {
            if (viewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}



