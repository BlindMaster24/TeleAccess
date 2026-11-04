namespace TeleAccess.Core.Keyboards;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
public readonly record struct KeyboardPosition(int Row, int Column)
{
    public static KeyboardPosition Start { get; } = new(1, 1);
}
