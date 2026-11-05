using Shouldly;
using TeleAccess.Core.Languages;

namespace TeleAccess.Core.Tests;

public class LanguageSelectionTests
{
    private static readonly string[] _available = ["en", "ru"];

    [Theory]
    [InlineData("ru", "ru")]
    [InlineData("RU", "ru")]
    [InlineData("en", "en")]
    public void Matches_an_available_language(string system, string expected)
    {
        LanguageSelection.Resolve(system, _available, "en").ShouldBe(expected);
    }

    [Theory]
    [InlineData("ru-RU", "ru")]
    [InlineData("en-US", "en")]
    public void Matches_by_language_part_of_a_region_code(string system, string expected)
    {
        LanguageSelection.Resolve(system, _available, "en").ShouldBe(expected);
    }

    [Theory]
    [InlineData("de")]
    [InlineData("")]
    [InlineData(null)]
    public void Falls_back_when_the_system_language_is_missing(string? system)
    {
        LanguageSelection.Resolve(system, _available, "en").ShouldBe("en");
    }

    [Fact]
    public void Recognises_the_system_marker()
    {
        LanguageSelection.IsSystem("system").ShouldBeTrue();
        LanguageSelection.IsSystem("SYSTEM").ShouldBeTrue();
        LanguageSelection.IsSystem("ru").ShouldBeFalse();
        LanguageSelection.IsSystem(null).ShouldBeFalse();
    }

    [Fact]
    public void Requires_arguments()
    {
        Should.Throw<ArgumentNullException>(() => LanguageSelection.Resolve("ru", null!, "en"));
        Should.Throw<ArgumentException>(() => LanguageSelection.Resolve("ru", _available, " "));
    }
}
