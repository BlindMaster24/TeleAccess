namespace TeleAccess.App.Commands;

public sealed record ComposerActions(
    Action Send,
    Action SendSilent,
    Action SendScheduled,
    Action Attach,
    Action NewLine,
    Action Voice);
