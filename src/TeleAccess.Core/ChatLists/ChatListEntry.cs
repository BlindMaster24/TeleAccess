namespace TeleAccess.Core.ChatLists;

public sealed record ChatListEntry(string Title, ChatListKind Kind, int FolderId = 0);
