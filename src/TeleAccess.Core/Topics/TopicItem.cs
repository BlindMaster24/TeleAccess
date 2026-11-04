namespace TeleAccess.Core.Topics;

public sealed record TopicItem(int Id, string Name, int UnreadCount, bool IsPinned, long Order)
{
    public string Display => UnreadCount > 0 ? $"{Name} ({UnreadCount})" : Name;
}
