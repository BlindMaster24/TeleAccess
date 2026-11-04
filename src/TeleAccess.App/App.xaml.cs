using TeleAccess.Core.Data;
using TeleAccess.App.Localization;

namespace TeleAccess.App;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var database = new AppDatabase(Services.AppSettings.DefaultDatabasePath);
        var settings = Services.AppSettings.Load(new SettingsRepository(database));
        LocalizationManager.Apply(settings.Language);

        base.OnStartup(e);
    }
}
