namespace TeleAccess.App.Services;

public sealed record MessageRow(
    long Id,
    long ChatId,
    string Sender,
    MessageKind Kind,
    string Text,
    string Time,
    bool IsOutgoing,
    bool IsPinned,
    int FileId)
{
    public string Display => string.IsNullOrEmpty(Sender)
        ? $"{Time}. {Text}"
        : $"{Sender}, {Time}. {Text}";

    public bool HasFile => FileId != 0;
}
