namespace Solution.Commands;

public class RelayCommand : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    private Action<object?> _execute;
    private Predicate<object?>? _canExecute;

    private bool _canExecuteRetry;
    private DispatcherTimer _clickTimer;


    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute);

        _execute = execute;
        _canExecute = canExecute;
        _canExecuteRetry = true;

        _clickTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(50) };
        _clickTimer.Tick += (_, _) => { _canExecuteRetry = true; _clickTimer.Stop(); };
    }


    public bool CanExecute(object? parameter)
    {
        if (!_canExecuteRetry) return false;

        return _canExecute is null || _canExecute.Invoke(parameter);
    }

    public void Execute(object? parameter)
    {
        _canExecuteRetry = false;
        _clickTimer.Start();

        _execute.Invoke(parameter);
    }
}
