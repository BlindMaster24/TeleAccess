namespace TeleAccess.Core.Accounts;

public static class AccountPaths
{
    public static bool IsValidId(string? id) =>
        !string.IsNullOrWhiteSpace(id) && id.All(character => char.IsAsciiLetterOrDigit(character));

    public static string DataDirectory(string root, string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        if (!IsValidId(id))
        {
            throw new ArgumentException("Account id must be alphanumeric.", nameof(id));
        }

        return Path.Combine(root, "accounts", id);
    }

    public static string NextId(IEnumerable<AccountEntry> existing)
    {
        ArgumentNullException.ThrowIfNull(existing);

        var numbers = existing
            .Select(account => int.TryParse(account.Id, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var number) ? number : 0)
            .DefaultIfEmpty(0)
            .ToList();

        return (numbers.Max() + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
