namespace TeleAccess.App.Services;

public sealed record TopicRow(
    int Id,
    string Name,
    int UnreadCount,
    bool IsPinned,
    bool IsClosed,
    long Order,
    long LastMessageId)
{
    public string Display => UnreadCount > 0 ? $"{Name} ({UnreadCount})" : Name;
}
