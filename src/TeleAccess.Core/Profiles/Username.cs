namespace TeleAccess.Core.Profiles;

public static class Username
{
    public const int MinLength = 5;

    public const int MaxLength = 32;

    public static bool TryNormalize(string? value, out string username)
    {
        username = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().TrimStart('@');

        if (candidate.Length is < MinLength or > MaxLength)
        {
            return false;
        }

        if (!char.IsAsciiLetter(candidate[0]))
        {
            return false;
        }

        if (!candidate.All(character => char.IsAsciiLetterOrDigit(character) || character == '_'))
        {
            return false;
        }

        username = candidate;
        return true;
    }
}
