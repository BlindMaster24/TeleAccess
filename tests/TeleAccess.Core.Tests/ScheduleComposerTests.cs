using Shouldly;
using TeleAccess.Core.Scheduling;

namespace TeleAccess.Core.Tests;

public class ScheduleComposerTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 4, 12, 0, 0, TimeSpan.FromHours(3));

    [Fact]
    public void Combines_date_and_time_in_the_future()
    {
        ScheduleComposer.TryCombine(new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Unspecified), new TimeOnly(9, 30), _now, out var result)
            .ShouldBeTrue();

        result.DateTime.ShouldBe(new DateTime(2026, 12, 31, 9, 30, 0, DateTimeKind.Unspecified));
    }

    [Fact]
    public void Rejects_a_moment_in_the_past()
    {
        ScheduleComposer.TryCombine(new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Unspecified), new TimeOnly(9, 0), _now, out _)
            .ShouldBeFalse();
    }

    [Fact]
    public void Rejects_a_time_earlier_today()
    {
        ScheduleComposer.TryCombine(new DateTime(2026, 11, 4, 0, 0, 0, DateTimeKind.Unspecified), new TimeOnly(9, 0), _now, out _)
            .ShouldBeFalse();
    }

    [Fact]
    public void Tomorrow_is_the_next_day_at_the_given_hour()
    {
        ScheduleComposer.Tomorrow(_now, 9).DateTime.ShouldBe(new DateTime(2026, 11, 5, 9, 0, 0, DateTimeKind.Unspecified));
    }

    [Fact]
    public void Next_monday_skips_to_the_following_week()
    {
        ScheduleComposer.NextMonday(_now).DayOfWeek.ShouldBe(DayOfWeek.Monday);
        ScheduleComposer.NextMonday(_now).DateTime.ShouldBe(new DateTime(2026, 11, 9, 9, 0, 0, DateTimeKind.Unspecified));
    }
}
