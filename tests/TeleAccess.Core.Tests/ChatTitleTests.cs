using Shouldly;
using TeleAccess.Core.Chats;

namespace TeleAccess.Core.Tests;

public class ChatTitleTests
{
    [Theory]
    [InlineData("Team", "Team")]
    [InlineData("  Team   chat ", "Team chat")]
    [InlineData("Один", "Один")]
    public void Normalizes_titles(string input, string expected)
    {
        ChatTitle.TryNormalize(input, out var normalized).ShouldBeTrue();
        normalized.ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_empty_titles(string input)
    {
        ChatTitle.TryNormalize(input, out _).ShouldBeFalse();
    }

    [Fact]
    public void Rejects_titles_over_the_limit()
    {
        var tooLong = new string('a', ChatTitle.MaxLength + 1);
        ChatTitle.TryNormalize(tooLong, out _).ShouldBeFalse();

        ChatTitle.TryNormalize(new string('a', ChatTitle.MaxLength), out var atLimit).ShouldBeTrue();
        atLimit.Length.ShouldBe(ChatTitle.MaxLength);
    }

    [Fact]
    public void Rejects_null()
    {
        ChatTitle.TryNormalize(null, out _).ShouldBeFalse();
    }
}
