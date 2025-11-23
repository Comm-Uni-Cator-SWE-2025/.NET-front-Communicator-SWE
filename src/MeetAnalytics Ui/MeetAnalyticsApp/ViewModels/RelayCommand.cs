using System;
using System.Windows.Input;

namespace MeetAnalyticsApp.Commands
{
    /// <summary>
    /// A simple command implementation used for binding UI actions to ViewModel methods.
    /// Always executable. Extend with CanExecute logic if needed.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;

        /// <summary>
        /// Creates a new command that invokes the specified action.
        /// </summary>
        /// <param name="execute">The action to run when the command is executed.</param>
        public RelayCommand(Action<object?> execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Defines whether the command can execute.
        /// This implementation always returns true.
        /// </summary>
        public bool CanExecute(object? parameter) => true;

        /// <summary>
        /// Runs the provided action.
        /// </summary>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}
