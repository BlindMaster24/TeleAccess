using Shouldly;
using TeleAccess.Core.Formatting;

namespace TeleAccess.Core.Tests;

public class SimpleMarkupTests
{
    [Fact]
    public void Plain_text_has_no_entities()
    {
        var result = SimpleMarkup.Parse("hello");

        result.Text.ShouldBe("hello");
        result.Entities.ShouldBeEmpty();
    }

    [Fact]
    public void Bold_is_stripped_and_marked()
    {
        var result = SimpleMarkup.Parse("a **bold** b");

        result.Text.ShouldBe("a bold b");
        result.Entities.ShouldBe([new EntitySpan(2, 4, EntityKind.Bold)]);
    }

    [Fact]
    public void Double_underscore_is_bold_too()
    {
        var result = SimpleMarkup.Parse("__heavy__");

        result.Text.ShouldBe("heavy");
        result.Entities[0].Kind.ShouldBe(EntityKind.Bold);
    }

    [Fact]
    public void Single_underscore_is_italic()
    {
        var result = SimpleMarkup.Parse("an _italic_ word");

        result.Text.ShouldBe("an italic word");
        result.Entities.ShouldBe([new EntitySpan(3, 6, EntityKind.Italic)]);
    }

    [Fact]
    public void Backticks_make_code()
    {
        var result = SimpleMarkup.Parse("run `dotnet test` now");

        result.Text.ShouldBe("run dotnet test now");
        result.Entities.ShouldBe([new EntitySpan(4, 11, EntityKind.Code)]);
    }

    [Fact]
    public void Several_entities_keep_their_offsets()
    {
        var result = SimpleMarkup.Parse("**a** and _b_");

        result.Text.ShouldBe("a and b");
        result.Entities.Count.ShouldBe(2);
        result.Entities[0].ShouldBe(new EntitySpan(0, 1, EntityKind.Bold));
        result.Entities[1].ShouldBe(new EntitySpan(6, 1, EntityKind.Italic));
    }

    [Theory]
    [InlineData("**open")]
    [InlineData("****")]
    [InlineData("*italic")]
    [InlineData("`code")]
    public void Unbalanced_markup_stays_literal(string input)
    {
        var result = SimpleMarkup.Parse(input);

        result.Text.ShouldBe(input);
        result.Entities.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Empty_input_is_safe(string? input)
    {
        var result = SimpleMarkup.Parse(input);

        result.Text.ShouldBeEmpty();
        result.Entities.ShouldBeEmpty();
    }
}
