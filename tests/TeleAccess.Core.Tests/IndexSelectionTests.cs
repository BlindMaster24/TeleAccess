using Shouldly;
using TeleAccess.Core.Selection;

namespace TeleAccess.Core.Tests;

public class IndexSelectionTests
{
    [Theory]
    [InlineData("1", 3, 0)]
    [InlineData("3", 3, 2)]
    [InlineData(" 2 ", 3, 1)]
    public void Converts_one_based_answers(string answer, int count, int expected)
    {
        IndexSelection.TrySelect(answer, count, out var index).ShouldBeTrue();
        index.ShouldBe(expected);
    }

    [Theory]
    [InlineData("0", 3)]
    [InlineData("4", 3)]
    [InlineData("-1", 3)]
    public void Rejects_out_of_range_answers(string answer, int count)
    {
        IndexSelection.TrySelect(answer, count, out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("two")]
    public void Rejects_non_numeric_answers(string answer)
    {
        IndexSelection.TrySelect(answer, 3, out _).ShouldBeFalse();
    }

    [Fact]
    public void Rejects_an_empty_collection()
    {
        IndexSelection.TrySelect("1", 0, out _).ShouldBeFalse();
    }

    [Fact]
    public void Rejects_null()
    {
        IndexSelection.TrySelect(null, 3, out _).ShouldBeFalse();
    }
}
