namespace TeleAccess.Core.Hotkeys;

public static class VirtualKeys
{
    public const ushort Enter = 0x0D;

    public const ushort Escape = 0x1B;

    public const ushort Tab = 0x09;

    public const ushort Space = 0x20;

    public const ushort Backspace = 0x08;

    public const ushort Delete = 0x2E;

    public const ushort Insert = 0x2D;

    public const ushort Home = 0x24;

    public const ushort End = 0x23;

    public const ushort PageUp = 0x21;

    public const ushort PageDown = 0x22;

    public const ushort Left = 0x25;

    public const ushort Up = 0x26;

    public const ushort Right = 0x27;

    public const ushort Down = 0x28;

    private static readonly Dictionary<string, ushort> Named = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Enter"] = Enter,
        ["Return"] = Enter,
        ["Esc"] = Escape,
        ["Escape"] = Escape,
        ["Tab"] = Tab,
        ["Space"] = Space,
        ["Backspace"] = Backspace,
        ["Delete"] = Delete,
        ["Insert"] = Insert,
        ["Home"] = Home,
        ["End"] = End,
        ["PageUp"] = PageUp,
        ["PageDown"] = PageDown,
        ["Left"] = Left,
        ["Up"] = Up,
        ["Right"] = Right,
        ["Down"] = Down,
        [","] = 0xBC,
        ["."] = 0xBE,
        ["-"] = 0xBD,
        ["="] = 0xBB,
        ["+"] = 0xBB,
        ["/"] = 0xBF
    };

    public static bool TryMap(string key, out ushort virtualKey)
    {
        virtualKey = 0;

        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var value = key.Trim();

        if (value.Length == 1)
        {
            var character = char.ToUpperInvariant(value[0]);

            if (character is >= 'A' and <= 'Z')
            {
                virtualKey = (ushort)character;
                return true;
            }

            if (character is >= '0' and <= '9')
            {
                virtualKey = (ushort)character;
                return true;
            }
        }

        if (value.Length is 2 or 3 &&
            (value[0] is 'F' or 'f') &&
            ushort.TryParse(value[1..], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var functionNumber) &&
            functionNumber is >= 1 and <= 24)
        {
            virtualKey = (ushort)(0x70 + functionNumber - 1);
            return true;
        }

        return Named.TryGetValue(value, out virtualKey);
    }
}
