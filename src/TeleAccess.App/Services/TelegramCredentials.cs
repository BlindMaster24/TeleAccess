namespace TeleAccess.App.Services;

public static class TelegramCredentials
{
    public const int DefaultApiId = 0;

    public const string DefaultApiHash = "";

    public static (int ApiId, string ApiHash) Resolve(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.IsConfigured)
        {
            return (settings.ApiId, settings.ApiHash);
        }

        return (DefaultApiId, DefaultApiHash);
    }
}
