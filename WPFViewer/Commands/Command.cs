using System;
using System.Diagnostics;
using System.Windows.Input;

namespace WPFViewer.Commands
{
    /// <summary>
    /// Команда единственной целью которой является
    /// передача функциональности другим объектам
    /// вызовом делегатов.
    /// По умолчанию значение свойства CanExecute
    /// равно true
    /// </summary>
    public class Command : ICommand
    {
        #region Поля

        private readonly Func<bool> _canExecute;
        private readonly Action _execute;

        #endregion Поля

        #region Конструкторы

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public Command(Action execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public Command(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion Конструкторы

        #region Члены ICommand

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        #endregion Члены ICommand
    }

    /// <summary>
    /// Команда единственной целью которой является
    /// передача функциональности другим объектам
    /// вызовом делегатов.
    /// По умолчанию значение свойства CanExecute
    /// равно true
    /// </summary>
    public class Command<T> : ICommand
    {
        #region Поля

        private readonly Action<T> _execute = null;
        private readonly Predicate<T> _canExecute = null;

        #endregion Поля

        #region Конструкторы

        public Command(Action<T> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public Command(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion Конструкторы

        #region Члены ICommand

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        #endregion Члены ICommand
    }
}