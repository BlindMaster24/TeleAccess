using Shouldly;

namespace TeleAccess.Core.Tests;

public class HotkeyCombinationTests
{
    [Theory]
    [InlineData("ctrl+shift+v", "Ctrl+Shift+V")]
    [InlineData("Alt+1", "Alt+1")]
    [InlineData(" Enter ", "Enter")]
    [InlineData("meta+k", "Win+K")]
    public void Parses_and_normalizes(string text, string expected)
    {
        HotkeyCombination.TryParse(text, out var combination).ShouldBeTrue();
        combination.ToString().ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Ctrl+")]
    [InlineData("Hyper+V")]
    [InlineData("Ctrl")]
    [InlineData("Shift")]
    public void Rejects_invalid_input(string text)
    {
        HotkeyCombination.TryParse(text, out _).ShouldBeFalse();
    }

    [Fact]
    public void Compares_keys_case_insensitively()
    {
        HotkeyCombination.TryParse("Ctrl+D", out var first).ShouldBeTrue();
        HotkeyCombination.TryParse("ctrl+d", out var second).ShouldBeTrue();

        first.IsSameAs(second).ShouldBeTrue();
    }

    [Fact]
    public void Different_modifiers_are_not_equal()
    {
        HotkeyCombination.TryParse("Ctrl+D", out var withControl).ShouldBeTrue();
        HotkeyCombination.TryParse("D", out var bare).ShouldBeTrue();

        withControl.IsSameAs(bare).ShouldBeFalse();
    }
}
