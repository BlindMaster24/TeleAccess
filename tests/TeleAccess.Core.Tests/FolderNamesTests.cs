using Shouldly;
using TeleAccess.Core.ChatLists;

namespace TeleAccess.Core.Tests;

public class FolderNamesTests
{
    [Fact]
    public void Accepts_a_new_name()
    {
        FolderNames.TryPrepare("  Work  ", ["Home"], out var name).ShouldBeTrue();
        name.ShouldBe("Work");
    }

    [Theory]
    [InlineData("work")]
    [InlineData("WORK")]
    public void Rejects_duplicates_ignoring_case(string candidate)
    {
        FolderNames.TryPrepare(candidate, ["Work", "Home"], out _).ShouldBeFalse();
    }

    [Fact]
    public void Rejects_names_that_are_only_whitespace()
    {
        FolderNames.TryPrepare("   ", [], out _).ShouldBeFalse();
    }

    [Fact]
    public void Rejects_null()
    {
        FolderNames.TryPrepare(null, [], out _).ShouldBeFalse();
    }

    [Fact]
    public void Rejects_names_over_the_limit()
    {
        FolderNames.TryPrepare(new string('a', 200), [], out _).ShouldBeFalse();
    }

    [Fact]
    public void Requires_the_existing_collection()
    {
        Should.Throw<ArgumentNullException>(() => FolderNames.TryPrepare("Work", null!, out _));
    }
}
