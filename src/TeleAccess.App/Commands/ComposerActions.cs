namespace TeleAccess.App.Commands;

public sealed record ComposerActions(
    Action Send,
    Action SendSilent,
    Action SendScheduled,
    Action NewLine,
    Action Voice);
