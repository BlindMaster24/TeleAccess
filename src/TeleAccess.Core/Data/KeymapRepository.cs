namespace TeleAccess.Core.Data;

public sealed class KeymapRepository
{
    private readonly AppDatabase _database;

    public KeymapRepository(AppDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    public void Save(CommandRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        using var connection = _database.OpenConnection();
        using var transaction = connection.BeginTransaction();

        using (var clear = connection.CreateCommand())
        {
            clear.Transaction = transaction;
            clear.CommandText = "DELETE FROM keymap;";
            clear.ExecuteNonQuery();
        }

        foreach (var command in registry.Commands)
        {
            using var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = "INSERT INTO keymap (command_id, hotkey) VALUES ($id, $hotkey);";
            insert.Parameters.AddWithValue("$id", command.Id);
            insert.Parameters.AddWithValue("$hotkey", command.Hotkey ?? string.Empty);
            insert.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public void Load(CommandRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        foreach (var (id, hotkey) in ReadAll())
        {
            registry.TrySetHotkey(id, string.IsNullOrEmpty(hotkey) ? null : hotkey, out _);
        }
    }

    public IReadOnlyDictionary<string, string> ReadAll()
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT command_id, hotkey FROM keymap;";

        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result[reader.GetString(0)] = reader.GetString(1);
        }

        return result;
    }
}
