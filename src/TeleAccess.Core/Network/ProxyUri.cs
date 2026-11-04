using System.Globalization;

namespace TeleAccess.Core.Network;

public sealed record ProxyUri(string Host, int Port, string Username, string Password)
{
    private static bool TryParseHostAndPort(string value, out string host, out int port)
    {
        host = value;
        port = 1080;
        var colon = value.LastIndexOf(':');

        if (colon >= 0)
        {
            host = value[..colon];

            if (!int.TryParse(value[(colon + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out port) ||
                port is <= 0 or > 65535)
            {
                return false;
            }
        }

        return !string.IsNullOrWhiteSpace(host);
    }

    private static (string Username, string Password) SplitCredentials(string credentials)
    {
        if (credentials.Length == 0)
        {
            return (string.Empty, string.Empty);
        }

        var split = credentials.IndexOf(':', StringComparison.Ordinal);
        return split >= 0
            ? (credentials[..split], credentials[(split + 1)..])
            : (credentials, string.Empty);
    }

    public static bool TryParse(string? text, out ProxyUri proxy)
    {
        proxy = new ProxyUri(string.Empty, 0, string.Empty, string.Empty);

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var value = text.Trim();

        var schemeSeparator = value.IndexOf("://", StringComparison.Ordinal);
        if (schemeSeparator >= 0)
        {
            var scheme = value[..schemeSeparator];

            if (!scheme.Equals("socks5", StringComparison.OrdinalIgnoreCase) &&
                !scheme.Equals("socks", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            value = value[(schemeSeparator + 3)..];
        }

        var credentials = string.Empty;
        var at = value.LastIndexOf('@');

        if (at >= 0)
        {
            credentials = value[..at];
            value = value[(at + 1)..];
        }

        if (!TryParseHostAndPort(value, out var host, out var port))
        {
            return false;
        }

        var (username, password) = SplitCredentials(credentials);

        proxy = new ProxyUri(host, port, username, password);
        return true;
    }
}
