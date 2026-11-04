using System.Globalization;
using System.Windows;
using System.Windows.Input;
using TeleAccess.App.Localization;
using TeleAccess.Core.Scheduling;

namespace TeleAccess.App.Windows;

public partial class ScheduleWindow : Window
{
    private readonly record struct Preset(string Title, Func<DateTimeOffset> Build);

    private readonly List<Preset> _presets;

    private DateTimeOffset? _result;

    public ScheduleWindow()
    {
        InitializeComponent();

        var now = DateTimeOffset.Now;
        _presets =
        [
            new Preset(Strings.Schedule_Preset10Minutes, () => DateTimeOffset.Now.AddMinutes(10)),
            new Preset(Strings.Schedule_PresetHour, () => DateTimeOffset.Now.AddHours(1)),
            new Preset(Strings.Schedule_PresetThreeHours, () => DateTimeOffset.Now.AddHours(3)),
            new Preset(Strings.Schedule_PresetTomorrow, () => ScheduleComposer.Tomorrow(DateTimeOffset.Now, 9)),
            new Preset(Strings.Schedule_PresetMonday, () => ScheduleComposer.NextMonday(DateTimeOffset.Now))
        ];

        PresetList.ItemsSource = _presets.Select(p => p.Title).ToArray();
        DateBox.SelectedDate = now.Date.AddDays(1);
        TimeBox.Text = "09:00";

        Loaded += (_, _) =>
        {
            PresetList.Focus();
            if (PresetList.Items.Count > 0)
            {
                PresetList.SelectedIndex = 0;
            }
        };
    }

    public static DateTimeOffset? Ask(Window owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        var window = new ScheduleWindow { Owner = owner };
        return window.ShowDialog() == true ? window._result : null;
    }

    private void OnPresetKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ApplySelectedPreset();
            e.Handled = true;
        }
    }

    private void OnPresetDoubleClick(object sender, MouseButtonEventArgs e) => ApplySelectedPreset();

    private void ApplySelectedPreset()
    {
        if (PresetList.SelectedIndex < 0 || PresetList.SelectedIndex >= _presets.Count)
        {
            Say(Strings.Schedule_Invalid);
            return;
        }

        Accept(_presets[PresetList.SelectedIndex].Build());
    }

    private void OnSendClick(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(ManualBox.Text))
        {
            if (ScheduleParser.TryParse(ManualBox.Text, DateTimeOffset.Now, out var parsed))
            {
                Accept(parsed);
                return;
            }

            Say(Strings.Error_BadTime);
            ManualBox.Focus();
            return;
        }

        if (DateBox.SelectedDate is not { } date || !TryReadTime(TimeBox.Text, out var time))
        {
            Say(Strings.Error_BadTime);
            TimeBox.Focus();
            return;
        }

        if (!ScheduleComposer.TryCombine(date, time, DateTimeOffset.Now, out var combined))
        {
            Say(Strings.Error_BadTime);
            return;
        }

        Accept(combined);
    }

    private static bool TryReadTime(string text, out TimeOnly time) =>
        TimeOnly.TryParseExact(text.Trim(), ["HH:mm", "H:mm"], CultureInfo.InvariantCulture, DateTimeStyles.None, out time);

    private void Accept(DateTimeOffset when)
    {
        _result = when;
        DialogResult = true;
    }

    private void Say(string text) => StatusText.Text = text;
}
