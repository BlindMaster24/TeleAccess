namespace TeleAccess.App.Services;

public sealed record SendOptions(bool Silent = false, DateTimeOffset? SendAt = null, bool WhenOnline = false)
{
    public static SendOptions Normal { get; } = new();
}
