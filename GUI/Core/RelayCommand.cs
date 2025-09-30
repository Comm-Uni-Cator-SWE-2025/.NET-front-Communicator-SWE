using System;
using System.Windows.Input;

namespace GUI.Core;

/// <summary>
/// ICommand implementation for delegating Execute and CanExecute logic.
/// </summary>
public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    /// <summary>
    /// Creates a command that delegates execution to the provided delegates.
    /// </summary>
    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Evaluates the provided <c>canExecute</c> predicate, defaulting to true when none was supplied.
    /// </summary>
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    /// <summary>
    /// Runs the <c>execute</c> delegate with the supplied parameter.
    /// </summary>
    public void Execute(object? parameter) => _execute(parameter);

    /// <summary>
    /// Notifies command sources (e.g., buttons) to re-query <see cref="CanExecute(object?)"/>.
    /// </summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
