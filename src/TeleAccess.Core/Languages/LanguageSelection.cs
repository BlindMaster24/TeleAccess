namespace TeleAccess.Core.Languages;

public static class LanguageSelection
{
    public const string System = "system";

    public static string Resolve(string? systemCode, IEnumerable<string> available, string fallback)
    {
        ArgumentNullException.ThrowIfNull(available);
        ArgumentException.ThrowIfNullOrWhiteSpace(fallback);

        var candidates = available as IReadOnlyList<string> ?? [.. available];

        if (string.IsNullOrWhiteSpace(systemCode))
        {
            return fallback;
        }

        var code = systemCode.Trim();
        var exact = candidates.FirstOrDefault(candidate =>
            string.Equals(candidate, code, StringComparison.OrdinalIgnoreCase));

        if (exact is not null)
        {
            return exact;
        }

        var shortCode = code.Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries)[0];
        var partial = candidates.FirstOrDefault(candidate =>
            string.Equals(candidate, shortCode, StringComparison.OrdinalIgnoreCase));

        return partial ?? fallback;
    }

    public static bool IsSystem(string? value) =>
        string.Equals(value, System, StringComparison.OrdinalIgnoreCase);
}
