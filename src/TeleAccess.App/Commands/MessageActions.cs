namespace TeleAccess.App.Commands;

public sealed record MessageActions(
    Action ReadAloud,
    Action Reply,
    Action Edit,
    Action Delete,
    Action DeleteForMe,
    Action Forward,
    Action Pin,
    Action Copy,
    Action React,
    Action SearchInChat,
    Action JumpToPinned,
    Action Translate);
