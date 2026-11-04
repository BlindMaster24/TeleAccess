using System.Globalization;

namespace TeleAccess.Core.Profiles;

public static class Birthdate
{
    public const int MinYear = 1900;

    private static readonly string[] _formats = ["yyyy-MM-dd", "dd.MM.yyyy", "dd/MM/yyyy"];

    public static bool TryParse(string? value, DateTimeOffset now, out BirthdateValue birthdate)
    {
        birthdate = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!DateTime.TryParseExact(value.Trim(), _formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return false;
        }

        if (parsed.Year < MinYear || parsed.Date > now.Date)
        {
            return false;
        }

        birthdate = new BirthdateValue(parsed.Day, parsed.Month, parsed.Year);
        return true;
    }
}
