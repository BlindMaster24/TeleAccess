using Shouldly;
using TeleAccess.Core.Topics;

namespace TeleAccess.Core.Tests;

public class TopicSorterTests
{
    private static TopicItem Topic(int id, long order, bool pinned = false, int unread = 0) =>
        new(id, "topic " + id, unread, pinned, order);

    [Fact]
    public void Pinned_topics_come_first()
    {
        var sorted = TopicSorter.Sort([Topic(1, 100), Topic(2, 10, pinned: true)]);

        sorted[0].Id.ShouldBe(2);
        sorted[1].Id.ShouldBe(1);
    }

    [Fact]
    public void Unpinned_topics_follow_the_order_field()
    {
        var sorted = TopicSorter.Sort([Topic(1, 5), Topic(2, 9), Topic(3, 7)]);

        sorted.Select(topic => topic.Id).ShouldBe([2, 3, 1]);
    }

    [Fact]
    public void Ties_are_broken_by_id()
    {
        var sorted = TopicSorter.Sort([Topic(7, 3), Topic(2, 3)]);

        sorted.Select(topic => topic.Id).ShouldBe([2, 7]);
    }

    [Fact]
    public void Totals_unread_counts()
    {
        TopicSorter.TotalUnread([Topic(1, 1, unread: 2), Topic(2, 2, unread: 3)]).ShouldBe(5);
        TopicSorter.TotalUnread([]).ShouldBe(0);
    }

    [Fact]
    public void Display_shows_unread_only_when_positive()
    {
        Topic(1, 1, unread: 0).Display.ShouldBe("topic 1");
        Topic(1, 1, unread: 4).Display.ShouldBe("topic 1 (4)");
    }

    [Fact]
    public void Sorting_requires_a_collection()
    {
        Should.Throw<ArgumentNullException>(() => TopicSorter.Sort(null!));
        Should.Throw<ArgumentNullException>(() => TopicSorter.TotalUnread(null!));
    }
}
