namespace TeleAccess.Core.Topics;

public static class TopicSorter
{
    public static IReadOnlyList<TopicItem> Sort(IEnumerable<TopicItem> topics)
    {
        ArgumentNullException.ThrowIfNull(topics);

        return topics
            .OrderByDescending(topic => topic.IsPinned)
            .ThenByDescending(topic => topic.Order)
            .ThenBy(topic => topic.Id)
            .ToArray();
    }

    public static int TotalUnread(IEnumerable<TopicItem> topics)
    {
        ArgumentNullException.ThrowIfNull(topics);

        return topics.Sum(topic => topic.UnreadCount);
    }
}
