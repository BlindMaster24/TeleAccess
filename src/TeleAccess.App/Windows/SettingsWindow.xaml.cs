using System.Globalization;
using System.Windows;
using TeleAccess.App.Localization;
using TeleAccess.App.Services;
using TeleAccess.Core.Data;

namespace TeleAccess.App.Windows;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;
    private readonly SettingsRepository _repository;

    public SettingsWindow(AppSettings settings, SettingsRepository repository)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(repository);

        InitializeComponent();

        _settings = settings;
        _repository = repository;

        var languages = LocalizationManager.SortedByEnglishName();
        LanguageBox.ItemsSource = languages;
        LanguageBox.SelectedItem = languages.FirstOrDefault(language =>
            string.Equals(language.Culture, _settings.Language, StringComparison.Ordinal)) ?? languages[0];

        ApiIdBox.Text = _settings.ApiId > 0 ? _settings.ApiId.ToString(CultureInfo.InvariantCulture) : string.Empty;
        ApiHashBox.Text = _settings.ApiHash;
        PhoneBox.Text = _settings.PhoneNumber;
    }

    public bool SignOutRequested { get; private set; }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(ApiIdBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var apiId))
        {
            _settings.ApiId = apiId;
        }

        _settings.ApiHash = ApiHashBox.Text.Trim();
        _settings.PhoneNumber = PhoneBox.Text.Trim();

        if (LanguageBox.SelectedItem is LanguageOption language)
        {
            _settings.Language = language.Culture;
        }

        _settings.Save(_repository);
        StatusText.Text = Strings.Settings_Saved;
        DialogResult = true;
    }

    private void OnSignOutClick(object sender, RoutedEventArgs e)
    {
        SignOutRequested = true;
        DialogResult = true;
    }
}
