using System.Windows.Input;
using System;

namespace MTGPrint.Helper.UI
{
    public class DelegateCommand : ICommand
    {
        private readonly Predicate<object> canExecute;
        private readonly Action<object> execute;

        public DelegateCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            this.canExecute = canExecute ?? delegate { return true; };
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) { return canExecute(parameter); }

        public void Execute(object parameter) { execute(parameter); }
    }

    public class LightCommand : ICommand
    {
        private readonly Predicate<object> canExecute;
        private readonly Action execute;

        public LightCommand(Action execute, Predicate<object> canExecute = null)
        {
            this.canExecute = canExecute ?? delegate { return true; };
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) { return canExecute(parameter); }

        public void Execute(object parameter) { execute(); }
    }
    public class EventCommand<T> : ICommand where T:EventArgs
    {
        private readonly Predicate<object> canExecute;
        private readonly Action<T> execute;

        public EventCommand(Action<T> execute, Predicate<object> canExecute = null)
        {
            this.canExecute = canExecute ?? delegate { return true; };
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) { return canExecute(parameter); }

        public void Execute(object parameter) { execute(parameter as T); }
    }
}
