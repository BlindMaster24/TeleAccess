using Shouldly;
using TeleAccess.Core.Data;

namespace TeleAccess.Core.Tests;

public class SettingsRepositoryTests
{
    private static string TemporaryDatabase()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"teleaccess-settings-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "test.db");
    }

    [Fact]
    public void Stores_and_overwrites_values()
    {
        var path = TemporaryDatabase();
        try
        {
            var repository = new SettingsRepository(new AppDatabase(path));

            repository.Get("api_id").ShouldBeNull();

            repository.Set("api_id", "12345");
            repository.Set("api_hash", "secret");
            repository.Set("api_id", "67890");

            repository.Get("api_id").ShouldBe("67890");
            repository.Get("api_hash").ShouldBe("secret");
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, recursive: true);
        }
    }

    [Fact]
    public void Returns_all_settings()
    {
        var path = TemporaryDatabase();
        try
        {
            var repository = new SettingsRepository(new AppDatabase(path));
            repository.Set("language", "ru");
            repository.Set("phone", "+10000000000");

            var all = repository.GetAll();

            all.Count.ShouldBe(2);
            all["language"].ShouldBe("ru");
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, recursive: true);
        }
    }
}
