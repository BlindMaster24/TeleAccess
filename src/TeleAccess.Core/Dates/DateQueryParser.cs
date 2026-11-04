using System.Globalization;

namespace TeleAccess.Core.Dates;

public static class DateQueryParser
{
    private static readonly string[] _formats =
    [
        "yyyy-MM-dd",
        "dd.MM.yyyy",
        "dd/MM/yyyy",
        "yyyy.MM.dd"
    ];

    public static bool TryParse(string? text, DateTimeOffset now, out DateOnly date)
    {
        date = default;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var value = text.Trim();

        if (value.Length > 1 && value[0] == '-' &&
            int.TryParse(value[1..].TrimEnd('d', 'D'), NumberStyles.Integer, CultureInfo.InvariantCulture, out var days) &&
            days > 0)
        {
            date = DateOnly.FromDateTime(now.DateTime.AddDays(-days));
            return true;
        }

        if (!DateTime.TryParseExact(value, _formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return false;
        }

        date = DateOnly.FromDateTime(parsed);
        return true;
    }
}
