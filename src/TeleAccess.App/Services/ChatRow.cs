namespace TeleAccess.App.Services;

public sealed record ChatRow(long Id, string Title, MessageKind PreviewKind, string PreviewText, int Unread)
{
    public string Display => Unread > 0 ? $"{Title} ({Unread})" : Title;
}
