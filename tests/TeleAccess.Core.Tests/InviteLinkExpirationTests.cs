using Shouldly;
using TeleAccess.Core.Invites;

namespace TeleAccess.Core.Tests;

public class InviteLinkExpirationTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 4, 12, 0, 0, TimeSpan.FromHours(3));

    [Fact]
    public void Zero_means_no_expiration()
    {
        InviteLinkExpiration.TryBuild(0, _now, out var unix).ShouldBeTrue();
        unix.ShouldBe(0);
    }

    [Theory]
    [InlineData(1, 86400)]
    [InlineData(7, 604800)]
    [InlineData(365, 31536000)]
    public void Converts_days_to_unix_seconds(int days, int expectedSeconds)
    {
        InviteLinkExpiration.TryBuild(days, _now, out var unix).ShouldBeTrue();
        unix.ShouldBe((int)_now.ToUnixTimeSeconds() + expectedSeconds);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(366)]
    public void Rejects_out_of_range_values(int days)
    {
        InviteLinkExpiration.TryBuild(days, _now, out _).ShouldBeFalse();
    }
}
