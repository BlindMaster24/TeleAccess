using System.IO;
using Shouldly;
using TeleAccess.Core.Data;

namespace TeleAccess.Core.Tests;

public class KeymapRepositoryTests
{
    private static CommandRegistry Build()
    {
        var registry = new CommandRegistry();
        registry.Add(new TeleCommand
        {
            Id = "message.delete",
            Title = "delete",
            Scope = CommandScope.Message,
            DefaultHotkey = "Ctrl+D"
        });
        registry.Add(new TeleCommand
        {
            Id = "global.search",
            Title = "search",
            Scope = CommandScope.Global,
            DefaultHotkey = "Ctrl+K"
        });
        return registry;
    }

    private static string TemporaryDatabase()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"teleaccess-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "test.db");
    }

    [Fact]
    public void Saves_and_restores_assignments()
    {
        var path = TemporaryDatabase();
        try
        {
            var database = new AppDatabase(path);
            var original = Build();
            original.TrySetHotkey("message.delete", "Ctrl+Shift+D", out _).ShouldBe(SetHotkeyResult.Ok);
            original.TrySetHotkey("global.search", null, out _).ShouldBe(SetHotkeyResult.Cleared);

            new KeymapRepository(database).Save(original);

            var restored = Build();
            new KeymapRepository(database).Load(restored);

            restored.Find("message.delete")!.Hotkey.ShouldBe("Ctrl+Shift+D");
            restored.Find("global.search")!.Hotkey.ShouldBeNull();
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, recursive: true);
        }
    }

    [Fact]
    public void Ignores_unknown_commands_from_an_older_version()
    {
        var path = TemporaryDatabase();
        try
        {
            var database = new AppDatabase(path);
            using (var connection = database.OpenConnection())
            {
                using var command = connection.CreateCommand();
                command.CommandText = """
                    INSERT INTO keymap (command_id, hotkey) VALUES ('ghost.command', 'Ctrl+Q');
                    INSERT INTO keymap (command_id, hotkey) VALUES ('global.search', 'Ctrl+J');
                    """;
                command.ExecuteNonQuery();
            }

            var registry = Build();
            new KeymapRepository(database).Load(registry);

            registry.Find("ghost.command").ShouldBeNull();
            registry.Find("global.search")!.Hotkey.ShouldBe("Ctrl+J");
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, recursive: true);
        }
    }
}
