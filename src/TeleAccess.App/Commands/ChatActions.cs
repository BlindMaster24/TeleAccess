namespace TeleAccess.App.Commands;

public sealed record ChatActions(
    Action Open,
    Action Delete,
    Action MarkUnread,
    Action Mute,
    Action Pin,
    Action Archive,
    Action ShowScheduled,
    Action SwitchList,
    Action Rename,
    Action InviteLink,
    Action BanMember,
    Action CreateFolder,
    Action DeleteFolder,
    Action RenameFolder);
