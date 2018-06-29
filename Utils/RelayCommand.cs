using System;
using System.Windows.Input;

namespace AudibleBookmarks.Utils
{
    public class RelayCommand : ICommand
    {
        private Func<bool> _canExecute;
        private Action _execute;

        public RelayCommand(Func<bool> canExecute, Action execute)
        {
            this._canExecute = canExecute;
            this._execute = execute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}
