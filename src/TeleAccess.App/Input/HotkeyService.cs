using System.Windows;
using System.Windows.Input;
using TeleAccess.Core;

namespace TeleAccess.App.Input;

public sealed class HotkeyService
{
    private readonly CommandRegistry _registry;

    public HotkeyService(Window window, CommandRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(registry);

        _registry = registry;
        window.PreviewKeyDown += OnPreviewKeyDown;
    }

    public Func<CommandScope> ActiveScope { get; set; } = () => CommandScope.Global;

    public bool Suspended { get; set; }

    public event EventHandler<CommandInvokedEventArgs>? CommandInvoked;

    public static bool TryFromKeyEvent(KeyEventArgs e, out HotkeyCombination combination)
    {
        ArgumentNullException.ThrowIfNull(e);

        combination = default;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key is Key.LeftCtrl or Key.RightCtrl
            or Key.LeftShift or Key.RightShift
            or Key.LeftAlt or Key.RightAlt
            or Key.LWin or Key.RWin
            or Key.None)
        {
            return false;
        }

        var modifiers = HotkeyModifiers.None;
        var current = Keyboard.Modifiers;

        if (current.HasFlag(ModifierKeys.Control))
        {
            modifiers |= HotkeyModifiers.Control;
        }

        if (current.HasFlag(ModifierKeys.Shift))
        {
            modifiers |= HotkeyModifiers.Shift;
        }

        if (current.HasFlag(ModifierKeys.Alt))
        {
            modifiers |= HotkeyModifiers.Alt;
        }

        if (current.HasFlag(ModifierKeys.Windows))
        {
            modifiers |= HotkeyModifiers.Windows;
        }

        combination = HotkeyCombination.Create(modifiers, KeyText(key));
        return true;
    }

    private static string KeyText(Key key) => key switch
    {
        Key.OemComma => ",",
        Key.OemPeriod => ".",
        Key.OemPlus => "+",
        Key.OemMinus => "-",
        Key.OemQuestion => "/",
        Key.Space => "Space",
        Key.Return => "Enter",
        Key.Escape => "Esc",
        Key.Back => "Backspace",
        Key.Delete => "Delete",
        Key.Prior => "PageUp",
        Key.Next => "PageDown",
        _ => key.ToString()
    };

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Suspended)
        {
            return;
        }

        if (!TryFromKeyEvent(e, out var combination))
        {
            return;
        }

        var command = _registry.Resolve(ActiveScope(), combination);
        if (command is null)
        {
            return;
        }

        e.Handled = true;

        if (!command.IsAvailable)
        {
            CommandInvoked?.Invoke(this, new CommandInvokedEventArgs(command));
            return;
        }

        command.Execute?.Invoke();
        CommandInvoked?.Invoke(this, new CommandInvokedEventArgs(command));
    }
}
