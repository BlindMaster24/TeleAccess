using TeleAccess.Core.Chats;

namespace TeleAccess.Core.ChatLists;

public static class FolderNames
{
    public static bool TryPrepare(string? candidate, IEnumerable<string> existing, out string name)
    {
        name = string.Empty;
        ArgumentNullException.ThrowIfNull(existing);

        if (!ChatTitle.TryNormalize(candidate, out var normalized))
        {
            return false;
        }

        if (existing.Any(taken => string.Equals(taken, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        name = normalized;
        return true;
    }
}
