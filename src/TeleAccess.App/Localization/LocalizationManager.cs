using System.Globalization;
using TeleAccess.Core.Languages;

namespace TeleAccess.App.Localization;

public static class LocalizationManager
{
    public static IReadOnlyList<LanguageOption> Languages { get; } =
    [
        new("en", "English", "English"),
        new("ru", "Русский", "Russian")
    ];

    public static string SystemLanguage => LanguageSelection.Resolve(
        CultureInfo.CurrentUICulture.Name,
        Languages.Select(language => language.Culture),
        "en");

    public static LanguageOption Current { get; private set; } = Languages[0];

    public static void Apply(string? culture)
    {
        var requested = LanguageSelection.IsSystem(culture) ? SystemLanguage : culture;

        var chosen = Languages.FirstOrDefault(l =>
            string.Equals(l.Culture, requested, StringComparison.OrdinalIgnoreCase)) ?? Languages[0];

        var info = CultureInfo.GetCultureInfo(chosen.Culture);
        CultureInfo.CurrentUICulture = info;
        CultureInfo.DefaultThreadCurrentUICulture = info;
        Strings.Culture = info;
        Current = chosen;
    }

    public static IReadOnlyList<LanguageOption> SortedByEnglishName() =>
    [
        new(LanguageSelection.System, "System", "0 System"),
        .. Languages.OrderBy(l => l.EnglishName, StringComparer.Ordinal)
    ];
}
