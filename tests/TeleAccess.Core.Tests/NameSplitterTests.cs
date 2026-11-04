using Shouldly;
using TeleAccess.Core.Profiles;

namespace TeleAccess.Core.Tests;

public class NameSplitterTests
{
    [Theory]
    [InlineData("Anna", "Anna", "")]
    [InlineData("Anna Petrova", "Anna", "Petrova")]
    [InlineData("  Anna   Maria  Petrova ", "Anna", "Maria Petrova")]
    public void Splits_first_and_last_name(string input, string expectedFirst, string expectedLast)
    {
        NameSplitter.TrySplit(input, out var first, out var last).ShouldBeTrue();
        first.ShouldBe(expectedFirst);
        last.ShouldBe(expectedLast);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_empty_names(string input)
    {
        NameSplitter.TrySplit(input, out _, out _).ShouldBeFalse();
    }

    [Fact]
    public void Rejects_null()
    {
        NameSplitter.TrySplit(null, out _, out _).ShouldBeFalse();
    }
}
