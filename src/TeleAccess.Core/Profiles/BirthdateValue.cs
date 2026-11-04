namespace TeleAccess.Core.Profiles;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
public readonly record struct BirthdateValue(int Day, int Month, int Year);
