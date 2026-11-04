using Shouldly;
using TeleAccess.Core.Search;

namespace TeleAccess.Core.Tests;

public class SearchQueryParserTests
{
    [Fact]
    public void Plain_text_stays_text()
    {
        var query = SearchQueryParser.Parse("hello world");

        query.Text.ShouldBe("hello world");
        query.Filter.ShouldBe(SearchFilterKind.Any);
        query.MinDate.ShouldBe(0);
    }

    [Theory]
    [InlineData("type:photo", SearchFilterKind.Photo)]
    [InlineData("TYPE:video", SearchFilterKind.Video)]
    [InlineData("type:doc", SearchFilterKind.Document)]
    [InlineData("type:music", SearchFilterKind.Audio)]
    [InlineData("type:voice", SearchFilterKind.Voice)]
    [InlineData("type:link", SearchFilterKind.Link)]
    public void Type_tokens_select_a_filter(string input, SearchFilterKind expected)
    {
        SearchQueryParser.Parse(input).Filter.ShouldBe(expected);
    }

    [Fact]
    public void Unknown_type_is_ignored()
    {
        var query = SearchQueryParser.Parse("type:banana cat");

        query.Filter.ShouldBe(SearchFilterKind.Any);
        query.Text.ShouldBe("cat");
    }

    [Fact]
    public void Date_token_limits_the_day()
    {
        var query = SearchQueryParser.Parse("date:2026-11-04 report");

        query.Text.ShouldBe("report");
        query.MinDate.ShouldBeGreaterThan(0);
        query.MaxDate.ShouldBe(query.MinDate + 86399);
    }

    [Fact]
    public void Broken_date_stays_in_the_text()
    {
        var query = SearchQueryParser.Parse("date:tomorrow report");

        query.MinDate.ShouldBe(0);
        query.Text.ShouldBe("date:tomorrow report");
    }

    [Fact]
    public void Text_and_tokens_combine()
    {
        var query = SearchQueryParser.Parse("type:photo date:2026-11-04 sunset");

        query.Text.ShouldBe("sunset");
        query.Filter.ShouldBe(SearchFilterKind.Photo);
        query.MinDate.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Colon_inside_a_word_is_kept()
    {
        SearchQueryParser.Parse("http://example").Text.ShouldBe("http://example");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Empty_input_gives_an_empty_query(string? input)
    {
        SearchQueryParser.Parse(input).ShouldBe(SearchQuery.Empty);
    }
}
