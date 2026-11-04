namespace TeleAccess.App.Commands;

public sealed record UiActions(
    Func<bool> HasChat,
    Func<bool> HasMessage,
    Action FocusChats,
    Action FocusMessages,
    Action FocusComposer,
    Action FocusSearch,
    Action Search,
    Action Refresh,
    Action Hotkeys,
    Action Quit,
    ChatActions Chat,
    MessageActions Message,
    ComposerActions Composer);
