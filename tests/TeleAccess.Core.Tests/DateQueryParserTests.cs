using Shouldly;
using TeleAccess.Core.Dates;

namespace TeleAccess.Core.Tests;

public class DateQueryParserTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 4, 12, 0, 0, TimeSpan.FromHours(3));

    [Theory]
    [InlineData("2026-11-04", 2026, 11, 4)]
    [InlineData("04.11.2026", 2026, 11, 4)]
    [InlineData("04/11/2026", 2026, 11, 4)]
    [InlineData("2020.01.31", 2020, 1, 31)]
    public void Parses_absolute_dates(string text, int year, int month, int day)
    {
        DateQueryParser.TryParse(text, _now, out var date).ShouldBeTrue();
        date.ShouldBe(new DateOnly(year, month, day));
    }

    [Theory]
    [InlineData("-7d", 2026, 10, 28)]
    [InlineData("-1", 2026, 11, 3)]
    public void Parses_relative_days(string text, int year, int month, int day)
    {
        DateQueryParser.TryParse(text, _now, out var date).ShouldBeTrue();
        date.Year.ShouldBe(year);
        date.Month.ShouldBe(month);
        date.Day.ShouldBe(day);
        date.ShouldBeLessThan(DateOnly.FromDateTime(_now.DateTime));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("yesterday")]
    [InlineData("2026-13-01")]
    [InlineData("-0d")]
    public void Rejects_invalid_queries(string text)
    {
        DateQueryParser.TryParse(text, _now, out _).ShouldBeFalse();
    }

    [Fact]
    public void Rejects_null()
    {
        DateQueryParser.TryParse(null, _now, out _).ShouldBeFalse();
    }
}
