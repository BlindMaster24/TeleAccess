using System.Globalization;

namespace TeleAccess.App.Localization;

public static class LocalizationManager
{
    public static IReadOnlyList<LanguageOption> Languages { get; } =
    [
        new("en", "English", "English"),
        new("ru", "Русский", "Russian")
    ];

    public static LanguageOption Current { get; private set; } = Languages[0];

    public static void Apply(string? culture)
    {
        var chosen = Languages.FirstOrDefault(l =>
            string.Equals(l.Culture, culture, StringComparison.OrdinalIgnoreCase)) ?? Languages[0];

        var info = CultureInfo.GetCultureInfo(chosen.Culture);
        CultureInfo.CurrentUICulture = info;
        CultureInfo.DefaultThreadCurrentUICulture = info;
        Strings.Culture = info;
        Current = chosen;
    }

    public static IReadOnlyList<LanguageOption> SortedByEnglishName() =>
        Languages.OrderBy(l => l.EnglishName, StringComparer.Ordinal).ToArray();
}
