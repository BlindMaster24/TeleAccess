namespace TeleAccess.Core;

public readonly record struct HotkeyCombination
{
    private HotkeyCombination(HotkeyModifiers modifiers, string key)
    {
        Modifiers = modifiers;
        Key = key;
    }

    public HotkeyModifiers Modifiers { get; }

    public string Key { get; }

    public static HotkeyCombination Create(HotkeyModifiers modifiers, string key) => new(modifiers, key);

    public static bool TryParse(string? text, out HotkeyCombination result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var parts = text.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return false;
        }

        var modifiers = HotkeyModifiers.None;

        for (var i = 0; i < parts.Length - 1; i++)
        {
            if (!TryParseModifier(parts[i], out var modifier))
            {
                return false;
            }

            modifiers |= modifier;
        }

        var key = parts[^1];
        if (key.Length == 0 || IsModifierName(key))
        {
            return false;
        }

        result = new HotkeyCombination(modifiers, key.Length == 1 ? key.ToUpperInvariant() : key);
        return true;
    }

    public static string? Normalize(string? text) => TryParse(text, out var parsed) ? parsed.ToString() : null;

    public bool IsSameAs(HotkeyCombination other) =>
        Modifiers == other.Modifiers && string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase);

    public override string ToString()
    {
        var parts = new List<string>(5);

        if (Modifiers.HasFlag(HotkeyModifiers.Control))
        {
            parts.Add("Ctrl");
        }

        if (Modifiers.HasFlag(HotkeyModifiers.Shift))
        {
            parts.Add("Shift");
        }

        if (Modifiers.HasFlag(HotkeyModifiers.Alt))
        {
            parts.Add("Alt");
        }

        if (Modifiers.HasFlag(HotkeyModifiers.Windows))
        {
            parts.Add("Win");
        }

        parts.Add(Key);
        return string.Join("+", parts);
    }

    private static bool TryParseModifier(string value, out HotkeyModifiers modifier)
    {
        if (value.Equals("ctrl", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("control", StringComparison.OrdinalIgnoreCase))
        {
            modifier = HotkeyModifiers.Control;
            return true;
        }

        if (value.Equals("shift", StringComparison.OrdinalIgnoreCase))
        {
            modifier = HotkeyModifiers.Shift;
            return true;
        }

        if (value.Equals("alt", StringComparison.OrdinalIgnoreCase))
        {
            modifier = HotkeyModifiers.Alt;
            return true;
        }

        if (value.Equals("win", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("windows", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("meta", StringComparison.OrdinalIgnoreCase))
        {
            modifier = HotkeyModifiers.Windows;
            return true;
        }

        modifier = HotkeyModifiers.None;
        return false;
    }

    private static bool IsModifierName(string value) => TryParseModifier(value, out _);
}
