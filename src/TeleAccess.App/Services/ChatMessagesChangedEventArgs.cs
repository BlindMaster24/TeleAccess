namespace TeleAccess.App.Services;

public sealed class ChatMessagesChangedEventArgs : EventArgs
{
    public ChatMessagesChangedEventArgs(long chatId)
    {
        ChatId = chatId;
    }

    public long ChatId { get; }
}
