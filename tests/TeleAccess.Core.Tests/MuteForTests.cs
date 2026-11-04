using Shouldly;
using TeleAccess.Core.Notifications;

namespace TeleAccess.Core.Tests;

public class MuteForTests
{
    [Fact]
    public void Zero_unmutes()
    {
        MuteFor.TryBuild(0, out var seconds).ShouldBeTrue();
        seconds.ShouldBe(0);
    }

    [Theory]
    [InlineData(1, 3600)]
    [InlineData(8, 28800)]
    [InlineData(168, 604800)]
    [InlineData(8760, 31536000)]
    public void Converts_hours_to_seconds(int hours, int expected)
    {
        MuteFor.TryBuild(hours, out var seconds).ShouldBeTrue();
        seconds.ShouldBe(expected);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(8761)]
    public void Rejects_out_of_range_values(int hours)
    {
        MuteFor.TryBuild(hours, out _).ShouldBeFalse();
    }
}
