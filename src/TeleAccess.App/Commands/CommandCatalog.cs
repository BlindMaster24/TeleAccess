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

        AddGlobal(registry, actions);
        AddChatList(registry, actions);
        AddMessage(registry, actions);
        AddComposer(registry, actions);

        return registry;
    }

    private static void AddGlobal(CommandRegistry registry, UiActions actions)
    {
        Add(registry, "global.focusChats", Strings.Cmd_FocusChats, CommandScope.Global, "Alt+1", actions.FocusChats);
        Add(registry, "global.focusMessages", Strings.Cmd_FocusMessages, CommandScope.Global, "Alt+2", actions.FocusMessages);
        Add(registry, "global.focusComposer", Strings.Cmd_FocusComposer, CommandScope.Global, "Alt+3", actions.FocusComposer);
        Add(registry, "global.focusSearch", Strings.Main_Search, CommandScope.Global, "Alt+4", actions.FocusSearch);
        Add(registry, "global.search", Strings.Cmd_Search, CommandScope.Global, "Ctrl+K", actions.Search);
        Add(registry, "global.searchGlobal", Strings.Cmd_GlobalSearch, CommandScope.Global, "Ctrl+Shift+F", actions.SearchGlobal);
        Add(registry, "global.chatInfo", Strings.Cmd_ChatInfo, CommandScope.Global, "Ctrl+I", actions.ChatInfo, actions.HasChat);
        Add(registry, "global.chatMembers", Strings.Cmd_ChatMembers, CommandScope.Global, "Ctrl+Shift+I", actions.ChatMembers, actions.HasChat);
        Add(registry, "global.chatTopics", Strings.Cmd_ChatTopics, CommandScope.Global, null, actions.ChatTopics, actions.HasChat);
        Add(registry, "global.chatJoin", Strings.Cmd_ChatJoin, CommandScope.Global, "Ctrl+J", actions.ChatJoin, actions.HasChat);
        Add(registry, "global.contacts", Strings.Cmd_Contacts, CommandScope.Global, "Ctrl+Shift+C", actions.Contacts);
        Add(registry, "global.secretChat", Strings.Cmd_SecretChat, CommandScope.Global, "Ctrl+Shift+X", actions.SecretChat);
        Add(registry, "global.setName", Strings.Cmd_SetName, CommandScope.Global, "Ctrl+Shift+N", actions.SetName);
        Add(registry, "global.setBio", Strings.Cmd_SetBio, CommandScope.Global, null, actions.SetBio);
        Add(registry, "global.muteScopes", Strings.Cmd_MuteScopes, CommandScope.Global, null, actions.MuteScopes);
        Add(registry, "global.stories", Strings.Cmd_Stories, CommandScope.Global, null, actions.Stories, actions.HasChat);
        Add(registry, "global.addAccount", Strings.Cmd_AddAccount, CommandScope.Global, null, actions.AddAccount);
        Add(registry, "global.switchAccount", Strings.Cmd_SwitchAccount, CommandScope.Global, null, actions.SwitchAccount);
        Add(registry, "global.removeAccount", Strings.Cmd_RemoveAccount, CommandScope.Global, null, actions.RemoveAccount);
        Add(registry, "global.enableProxy", Strings.Cmd_EnableProxy, CommandScope.Global, null, actions.EnableProxy);
        Add(registry, "global.removeProxy", Strings.Cmd_RemoveProxy, CommandScope.Global, null, actions.RemoveProxy);
        Add(registry, "global.disableProxy", Strings.Cmd_DisableProxy, CommandScope.Global, null, actions.DisableProxy);
        Add(registry, "global.proxy", Strings.Cmd_Proxy, CommandScope.Global, null, actions.Proxy);
        Add(registry, "global.setUsername", Strings.Cmd_SetUsername, CommandScope.Global, null, actions.SetUsername);
        Add(registry, "global.setBirthdate", Strings.Cmd_SetBirthdate, CommandScope.Global, null, actions.SetBirthdate);
        Add(registry, "global.training", Strings.Cmd_Training, CommandScope.Global, "Ctrl+F1", actions.Training);
        Add(registry, "global.settings", Strings.Cmd_Settings, CommandScope.Global, "Ctrl+Comma", actions.Settings);
        Add(registry, "global.refresh", Strings.Cmd_Refresh, CommandScope.Global, "F5", actions.Refresh);
        Add(registry, "global.hotkeys", Strings.Cmd_Hotkeys, CommandScope.Global, "Ctrl+Shift+K", actions.Hotkeys);
        Add(registry, "global.quit", Strings.Cmd_Quit, CommandScope.Global, null, actions.Quit);
    }

    private static void AddChatList(CommandRegistry registry, UiActions actions)
    {
        Add(registry, "chatList.open", Strings.Cmd_OpenChat, CommandScope.ChatList, "Enter", actions.Chat.Open, actions.HasChat);
        Add(registry, "chatList.delete", Strings.Cmd_DeleteChat, CommandScope.ChatList, "Ctrl+D", actions.Chat.Delete, actions.HasChat);
        Add(registry, "chatList.markUnread", Strings.Cmd_MarkUnread, CommandScope.ChatList, "Ctrl+Shift+U", actions.Chat.MarkUnread, actions.HasChat);
        Add(registry, "chatList.mute", Strings.Cmd_Mute, CommandScope.ChatList, "Ctrl+Shift+M", actions.Chat.Mute, actions.HasChat);
        Add(registry, "chatList.pin", Strings.Cmd_PinChat, CommandScope.ChatList, "Ctrl+Shift+P", actions.Chat.Pin, actions.HasChat);
        Add(registry, "chatList.archive", Strings.Cmd_ArchiveChat, CommandScope.ChatList, "Ctrl+Shift+A", actions.Chat.Archive, actions.HasChat);
        Add(registry, "chatList.createFolder", Strings.Cmd_CreateFolder, CommandScope.ChatList, "Ctrl+Alt+F", actions.Chat.CreateFolder);
        Add(registry, "chatList.renameFolder", Strings.Cmd_RenameFolder, CommandScope.ChatList, null, actions.Chat.RenameFolder, actions.HasChat);
        Add(registry, "chatList.deleteFolder", Strings.Cmd_DeleteFolder, CommandScope.ChatList, "Ctrl+Alt+D", actions.Chat.DeleteFolder, actions.HasChat);
        Add(registry, "chatList.secretState", Strings.Cmd_SecretState, CommandScope.ChatList, null, actions.Chat.SecretState, actions.HasChat);
        Add(registry, "chatList.searchSecret", Strings.Cmd_SearchSecret, CommandScope.ChatList, null, actions.Chat.SearchSecret, actions.HasChat);
        Add(registry, "chatList.findMember", Strings.Cmd_FindMember, CommandScope.ChatList, null, actions.Chat.FindMember, actions.HasChat);
        Add(registry, "chatList.unbanMember", Strings.Cmd_UnbanMember, CommandScope.ChatList, null, actions.UnbanMember, actions.HasChat);
        Add(registry, "chatList.banMember", Strings.Cmd_BanMember, CommandScope.ChatList, "Ctrl+Alt+B", actions.Chat.BanMember, actions.HasChat);
        Add(registry, "chatList.inviteLink", Strings.Cmd_InviteLink, CommandScope.ChatList, "Ctrl+Shift+I", actions.Chat.InviteLink, actions.HasChat);
        Add(registry, "chatList.rename", Strings.Cmd_Rename, CommandScope.ChatList, "F2", actions.Chat.Rename, actions.HasChat);
        Add(registry, "chatList.scheduled", Strings.Cmd_Scheduled, CommandScope.ChatList, "Ctrl+Shift+S", actions.Chat.ShowScheduled, actions.HasChat);
        Add(registry, "chatList.switchList", Strings.Cmd_SwitchList, CommandScope.ChatList, "Ctrl+Shift+L", actions.Chat.SwitchList);
    }

    private static void AddMessage(CommandRegistry registry, UiActions actions)
    {
        Add(registry, "messageList.pinned", Strings.Cmd_JumpPinned, CommandScope.MessageList, "Ctrl+Shift+P", actions.Message.JumpToPinned, actions.HasChat);
        Add(registry, "message.read", Strings.Cmd_Listen, CommandScope.Message, "Enter", actions.Message.ReadAloud, actions.HasMessage);
        Add(registry, "message.reply", Strings.Cmd_Reply, CommandScope.Message, "Ctrl+R", actions.Message.Reply, actions.HasMessage);
        Add(registry, "message.edit", Strings.Cmd_Edit, CommandScope.Message, "Ctrl+E", actions.Message.Edit, actions.HasMessage);
        Add(registry, "message.delete", Strings.Cmd_Delete, CommandScope.Message, "Ctrl+D", actions.Message.Delete, actions.HasMessage);
        Add(registry, "message.deleteForMe", Strings.Cmd_DeleteForMe, CommandScope.Message, "Ctrl+Shift+D", actions.Message.DeleteForMe, actions.HasMessage);
        Add(registry, "message.forward", Strings.Cmd_Forward, CommandScope.Message, "Ctrl+Shift+F", actions.Message.Forward, actions.HasMessage);
        Add(registry, "message.pin", Strings.Cmd_Pin, CommandScope.Message, "Ctrl+P", actions.Message.Pin, actions.HasMessage);
        Add(registry, "message.buttons", Strings.Cmd_Buttons, CommandScope.Message, "Ctrl+B", actions.Message.Buttons, actions.HasMessage);
        Add(registry, "message.copy", Strings.Cmd_Copy, CommandScope.Message, "Ctrl+C", actions.Message.Copy, actions.HasMessage);
        Add(registry, "messageList.jumpToDate", Strings.Cmd_JumpToDate, CommandScope.MessageList, "Ctrl+G", actions.Message.JumpToDate, actions.HasChat);
        Add(registry, "message.select", Strings.Cmd_Select, CommandScope.Message, "Space", actions.Message.Select, actions.HasMessage);
        Add(registry, "messageList.selectAll", Strings.Cmd_SelectAll, CommandScope.MessageList, "Ctrl+A", actions.Message.SelectAll, actions.HasChat);
        Add(registry, "message.save", Strings.Cmd_Save, CommandScope.Message, "Ctrl+S", actions.Message.Save, actions.HasMessage);
        Add(registry, "message.vote", Strings.Cmd_Vote, CommandScope.Message, null, actions.Message.Vote, actions.HasMessage);
        Add(registry, "message.link", Strings.Cmd_MessageLink, CommandScope.Message, null, actions.Message.MessageLink, actions.HasMessage);
        Add(registry, "message.stopPoll", Strings.Cmd_StopPoll, CommandScope.Message, null, actions.Message.StopPoll, actions.HasMessage);
        Add(registry, "message.thread", Strings.Cmd_Thread, CommandScope.Message, null, actions.Message.Thread, actions.HasMessage);
        Add(registry, "message.replied", Strings.Cmd_ShowReplied, CommandScope.Message, null, actions.Message.ShowReplied, actions.HasMessage);
        Add(registry, "message.translate", Strings.Cmd_Translate, CommandScope.Message, "Ctrl+Shift+T", actions.Message.Translate, actions.HasMessage);
        Add(registry, "message.react", Strings.Cmd_React, CommandScope.Message, "Ctrl+Shift+E", actions.Message.React, actions.HasMessage);
        Add(registry, "message.searchInChat", Strings.Cmd_SearchInChat, CommandScope.Message, "Ctrl+F", actions.Message.SearchInChat, actions.HasChat);
    }

    private static void AddComposer(CommandRegistry registry, UiActions actions)
    {
        Add(registry, "messageList.topicOpen", Strings.Cmd_TopicOpen, CommandScope.MessageList, "Enter", actions.Message.OpenTopic);
        Add(registry, "messageList.topicBack", Strings.Cmd_TopicBack, CommandScope.MessageList, "Backspace", actions.Message.BackToTopics);
        Add(registry, "messageList.topicCreate", Strings.Cmd_TopicCreate, CommandScope.MessageList, "Ctrl+Alt+N", actions.Message.CreateTopic, actions.HasChat);
        Add(registry, "messageList.topicRename", Strings.Cmd_TopicRename, CommandScope.MessageList, null, actions.Message.RenameTopic, actions.HasChat);
        Add(registry, "messageList.topicRead", Strings.Cmd_TopicRead, CommandScope.MessageList, null, actions.Message.ReadTopic, actions.HasChat);
        Add(registry, "messageList.topicLink", Strings.Cmd_TopicLink, CommandScope.MessageList, null, actions.Message.TopicLink, actions.HasChat);
        Add(registry, "messageList.topicClose", Strings.Cmd_TopicClose, CommandScope.MessageList, null, actions.Message.CloseTopic, actions.HasChat);
        Add(registry, "messageList.topicDelete", Strings.Cmd_TopicDelete, CommandScope.MessageList, null, actions.Message.DeleteTopic, actions.HasChat);
        Add(registry, "messageList.topicPin", Strings.Cmd_TopicPin, CommandScope.MessageList, null, actions.Message.PinTopic, actions.HasChat);
        Add(registry, "messageList.older", Strings.Cmd_Older, CommandScope.MessageList, "Ctrl+Up", actions.Message.Older, actions.HasChat);
        Add(registry, "composer.send", Strings.Cmd_Send, CommandScope.Composer, "Enter", actions.Composer.Send, actions.HasChat);
        Add(registry, "composer.sendSilent", Strings.Cmd_SendSilent, CommandScope.Composer, "Ctrl+Enter", actions.Composer.SendSilent, actions.HasChat);
        Add(registry, "composer.sendScheduled", Strings.Cmd_SendScheduled, CommandScope.Composer, "Ctrl+Shift+Enter", actions.Composer.SendScheduled, actions.HasChat);
        Add(registry, "composer.attach", Strings.Cmd_Attach, CommandScope.Composer, "Ctrl+O", actions.Composer.Attach, actions.HasChat);
        Add(registry, "composer.contact", Strings.Cmd_SendContact, CommandScope.Composer, "Ctrl+Alt+C", actions.Composer.SendContact, actions.HasChat);
        Add(registry, "composer.location", Strings.Cmd_SendLocation, CommandScope.Composer, "Ctrl+Alt+L", actions.Composer.SendLocation, actions.HasChat);
        Add(registry, "composer.poll", Strings.Cmd_SendPoll, CommandScope.Composer, "Ctrl+Alt+P", actions.Composer.SendPoll, actions.HasChat);
        Add(registry, "composer.newline", Strings.Cmd_Newline, CommandScope.Composer, "Shift+Enter", actions.Composer.NewLine, actions.HasChat);
        Add(registry, "composer.voice", Strings.Cmd_Voice, CommandScope.Composer, "Ctrl+Shift+V", actions.Composer.Voice, actions.HasChat);
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
