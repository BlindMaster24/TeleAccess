namespace TeleAccess.App.Services;

public sealed record MessagePermissions(bool CanBeEdited, bool CanBeDeletedForAll, bool CanBeForwarded);
