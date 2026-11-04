namespace TeleAccess.Core.Scheduling;

public static class ScheduleComposer
{
    public static bool TryCombine(DateTime date, TimeOnly time, DateTimeOffset now, out DateTimeOffset result)
    {
        var local = date.Date.Add(time.ToTimeSpan());

        if (local <= now.DateTime)
        {
            result = default;
            return false;
        }

        result = new DateTimeOffset(local, TimeZoneInfo.Local.GetUtcOffset(local));
        return true;
    }

    public static DateTimeOffset NextMonday(DateTimeOffset now)
    {
        var days = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
        if (days == 0)
        {
            days = 7;
        }

        var date = now.Date.AddDays(days);
        return new DateTimeOffset(date.AddHours(9), TimeZoneInfo.Local.GetUtcOffset(date.AddHours(9)));
    }

    public static DateTimeOffset Tomorrow(DateTimeOffset now, int hour)
    {
        var date = now.Date.AddDays(1).AddHours(hour);
        return new DateTimeOffset(date, TimeZoneInfo.Local.GetUtcOffset(date));
    }
}
