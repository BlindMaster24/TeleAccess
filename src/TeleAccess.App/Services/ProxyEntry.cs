namespace TeleAccess.App.Services;

public sealed record ProxyEntry(int Id, string Server, int Port, bool IsEnabled, string Comment)
{
    public string Display => $"{Server}:{Port}{(IsEnabled ? " (on)" : string.Empty)}";
}
