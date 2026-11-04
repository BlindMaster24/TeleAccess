namespace TeleAccess.Core.Profiles;

public static class NameSplitter
{
    public static bool TrySplit(string? fullName, out string firstName, out string lastName)
    {
        firstName = string.Empty;
        lastName = string.Empty;

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return false;
        }

        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return false;
        }

        firstName = parts[0];
        lastName = parts.Length > 1 ? string.Join(' ', parts[1..]) : string.Empty;
        return true;
    }
}
