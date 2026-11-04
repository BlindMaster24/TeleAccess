using System.Windows.Controls;
using TeleAccess.App.Localization;
using TeleAccess.Core;

namespace TeleAccess.App.Commands;

public static class CommandCatalog
{
    public static CommandRegistry Create(UiActions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        var registry = new CommandRegistry();

        Add(registry, "global.focusChats", Strings.Cmd_FocusChats, CommandScope.Global, "Alt+1", actions.FocusChats);
        Add(registry, "global.focusMessages", Strings.Cmd_FocusMessages, CommandScope.Global, "Alt+2", actions.FocusMessages);
        Add(registry, "global.focusComposer", Strings.Cmd_FocusComposer, CommandScope.Global, "Alt+3", actions.FocusComposer);
        Add(registry, "global.focusSearch", Strings.Main_Search, CommandScope.Global, "Alt+4", actions.FocusSearch);
        Add(registry, "global.search", Strings.Cmd_Search, CommandScope.Global, "Ctrl+K", actions.Search);
        Add(registry, "global.refresh", Strings.Cmd_Refresh, CommandScope.Global, "F5", actions.Refresh);
        Add(registry, "global.hotkeys", Strings.Cmd_Hotkeys, CommandScope.Global, "Ctrl+Shift+K", actions.Hotkeys);
        Add(registry, "global.quit", Strings.Cmd_Quit, CommandScope.Global, null, actions.Quit);

        Add(registry, "chatList.open", Strings.Cmd_OpenChat, CommandScope.ChatList, "Enter", actions.OpenChat, actions.HasChat);
        Add(registry, "chatList.delete", Strings.Cmd_DeleteChat, CommandScope.ChatList, "Ctrl+D", actions.DeleteChat, actions.HasChat);
        Add(registry, "chatList.markUnread", Strings.Cmd_MarkUnread, CommandScope.ChatList, "Ctrl+Shift+U", actions.MarkUnread, actions.HasChat);

        Add(registry, "message.read", Strings.Cmd_Listen, CommandScope.Message, "Enter", actions.ReadAloud, actions.HasMessage);
        Add(registry, "message.reply", Strings.Cmd_Reply, CommandScope.Message, "Ctrl+R", actions.Reply, actions.HasMessage);
        Add(registry, "message.edit", Strings.Cmd_Edit, CommandScope.Message, "Ctrl+E", actions.Edit, actions.HasMessage);
        Add(registry, "message.delete", Strings.Cmd_Delete, CommandScope.Message, "Ctrl+D", actions.DeleteMessage, actions.HasMessage);
        Add(registry, "message.forward", Strings.Cmd_Forward, CommandScope.Message, "Ctrl+Shift+F", actions.Forward, actions.HasMessage);
        Add(registry, "message.pin", Strings.Cmd_Pin, CommandScope.Message, "Ctrl+P", actions.Pin, actions.HasMessage);
        Add(registry, "message.copy", Strings.Cmd_Copy, CommandScope.Message, "Ctrl+C", actions.Copy, actions.HasMessage);

        Add(registry, "composer.send", Strings.Cmd_Send, CommandScope.Composer, "Enter", actions.Send, actions.HasChat);
        Add(registry, "composer.newline", Strings.Cmd_Newline, CommandScope.Composer, "Shift+Enter", actions.NewLine, actions.HasChat);
        Add(registry, "composer.voice", Strings.Cmd_Voice, CommandScope.Composer, "Ctrl+Shift+V", actions.Voice, actions.HasChat);

        return registry;
    }

    public static ContextMenu BuildContextMenu(CommandRegistry registry, CommandScope scope)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var menu = new ContextMenu();

        foreach (var command in registry.Commands.Where(c => c.Scope == scope))
        {
            var execute = command.Execute;
            if (execute is null)
            {
                continue;
            }

            var item = new MenuItem
            {
                Header = command.Title,
                InputGestureText = command.Hotkey ?? string.Empty,
                IsEnabled = command.IsAvailable,
                Command = new RelayCommand(execute)
            };

            menu.Items.Add(item);
        }

        return menu;
    }

    private static void Add(
        CommandRegistry registry,
        string id,
        string title,
        CommandScope scope,
        string? hotkey,
        Action execute,
        Func<bool>? canExecute = null)
    {
        registry.Add(new TeleCommand
        {
            Id = id,
            Title = title,
            Scope = scope,
            DefaultHotkey = hotkey,
            Execute = execute,
            CanExecute = canExecute
        });
    }
}
