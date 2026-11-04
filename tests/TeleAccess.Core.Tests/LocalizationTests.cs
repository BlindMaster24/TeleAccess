using System.IO;
using System.Xml.Linq;
using Shouldly;
using TeleAccess.App.Localization;

namespace TeleAccess.Core.Tests;

public class LocalizationTests
{
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Directory.Build.props")))
        {
            directory = directory.Parent;
        }

        directory.ShouldNotBeNull("the repository root with Directory.Build.props must be reachable");

        return directory.FullName;
    }

    private static Dictionary<string, string> ReadResx(string name)
    {
        var path = Path.Combine(RepositoryRoot(), "src", "TeleAccess.App", "Localization", name);
        var document = XDocument.Load(path);

        return document.Root!
            .Elements("data")
            .ToDictionary(
                element => element.Attribute("name")!.Value,
                element => element.Element("value")?.Value ?? string.Empty,
                StringComparer.Ordinal);
    }

    [Fact]
    public void Russian_resources_cover_every_english_key()
    {
        var english = ReadResx("Strings.resx");
        var russian = ReadResx("Strings.ru.resx");

        var missing = english.Keys.Where(key => !russian.ContainsKey(key)).OrderBy(key => key, StringComparer.Ordinal).ToArray();

        missing.ShouldBeEmpty("these keys have no Russian translation: " + string.Join(", ", missing));
    }

    [Fact]
    public void No_resource_value_is_empty()
    {
        foreach (var (name, values) in new[] { ("Strings.resx", ReadResx("Strings.resx")), ("Strings.ru.resx", ReadResx("Strings.ru.resx")) })
        {
            var empty = values.Where(pair => string.IsNullOrWhiteSpace(pair.Value)).Select(pair => pair.Key).ToArray();
            empty.ShouldBeEmpty($"{name} has empty values: {string.Join(", ", empty)}");
        }
    }

    [Fact]
    public void Russian_resources_have_no_duplicate_keys()
    {
        var path = Path.Combine(RepositoryRoot(), "src", "TeleAccess.App", "Localization", "Strings.ru.resx");
        var root = XDocument.Load(path).Root;
        root.ShouldNotBeNull();
        var keys = root.Elements("data").Select(e => e.Attribute("name")!.Value).ToArray();

        keys.Distinct(StringComparer.Ordinal).Count().ShouldBe(keys.Length);
    }

    [Fact]
    public void Languages_are_sorted_by_english_name_and_include_english()
    {
        var languages = LocalizationManager.SortedByEnglishName();

        languages.ShouldContain(language => language.Culture == "en");
        languages.Select(language => language.EnglishName).ShouldBe(
            languages.Select(language => language.EnglishName).OrderBy(name => name, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void Unknown_culture_falls_back_to_english()
    {
        LocalizationManager.Apply("xx-unknown");
        LocalizationManager.Current.Culture.ShouldBe("en");
        Strings.AppTitle.ShouldNotBeNullOrWhiteSpace();
    }
}
