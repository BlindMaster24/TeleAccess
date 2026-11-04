using Shouldly;
using TeleAccess.Core.ChatLists;

namespace TeleAccess.Core.Tests;

public class ChatListCycleTests
{
    private static readonly IReadOnlyList<ChatListEntry> _entries = ChatListCycle.Build(["Work", "Family"]);

    [Fact]
    public void Build_starts_with_main_and_ends_with_archive()
    {
        _entries.Count.ShouldBe(4);
        _entries[0].Kind.ShouldBe(ChatListKind.Main);
        _entries[^1].Kind.ShouldBe(ChatListKind.Archive);
        _entries[1].Title.ShouldBe("Work");
        _entries[1].FolderId.ShouldBe(0);
        _entries[2].FolderId.ShouldBe(1);
    }

    [Fact]
    public void Next_moves_forward()
    {
        ChatListCycle.Next(_entries, _entries[0]).ShouldBe(_entries[1]);
        ChatListCycle.Next(_entries, _entries[2]).ShouldBe(_entries[3]);
    }

    [Fact]
    public void Next_wraps_to_main_from_archive()
    {
        ChatListCycle.Next(_entries, _entries[^1]).ShouldBe(_entries[0]);
    }

    [Fact]
    public void Unknown_current_starts_from_the_first_entry()
    {
        var unknown = new ChatListEntry("Other", ChatListKind.Folder, 42);
        ChatListCycle.Next(_entries, unknown).ShouldBe(_entries[1]);
    }

    [Fact]
    public void Empty_list_is_rejected()
    {
        Should.Throw<ArgumentException>(() => ChatListCycle.Next([], _entries[0]));
    }

    [Fact]
    public void Build_without_folders_has_two_entries()
    {
        ChatListCycle.Build([]).Count.ShouldBe(2);
    }
}
