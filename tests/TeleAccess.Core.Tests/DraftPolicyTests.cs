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
