/*
 * -----------------------------------------------------------------------------
 *  File: NavigationService.cs
 *  Owner: Pramodh Sai
 *  Roll Number : 112201029
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
    private object? _currentView;

    public event EventHandler? NavigationChanged;

    /// <inheritdoc />
    public bool CanGoBack => _backStack.Count > 0;

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

        CurrentView = viewModel;
    }

    /// <inheritdoc />
    public void GoBack()
    {
        if (!CanGoBack)
        {
            return;
        }

        CurrentView = _backStack.Pop();
    }

    /// <inheritdoc />
    public void ClearHistory()
    {
        DisposeStack(_backStack);
        _backStack.Clear();
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



