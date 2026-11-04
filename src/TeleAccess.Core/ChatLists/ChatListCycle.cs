namespace TeleAccess.Core.ChatLists;

public static class ChatListCycle
{
    public static ChatListEntry Next(IReadOnlyList<ChatListEntry> entries, ChatListEntry current)
    {
        ArgumentNullException.ThrowIfNull(entries);

        if (entries.Count == 0)
        {
            throw new ArgumentException("At least one chat list is required.", nameof(entries));
        }

        var index = IndexOf(entries, current);
        return entries[(index + 1) % entries.Count];
    }

    public static int IndexOf(IReadOnlyList<ChatListEntry> entries, ChatListEntry current)
    {
        ArgumentNullException.ThrowIfNull(entries);

        for (var i = 0; i < entries.Count; i++)
        {
            if (entries[i] == current)
            {
                return i;
            }
        }

        return 0;
    }

    public static IReadOnlyList<ChatListEntry> Build(IEnumerable<string> folderTitles)
    {
        ArgumentNullException.ThrowIfNull(folderTitles);

        var entries = new List<ChatListEntry> { new("Main", ChatListKind.Main) };
        var id = 0;

        foreach (var title in folderTitles)
        {
            entries.Add(new ChatListEntry(title, ChatListKind.Folder, id++));
        }

        entries.Add(new ChatListEntry("Archive", ChatListKind.Archive));
        return entries;
    }
}
