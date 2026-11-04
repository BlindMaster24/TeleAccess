using System.IO;
using Shouldly;
using TeleAccess.Core.Accounts;
using TeleAccess.Core.Data;

namespace TeleAccess.Core.Tests;

public class AccountTests
{
    private static string TemporaryDatabase()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"teleaccess-accounts-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "test.db");
    }

    [Fact]
    public void Adds_and_lists_accounts_with_increasing_ids()
    {
        var path = TemporaryDatabase();
        try
        {
            var database = new AppDatabase(path);
            var repository = new AccountRepository(database);

            repository.List().ShouldBeEmpty();

            var first = repository.Add("Personal");
            var second = repository.Add("Work");

            first.Id.ShouldBe("1");
            second.Id.ShouldBe("2");
            repository.List().Count.ShouldBe(2);
            repository.List()[1].Name.ShouldBe("Work");
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, recursive: true);
        }
    }

    [Fact]
    public void Renames_and_removes_an_account()
    {
        var path = TemporaryDatabase();
        try
        {
            var repository = new AccountRepository(new AppDatabase(path));
            var account = repository.Add("Old");

            repository.Rename(account.Id, "New");
            repository.List()[0].Name.ShouldBe("New");

            repository.Remove(account.Id);
            repository.List().ShouldBeEmpty();
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, recursive: true);
        }
    }

    [Fact]
    public void Remembers_the_active_account()
    {
        var path = TemporaryDatabase();
        try
        {
            var database = new AppDatabase(path);
            var settings = new SettingsRepository(database);

            AccountRepository.GetActive(settings).ShouldBeNull();

            AccountRepository.SetActive(settings, "2");
            AccountRepository.GetActive(settings).ShouldBe("2");
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, recursive: true);
        }
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("account7", true)]
    [InlineData("", false)]
    [InlineData("../evil", false)]
    [InlineData("a/b", false)]
    public void Validates_ids(string id, bool expected)
    {
        AccountPaths.IsValidId(id).ShouldBe(expected);
    }

    [Fact]
    public void Builds_a_data_directory_per_account()
    {
        AccountPaths.DataDirectory(@"C:\data", "3").ShouldBe(Path.Combine(@"C:\data", "accounts", "3"));
        Should.Throw<ArgumentException>(() => AccountPaths.DataDirectory(@"C:\data", "../x"));
    }

    [Fact]
    public void Next_id_continues_the_sequence()
    {
        AccountPaths.NextId([]).ShouldBe("1");
        AccountPaths.NextId([new AccountEntry("1", "a"), new AccountEntry("4", "b")]).ShouldBe("5");
    }
}
