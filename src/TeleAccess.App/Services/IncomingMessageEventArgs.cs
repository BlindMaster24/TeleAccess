namespace TeleAccess.App.Services;

public sealed class IncomingMessageEventArgs : EventArgs
{
    public IncomingMessageEventArgs(long chatId, string text)
    {
        ChatId = chatId;
        Text = text;
    }

    public long ChatId { get; }

    public string Text { get; }
}
