namespace TeleAccess.Core.Notifications;

public static class MuteFor
{
    public const int MaxHours = 8760;

    public static bool TryBuild(int hours, out int seconds)
    {
        seconds = 0;

        if (hours == 0)
        {
            return true;
        }

        if (hours < 0 || hours > MaxHours)
        {
            return false;
        }

        seconds = hours * 3600;
        return true;
    }
}
