namespace TeleAccess.App.Services;

public sealed record ChatRow(
    long Id,
    string Title,
    MessageKind PreviewKind,
    string PreviewText,
    int Unread,
    bool IsMuted,
    bool IsPinned,
    bool IsArchived,
    bool IsForum)
{
    public string Display => Unread > 0 ? $"{Title} ({Unread})" : Title;
}
