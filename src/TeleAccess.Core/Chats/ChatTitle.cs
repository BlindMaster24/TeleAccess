namespace TeleAccess.Core.Chats;

public static class ChatTitle
{
    public const int MaxLength = 128;

    public static bool TryNormalize(string? title, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(title))
        {
            return false;
        }

        var collapsed = string.Join(' ', title.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        if (collapsed.Length is 0 or > MaxLength)
        {
            return false;
        }

        normalized = collapsed;
        return true;
    }
}
