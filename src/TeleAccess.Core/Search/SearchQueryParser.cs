using System.Globalization;

namespace TeleAccess.Core.Search;

public static class SearchQueryParser
{
    public static SearchQuery Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return SearchQuery.Empty;
        }

        var words = new List<string>();
        var filter = SearchFilterKind.Any;
        var minDate = 0;
        var maxDate = 0;

        foreach (var raw in input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var colon = raw.IndexOf(':', StringComparison.Ordinal);

            if (colon <= 0)
            {
                words.Add(raw);
                continue;
            }

            var key = raw[..colon];
            var value = raw[(colon + 1)..];

            if (key.Equals("type", StringComparison.OrdinalIgnoreCase))
            {
                if (TryParseFilter(value, out var parsed))
                {
                    filter = parsed;
                }

                continue;
            }

            if (key.Equals("date", StringComparison.OrdinalIgnoreCase) &&
                DateTime.TryParseExact(value, ["yyyy-MM-dd", "dd.MM.yyyy"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var day))
            {
                var start = new DateTimeOffset(day.Date, TimeZoneInfo.Local.GetUtcOffset(day.Date));
                minDate = (int)start.ToUnixTimeSeconds();
                maxDate = minDate + 86399;
                continue;
            }

            words.Add(raw);
        }

        return new SearchQuery(string.Join(' ', words), filter, minDate, maxDate);
    }

    private static bool TryParseFilter(string value, out SearchFilterKind filter)
    {
        filter = value.ToUpperInvariant() switch
        {
            "PHOTO" or "PHOTOS" => SearchFilterKind.Photo,
            "VIDEO" or "VIDEOS" => SearchFilterKind.Video,
            "FILE" or "FILES" or "DOC" or "DOCUMENT" => SearchFilterKind.Document,
            "AUDIO" or "MUSIC" => SearchFilterKind.Audio,
            "VOICE" => SearchFilterKind.Voice,
            "LINK" or "LINKS" or "URL" => SearchFilterKind.Link,
            _ => SearchFilterKind.Any
        };

        return filter != SearchFilterKind.Any;
    }
}
