namespace TeleAccess.Core.Invites;

public static class InviteLinkExpiration
{
    public const int MaxDays = 365;

    public static bool TryBuild(int days, DateTimeOffset now, out int unixSeconds)
    {
        unixSeconds = 0;

        if (days == 0)
        {
            return true;
        }

        if (days < 0 || days > MaxDays)
        {
            return false;
        }

        unixSeconds = (int)now.AddDays(days).ToUnixTimeSeconds();
        return true;
    }
}
