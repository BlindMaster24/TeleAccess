namespace TeleAccess.Core.Drafts;

public static class DraftPolicy
{
    public const int MinLengthToStore = 1;

    public static bool ShouldSave(string? current, string? stored)
    {
        var draft = current ?? string.Empty;
        var previous = stored ?? string.Empty;

        if (string.Equals(draft, previous, StringComparison.Ordinal))
        {
            return false;
        }

        return draft.Length >= MinLengthToStore || previous.Length > 0;
    }

    public static bool ShouldApply(string? typing, string? stored, string? incoming)
    {
        if (incoming is null)
        {
            return false;
        }

        if (string.Equals(incoming, stored, StringComparison.Ordinal))
        {
            return false;
        }

        if (string.IsNullOrEmpty(typing))
        {
            return true;
        }

        return string.Equals(typing, stored, StringComparison.Ordinal);
    }

    public static string Limit(string? text, int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLength);

        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text.Length <= maxLength ? text : text[..maxLength];
    }

    public static string? Normalize(string? current)
    {
        if (string.IsNullOrWhiteSpace(current))
        {
            return null;
        }

        return current;
    }
}
