using Microsoft.Data.Sqlite;

namespace TeleAccess.Core.Data;

public sealed class AppDatabase
{
    private const string Schema = """
        CREATE TABLE IF NOT EXISTS settings (
            key   TEXT PRIMARY KEY NOT NULL,
            value TEXT NOT NULL
        );
        CREATE TABLE IF NOT EXISTS accounts (
            id   TEXT PRIMARY KEY NOT NULL,
            name TEXT NOT NULL
        );
        CREATE TABLE IF NOT EXISTS keymap (
            command_id TEXT PRIMARY KEY NOT NULL,
            hotkey     TEXT NOT NULL
        );
        """;

    private readonly string _connectionString;

    public AppDatabase(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = new SqliteConnectionStringBuilder { DataSource = path, Pooling = false }.ConnectionString;
        Migrate();
    }

    public SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private void Migrate()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = Schema;
        command.ExecuteNonQuery();
    }
}
