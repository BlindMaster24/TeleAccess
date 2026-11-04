using System.Globalization;
using SmartFormat;

namespace TeleAccess.Core;

public static class PluralFormatter
{
    public static string Format(string pattern, int count, CultureInfo? culture = null) =>
        Smart.Format(culture ?? CultureInfo.CurrentUICulture, pattern, count);
}
