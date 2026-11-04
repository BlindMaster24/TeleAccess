namespace TeleAccess.Core.Formatting;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
public readonly record struct EntitySpan(int Offset, int Length, EntityKind Kind);
