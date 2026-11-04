using TeleAccess.Core.Data;

namespace TeleAccess.Core.Accounts;

public sealed class AccountRepository
{
    private readonly AppDatabase _database;

    public AccountRepository(AppDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    public IReadOnlyList<AccountEntry> List()
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name FROM accounts ORDER BY id;";

        var result = new List<AccountEntry>();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new AccountEntry(reader.GetString(0), reader.GetString(1)));
        }

        return result;
    }

    public AccountEntry Add(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var id = AccountPaths.NextId(List());

        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO accounts (id, name) VALUES ($id, $name);";
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$name", name.Trim());
        command.ExecuteNonQuery();

        return new AccountEntry(id, name.Trim());
    }

    public void Rename(string id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE accounts SET name = $name WHERE id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$name", name.Trim());
        command.ExecuteNonQuery();
    }

    public void Remove(string id)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM accounts WHERE id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public static string? GetActive(SettingsRepository settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return settings.Get("active_account");
    }

    public static void SetActive(SettingsRepository settings, string id)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.Set("active_account", id);
    }
}
