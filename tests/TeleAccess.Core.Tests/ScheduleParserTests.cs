using Shouldly;
using TeleAccess.Core.Scheduling;

namespace TeleAccess.Core.Tests;

public class ScheduleParserTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 4, 12, 0, 0, TimeSpan.FromHours(3));

    [Theory]
    [InlineData("+10m", 10)]
    [InlineData("+2h", 120)]
    [InlineData("+1d", 1440)]
    public void Parses_relative_offsets(string text, int expectedMinutes)
    {
        ScheduleParser.TryParse(text, _now, out var result).ShouldBeTrue();
        result.ShouldBe(_now.AddMinutes(expectedMinutes));
    }

    [Fact]
    public void Parses_time_today_when_it_is_still_ahead()
    {
        ScheduleParser.TryParse("18:30", _now, out var result).ShouldBeTrue();
        result.DateTime.ShouldBe(new DateTime(2026, 11, 4, 18, 30, 0, DateTimeKind.Unspecified));
    }

    [Fact]
    public void Moves_time_to_tomorrow_when_it_has_passed()
    {
        ScheduleParser.TryParse("09:15", _now, out var result).ShouldBeTrue();
        result.DateTime.ShouldBe(new DateTime(2026, 11, 5, 9, 15, 0, DateTimeKind.Unspecified));
    }

    [Fact]
    public void Parses_absolute_date_and_time()
    {
        ScheduleParser.TryParse("2026-12-31 23:45", _now, out var result).ShouldBeTrue();
        result.DateTime.ShouldBe(new DateTime(2026, 12, 31, 23, 45, 0, DateTimeKind.Unspecified));
    }

    [Fact]
    public void Parses_russian_date_format()
    {
        ScheduleParser.TryParse("05.11.2026 08:00", _now, out var result).ShouldBeTrue();
        result.DateTime.ShouldBe(new DateTime(2026, 11, 5, 8, 0, 0, DateTimeKind.Unspecified));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("tomorrow")]
    [InlineData("+0m")]
    [InlineData("+10x")]
    [InlineData("+m")]
    [InlineData("2026-01-01 10:00")]
    public void Rejects_invalid_or_past_values(string text)
    {
        ScheduleParser.TryParse(text, _now, out _).ShouldBeFalse();
    }

    [Fact]
    public void Rejects_null()
    {
        ScheduleParser.TryParse(null, _now, out _).ShouldBeFalse();
    }
}
