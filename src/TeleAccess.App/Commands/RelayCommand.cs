using System.Windows.Input;

namespace TeleAccess.App.Commands;

public sealed class RelayCommand : ICommand
{
    private readonly Action _execute;

    public RelayCommand(Action execute)
    {
        ArgumentNullException.ThrowIfNull(execute);
        _execute = execute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => _ = value;
        remove => _ = value;
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => _execute();
}
