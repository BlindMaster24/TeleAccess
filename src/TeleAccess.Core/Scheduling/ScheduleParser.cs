using System.Globalization;

namespace TeleAccess.Core.Scheduling;

public static class ScheduleParser
{
    private static readonly string[] _timeFormats = ["HH:mm", "H:mm"];

    private static readonly string[] _dateFormats =
    [
        "yyyy-MM-dd HH:mm",
        "yyyy-MM-dd H:mm",
        "dd.MM.yyyy HH:mm",
        "dd.MM.yyyy H:mm"
    ];

    public static bool TryParse(string? text, DateTimeOffset now, out DateTimeOffset result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var value = text.Trim();

        if (value.StartsWith('+'))
        {
            return TryParseOffset(value[1..], now, out result);
        }

        if (DateTime.TryParseExact(value, _timeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
        {
            var candidate = now.Date.Add(time.TimeOfDay);
            if (candidate <= now.DateTime)
            {
                candidate = candidate.AddDays(1);
            }

            result = At(candidate);
            return true;
        }

        if (DateTime.TryParseExact(value, _dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            if (date <= now.DateTime)
            {
                return false;
            }

            result = At(date);
            return true;
        }

        return false;
    }

    private static bool TryParseOffset(string text, DateTimeOffset now, out DateTimeOffset result)
    {
        result = default;

        if (text.Length < 2)
        {
            return false;
        }

        var unit = char.ToLowerInvariant(text[^1]);
        if (!int.TryParse(text[..^1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            return false;
        }

        var offset = unit switch
        {
            'm' => TimeSpan.FromMinutes(amount),
            'h' => TimeSpan.FromHours(amount),
            'd' => TimeSpan.FromDays(amount),
            _ => TimeSpan.Zero
        };

        if (offset == TimeSpan.Zero)
        {
            return false;
        }

        result = now.Add(offset);
        return true;
    }

    private static DateTimeOffset At(DateTime local) =>
        new(local, TimeZoneInfo.Local.GetUtcOffset(local));
}
