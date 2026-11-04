using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using TeleAccess.Core;
using TeleAccess.Core.Hotkeys;

namespace TeleAccess.App.Input;

public sealed partial class GlobalHotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModNoRepeat = 0x4000;

    private readonly HwndSource _source;
    private readonly Action _callback;
    private readonly int _id;
    public bool IsRegistered { get; private set; }

    public GlobalHotkeyService(Window window, HotkeyCombination combination, Action callback)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(callback);

        _callback = callback;
        _id = combination.GetHashCode() & 0xBFFF;
        _source = (HwndSource)PresentationSource.FromVisual(window) ?? new HwndSource(new HwndSourceParameters());
        _source.AddHook(OnMessage);

        var modifiers = ModNoRepeat;

        if (combination.Modifiers.HasFlag(HotkeyModifiers.Control))
        {
            modifiers |= ModControl;
        }

        if (combination.Modifiers.HasFlag(HotkeyModifiers.Alt))
        {
            modifiers |= ModAlt;
        }

        if (combination.Modifiers.HasFlag(HotkeyModifiers.Shift))
        {
            modifiers |= ModShift;
        }

        IsRegistered = VirtualKeys.TryMap(combination.Key, out var virtualKey) &&
            RegisterHotKey(_source.Handle, _id, modifiers, virtualKey);
    }

    public void Dispose()
    {
        if (IsRegistered)
        {
            _ = UnregisterHotKey(_source.Handle, _id);
            IsRegistered = false;
        }

        _source.RemoveHook(OnMessage);
    }

    private IntPtr OnMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == WmHotkey && wParam.ToInt32() == _id)
        {
            _callback();
            handled = true;
        }

        return IntPtr.Zero;
    }

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr window, int id, uint modifiers, uint virtualKey);

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr window, int id);
}
