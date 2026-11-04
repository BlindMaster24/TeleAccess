using System.Globalization;
using System.IO;
using TeleAccess.Core.Data;

namespace TeleAccess.App.Services;

public sealed class AppSettings
{
    public int ApiId { get; set; }

    public string ApiHash { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Language { get; set; } = "en";

    public bool IsConfigured => ApiId > 0 && ApiHash.Length > 0;

    public static string DefaultDatabasePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TeleAccess",
        "teleaccess.db");

    public static AppSettings Load(SettingsRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var settings = new AppSettings
        {
            ApiId = int.TryParse(repository.Get("api_id"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var apiId) ? apiId : 0,
            ApiHash = repository.Get("api_hash") ?? string.Empty,
            PhoneNumber = repository.Get("phone") ?? string.Empty,
            Language = repository.Get("language") ?? "en"
        };

        return settings;
    }

    public void Save(SettingsRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        repository.Set("api_id", ApiId.ToString(CultureInfo.InvariantCulture));
        repository.Set("api_hash", ApiHash);
        repository.Set("phone", PhoneNumber);
        repository.Set("language", Language);
    }
}
