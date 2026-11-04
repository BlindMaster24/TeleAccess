using TeleAccess.Core;

namespace TeleAccess.App.Input;

public sealed class CommandInvokedEventArgs : EventArgs
{
    public CommandInvokedEventArgs(TeleCommand command)
    {
        Command = command;
    }

    public TeleCommand Command { get; }
}
