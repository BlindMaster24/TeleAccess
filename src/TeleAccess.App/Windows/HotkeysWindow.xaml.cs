using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TeleAccess.App.Input;
using TeleAccess.App.Localization;
using TeleAccess.Core;
using TeleAccess.Core.Data;

namespace TeleAccess.App.Windows;

public partial class HotkeysWindow : Window
{
    private readonly CommandRegistry _registry;
    private readonly KeymapRepository _keymap;
    private readonly ObservableCollection<HotkeyRow> _rows = [];
    private HotkeyRow? _capturing;

    public HotkeysWindow(CommandRegistry registry, KeymapRepository keymap)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(keymap);

        InitializeComponent();

        _registry = registry;
        _keymap = keymap;
        CommandList.ItemsSource = _rows;

        Refresh();
        if (_rows.Count > 0)
        {
            CommandList.SelectedIndex = 0;
        }

        PreviewKeyDown += OnCaptureKey;
        Loaded += (_, _) => CommandList.Focus();
    }

    private void Refresh()
    {
        var selected = CommandList.SelectedIndex;
        _rows.Clear();

        foreach (var command in _registry.Commands)
        {
            _rows.Add(new HotkeyRow(command.Id, command.Title, command.Hotkey ?? string.Empty));
        }

        if (selected >= 0 && selected < _rows.Count)
        {
            CommandList.SelectedIndex = selected;
        }
    }

    private void OnListKeyDown(object sender, KeyEventArgs e)
    {
        if (CommandList.SelectedItem is not HotkeyRow row)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Enter:
                _capturing = row;
                Say(Strings.Hotkeys_PressKey);
                e.Handled = true;
                break;

            case Key.Delete:
                _registry.ResetCommand(row.Id);
                _keymap.Save(_registry);
                Refresh();
                Say(Strings.Hotkeys_Saved);
                e.Handled = true;
                break;

            default:
                break;
        }
    }

    private void OnCaptureKey(object sender, KeyEventArgs e)
    {
        if (_capturing is null)
        {
            return;
        }

        e.Handled = true;

        if (e.Key == Key.Escape)
        {
            _capturing = null;
            Say(Strings.Hotkeys_Cancelled);
            return;
        }

        if (!HotkeyService.TryFromKeyEvent(e, out var combination))
        {
            return;
        }

        var result = _registry.TrySetHotkey(_capturing.Id, combination.ToString(), out var conflict);
        _capturing = null;

        switch (result)
        {
            case SetHotkeyResult.Ok:
                _keymap.Save(_registry);
                Refresh();
                Say(Strings.Hotkeys_Saved);
                break;

            case SetHotkeyResult.Conflict:
                Say(string.Format(Strings.Culture, Strings.Hotkeys_Conflict, conflict?.Title ?? string.Empty));
                break;

            default:
                Say(Strings.Hotkeys_Invalid);
                break;
        }
    }

    private void Say(string text)
    {
        StatusText.Text = text;
        System.Diagnostics.Debug.WriteLine(text);
    }
}
