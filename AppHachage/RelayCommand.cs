using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppHachage
{
    public class RelayCommand : ICommand
    {
        private readonly Func<bool>? _canExecute;
        private readonly Func<Task>? _executeAsync;
        private readonly Action? _executeSync;

        public RelayCommand(Action executeSync, Func<bool>? canExecute = null)
        {
            _executeSync = executeSync ?? throw new ArgumentNullException(nameof(executeSync));
            _canExecute = canExecute;
        }

        public RelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public async void Execute(object? parameter)
        {
            if (_executeAsync is not null)
            {
                await _executeAsync();
                return;
            }

            _executeSync?.Invoke();
        }

        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}
