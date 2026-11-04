using Shouldly;
using TeleAccess.Core.Drafts;

namespace TeleAccess.Core.Tests;

public class DraftPolicyTests
{
    [Fact]
    public void Saves_a_new_draft()
    {
        DraftPolicy.ShouldSave("hello", null).ShouldBeTrue();
        DraftPolicy.ShouldSave("hello", string.Empty).ShouldBeTrue();
    }

    [Fact]
    public void Skips_an_unchanged_draft()
    {
        DraftPolicy.ShouldSave("hello", "hello").ShouldBeFalse();
        DraftPolicy.ShouldSave(string.Empty, string.Empty).ShouldBeFalse();
        DraftPolicy.ShouldSave(null, null).ShouldBeFalse();
    }

    [Fact]
    public void Saves_an_emptied_draft_to_replace_a_stored_one()
    {
        DraftPolicy.ShouldSave(string.Empty, "hello").ShouldBeTrue();
        DraftPolicy.ShouldSave(null, "hello").ShouldBeTrue();
    }

    [Theory]
    [InlineData(null, null, "hello", true)]
    [InlineData("", null, "hello", true)]
    [InlineData("hello", null, "hello", false)]
    [InlineData("typing", null, "hello", false)]
    [InlineData("hello", "hello", "hello", false)]
    [InlineData("hello", null, null, false)]
    public void Decides_whether_to_apply_a_restored_draft(string? typing, string? stored, string? incoming, bool expected)
    {
        DraftPolicy.ShouldApply(typing, stored, incoming).ShouldBe(expected);
    }

    [Fact]
    public void Limits_long_text_without_throwing()
    {
        DraftPolicy.Limit("abcdef", 4).ShouldBe("abcd");
        DraftPolicy.Limit("ab", 4).ShouldBe("ab");
        DraftPolicy.Limit(null, 4).ShouldBe(string.Empty);
        Should.Throw<ArgumentOutOfRangeException>(() => DraftPolicy.Limit("a", 0));
    }

    [Fact]
    public void Normalize_keeps_meaningful_text()
    {
        DraftPolicy.Normalize("hi").ShouldBe("hi");
        DraftPolicy.Normalize("  hi  ").ShouldBe("  hi  ");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Normalize_drops_blank_text(string? text)
    {
        DraftPolicy.Normalize(text).ShouldBeNull();
    }
}
