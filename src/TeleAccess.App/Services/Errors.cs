using System.IO;
using System.Text.RegularExpressions;
using TdLib;
using TeleAccess.App.Localization;

namespace TeleAccess.App.Services;

public static class Errors
{
    private static readonly Regex RetryAfter = new(
        @"retry\safter\s(?<seconds>\d+)",
        RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture,
        TimeSpan.FromMilliseconds(200));

    public static string Describe(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            TdException td => DescribeTd(td),
            TimeoutException => Strings.Error_Network,
            IOException => Strings.Error_File,
            _ => string.Format(Strings.Culture, Strings.Error_Generic, exception.Message)
        };
    }

    private static string DescribeTd(TdException exception)
    {
        var message = exception.Message ?? string.Empty;

        var retry = RetryAfter.Match(message);
        if (retry.Success)
        {
            return string.Format(Strings.Culture, Strings.Error_FloodWait, retry.Groups["seconds"].Value);
        }

        if (message.Contains("too many requests", StringComparison.OrdinalIgnoreCase))
        {
            return string.Format(Strings.Culture, Strings.Error_FloodWait, "60");
        }

        if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return Strings.Error_NotFound;
        }

        if (message.Contains("rights", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("permission", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("forbidden", StringComparison.OrdinalIgnoreCase))
        {
            return Strings.Error_Permission;
        }

        if (message.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("auth", StringComparison.OrdinalIgnoreCase))
        {
            return Strings.Error_Auth;
        }

        return string.Format(Strings.Culture, Strings.Error_Generic, message);
    }
}
