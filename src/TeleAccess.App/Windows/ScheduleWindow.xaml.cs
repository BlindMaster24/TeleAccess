using System.Globalization;
using System.Windows;
using System.Windows.Controls;
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

        FillSelectors(now);

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

    private void FillSelectors(DateTimeOffset now)
    {
        var culture = Strings.Culture;

        YearBox.ItemsSource = Enumerable.Range(now.Year - 1, 11).ToArray();
        MonthBox.ItemsSource = Enumerable.Range(1, 12).Select(m => new MonthItem(m, culture.DateTimeFormat.GetMonthName(m))).ToArray();
        HourBox.ItemsSource = Enumerable.Range(0, 24).Select(h => h.ToString("00", CultureInfo.InvariantCulture)).ToArray();
        MinuteBox.ItemsSource = Enumerable.Range(0, 12).Select(m => (m * 5).ToString("00", CultureInfo.InvariantCulture)).ToArray();

        YearBox.SelectedItem = now.Year;
        MonthBox.SelectedIndex = now.Month - 1;
        HourBox.SelectedItem = "09";
        MinuteBox.SelectedItem = "00";
        FillDays(now.Day);
    }

    private void FillDays(int selectDay)
    {
        if (YearBox.SelectedItem is not int year || MonthBox.SelectedItem is not MonthItem month)
        {
            return;
        }

        var days = DateTime.DaysInMonth(year, month.Number);
        DayBox.ItemsSource = Enumerable.Range(1, days).ToArray();
        DayBox.SelectedItem = Math.Min(selectDay, days);
    }

    private void OnDatePartChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DayBox is null)
        {
            return;
        }

        FillDays(DayBox.SelectedItem as int? ?? 1);
    }

    private void OnPresetKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ApplySelectedPreset();
            e.Handled = true;
        }
    }

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

        if (YearBox.SelectedItem is not int year ||
            MonthBox.SelectedItem is not MonthItem month ||
            DayBox.SelectedItem is not int day)
        {
            Say(Strings.Error_BadTime);
            return;
        }

        var hour = int.TryParse(HourBox.SelectedItem as string, CultureInfo.InvariantCulture, out var h) ? h : 0;
        var minute = int.TryParse(MinuteBox.SelectedItem as string, CultureInfo.InvariantCulture, out var m) ? m : 0;

        var date = new DateTime(year, month.Number, day, 0, 0, 0, DateTimeKind.Unspecified);

        if (!ScheduleComposer.TryCombine(date, new TimeOnly(hour, minute), DateTimeOffset.Now, out var combined))
        {
            Say(Strings.Error_BadTime);
            return;
        }

        Accept(combined);
    }

    private void Accept(DateTimeOffset when)
    {
        _result = when;
        DialogResult = true;
    }

    private void Say(string text) => StatusText.Text = text;

    private sealed record MonthItem(int Number, string Name)
    {
        public override string ToString() => Name;
    }
}
