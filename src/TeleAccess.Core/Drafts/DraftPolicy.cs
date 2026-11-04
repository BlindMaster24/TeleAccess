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

    public static string? Normalize(string? current)
    {
        if (string.IsNullOrWhiteSpace(current))
        {
            return null;
        }

        return current;
    }
}
