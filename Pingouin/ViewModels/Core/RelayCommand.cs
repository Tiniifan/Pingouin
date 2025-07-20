using System;
using System.Windows.Input;

namespace Pingouin.ViewModels
{
    /// <summary>
    /// A generic command whose logic is relayed to delegates.
    /// This is a common implementation of the ICommand interface used in MVVM applications
    /// to bind UI actions (like button clicks) to methods in the ViewModel.
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// The delegate to execute when the command is invoked.
        /// </summary>
        private readonly Action<object> _execute;

        /// <summary>
        /// The delegate that determines whether the command can be executed.
        /// </summary>
        private readonly Func<object, bool> _canExecute;

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// This implementation hooks into the CommandManager's RequerySuggested event,
        /// which allows WPF to automatically handle the enabling/disabling of UI controls
        /// based on focus changes and other UI events.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic. If null, the command is always considered executable.</param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Overload for creating a command with an action that does not take a parameter.
        /// </summary>
        /// <param name="execute">The parameterless execution logic.</param>
        /// <param name="canExecute">The parameterless execution status logic.</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null): this(p => execute(), p => canExecute == null || canExecute())
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Manually raises the CanExecuteChanged event to force the UI to re-evaluate the command's status.
        /// This is useful when a condition affecting the command changes from within the ViewModel.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}