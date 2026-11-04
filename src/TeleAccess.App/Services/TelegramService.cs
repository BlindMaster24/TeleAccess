using System.Diagnostics;
using System.Globalization;
using System.IO;
using TdLib;
using TdLib.Bindings;

namespace TeleAccess.App.Services;

public sealed class TelegramService : IDisposable
{
    private readonly Dictionary<long, string> _userNames = [];
    private AppSettings _settings = new();
    private TdClient? _client;

    public event EventHandler<AuthStageChangedEventArgs>? AuthStageChanged;

    public event EventHandler? ChatsChanged;

    public event EventHandler<ChatMessagesChangedEventArgs>? ChatMessagesChanged;

    public AuthStage Stage { get; private set; } = AuthStage.Starting;

    public void Start(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _settings = settings;

        if (_client is not null)
        {
            return;
        }

        _client = new TdClient();
        _client.Bindings.SetLogVerbosityLevel(TdLogLevel.Fatal);
        _client.UpdateReceived += (_, update) => OnUpdate(update);
    }

    public async Task SubmitPhoneAsync(string phone)
    {
        RequireClient();
        await _client!.ExecuteAsync(new TdApi.SetAuthenticationPhoneNumber
        {
            PhoneNumber = phone,
            Settings = null
        });
    }

    public async Task SubmitCodeAsync(string code)
    {
        RequireClient();
        await _client!.ExecuteAsync(new TdApi.CheckAuthenticationCode { Code = code });
    }

    public async Task SubmitPasswordAsync(string password)
    {
        RequireClient();
        await _client!.ExecuteAsync(new TdApi.CheckAuthenticationPassword { Password = password });
    }

    public async Task<IReadOnlyList<ChatRow>> LoadChatsAsync(int limit = 60, string? query = null)
    {
        RequireClient();

        var ids = new List<long>();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var found = await _client!.ExecuteAsync(new TdApi.SearchChats
            {
                Query = query,
                TypeFilter = null,
                Limit = limit
            });

            ids.AddRange(found.ChatIds);
        }
        else
        {
            await _client!.ExecuteAsync(new TdApi.LoadChats
            {
                ChatList = new TdApi.ChatList.ChatListMain(),
                Limit = limit
            });

            var chats = await _client.ExecuteAsync(new TdApi.GetChats
            {
                ChatList = new TdApi.ChatList.ChatListMain(),
                Limit = limit
            });

            ids.AddRange(chats.ChatIds);
        }

        var rows = new List<ChatRow>(ids.Count);

        foreach (var id in ids)
        {
            var chat = await _client.ExecuteAsync(new TdApi.GetChat { ChatId = id });
            var (kind, text) = PreviewOf(chat);
            rows.Add(new ChatRow(id, chat.Title, kind, text, chat.UnreadCount));
        }

        return rows;
    }

    public async Task<IReadOnlyList<MessageRow>> LoadMessagesAsync(long chatId, int limit = 40)
    {
        RequireClient();

        var history = await _client!.ExecuteAsync(new TdApi.GetChatHistory
        {
            ChatId = chatId,
            FromMessageId = 0,
            Offset = 0,
            Limit = limit,
            OnlyLocal = false
        });

        var rows = new List<MessageRow>(history.Messages_.Length);

        foreach (var message in history.Messages_.Reverse())
        {
            rows.Add(await ToRowAsync(message));
        }

        return rows;
    }

    public async Task SendTextAsync(long chatId, string text, long replyToMessageId = 0)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.SendMessage
        {
            ChatId = chatId,
            TopicId = null,
            ReplyTo = replyToMessageId == 0
                ? null
                : new TdApi.InputMessageReplyTo.InputMessageReplyToMessage
                {
                    MessageId = replyToMessageId,
                    Quote = null,
                    ChecklistTaskId = 0,
                    PollOptionId = string.Empty
                },
            Options = null,
            ReplyMarkup = null,
            InputMessageContent = new TdApi.InputMessageContent.InputMessageText
            {
                Text = new TdApi.FormattedText { Text = text, Entities = [] },
                LinkPreviewOptions = null,
                ClearDraft = true
            }
        });
    }

    public async Task DeleteMessagesAsync(long chatId, IReadOnlyCollection<long> messageIds, bool revoke)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.DeleteMessages
        {
            ChatId = chatId,
            MessageIds = [.. messageIds],
            Revoke = revoke
        });
    }

    public async Task EditMessageAsync(long chatId, long messageId, string text)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.EditMessageText
        {
            ChatId = chatId,
            MessageId = messageId,
            ReplyMarkup = null,
            InputMessageContent = new TdApi.InputMessageContent.InputMessageText
            {
                Text = new TdApi.FormattedText { Text = text, Entities = [] },
                LinkPreviewOptions = null,
                ClearDraft = false
            }
        });
    }

    public async Task ForwardMessageAsync(long fromChatId, long toChatId, long messageId)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.ForwardMessages
        {
            ChatId = toChatId,
            TopicId = null,
            FromChatId = fromChatId,
            MessageIds = [messageId],
            Options = null,
            SendCopy = false,
            RemoveCaption = false
        });
    }

    public async Task PinMessageAsync(long chatId, long messageId, bool pin)
    {
        RequireClient();

        if (pin)
        {
            await _client!.ExecuteAsync(new TdApi.PinChatMessage
            {
                ChatId = chatId,
                MessageId = messageId,
                DisableNotification = true,
                OnlyForSelf = true
            });
            return;
        }

        await _client!.ExecuteAsync(new TdApi.UnpinChatMessage
        {
            ChatId = chatId,
            MessageId = messageId
        });
    }

    public async Task MarkChatReadAsync(long chatId)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.ViewMessages
        {
            ChatId = chatId,
            MessageIds = [],
            Source = null,
            ForceRead = true
        });
    }

    public void Dispose()
    {
        _client?.Dispose();
        _client = null;
    }

    private void OnUpdate(TdApi.Update update)
    {
        switch (update)
        {
            case TdApi.Update.UpdateAuthorizationState auth:
                HandleAuthorization(auth.AuthorizationState);
                break;

            case TdApi.Update.UpdateNewChat:
            case TdApi.Update.UpdateChatTitle:
            case TdApi.Update.UpdateChatLastMessage:
            case TdApi.Update.UpdateChatReadInbox:
            case TdApi.Update.UpdateDeleteMessages:
            case TdApi.Update.UpdateChatPosition:
                ChatsChanged?.Invoke(this, EventArgs.Empty);
                break;

            case TdApi.Update.UpdateNewMessage newMessage:
                ChatMessagesChanged?.Invoke(this, new ChatMessagesChangedEventArgs(newMessage.Message.ChatId));
                ChatsChanged?.Invoke(this, EventArgs.Empty);
                break;

            case TdApi.Update.UpdateMessageEdited edited:
                ChatMessagesChanged?.Invoke(this, new ChatMessagesChangedEventArgs(edited.ChatId));
                break;

            case TdApi.Update.UpdateMessageSendSucceeded succeeded:
                ChatMessagesChanged?.Invoke(this, new ChatMessagesChangedEventArgs(succeeded.Message.ChatId));
                break;

            case TdApi.Update.UpdateMessageSendFailed failed:
                ChatMessagesChanged?.Invoke(this, new ChatMessagesChangedEventArgs(failed.Message.ChatId));
                break;
        }
    }

    private void HandleAuthorization(TdApi.AuthorizationState state)
    {
        switch (state)
        {
            case TdApi.AuthorizationState.AuthorizationStateWaitTdlibParameters:
                SetStage(AuthStage.NeedParameters);
                _ = ApplyParametersAsync();
                break;

            case TdApi.AuthorizationState.AuthorizationStateWaitPhoneNumber:
                SetStage(AuthStage.NeedPhone);
                break;

            case TdApi.AuthorizationState.AuthorizationStateWaitCode:
                SetStage(AuthStage.NeedCode);
                break;

            case TdApi.AuthorizationState.AuthorizationStateWaitPassword:
                SetStage(AuthStage.NeedPassword);
                break;

            case TdApi.AuthorizationState.AuthorizationStateReady:
                SetStage(AuthStage.Ready);
                _ = WarmUpAsync();
                break;

            case TdApi.AuthorizationState.AuthorizationStateClosed:
                SetStage(AuthStage.Closed);
                break;
        }
    }

    private void SetStage(AuthStage stage)
    {
        Stage = stage;
        AuthStageChanged?.Invoke(this, new AuthStageChangedEventArgs(stage));
    }

    private async Task ApplyParametersAsync()
    {
        if (_client is null)
        {
            return;
        }

        var (apiId, apiHash) = TelegramCredentials.Resolve(_settings);
        if (apiId <= 0 || apiHash.Length == 0)
        {
            return;
        }

        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TeleAccess",
            "td");

        Directory.CreateDirectory(directory);

        await _client.ExecuteAsync(new TdApi.SetTdlibParameters
        {
            UseTestDc = false,
            DatabaseDirectory = directory,
            FilesDirectory = directory,
            DatabaseEncryptionKey = [],
            UseFileDatabase = true,
            UseChatInfoDatabase = true,
            UseMessageDatabase = true,
            UseSecretChats = true,
            ApiId = apiId,
            ApiHash = apiHash,
            SystemLanguageCode = _settings.Language,
            DeviceModel = "TeleAccess",
            SystemVersion = Environment.OSVersion.VersionString,
            ApplicationVersion = "0.1"
        });
    }

    private async Task WarmUpAsync()
    {
        try
        {
            await _client!.ExecuteAsync(new TdApi.LoadChats
            {
                ChatList = new TdApi.ChatList.ChatListMain(),
                Limit = 100
            });
        }
        catch (TdException exception)
        {
            Debug.WriteLine(exception);
        }
        catch (InvalidOperationException exception)
        {
            Debug.WriteLine(exception);
        }

        ChatsChanged?.Invoke(this, EventArgs.Empty);
    }

    private static (MessageKind Kind, string Text) PreviewOf(TdApi.Chat chat)
    {
        var content = chat.LastMessage?.Content;

        return content switch
        {
            TdApi.MessageContent.MessageText text => (MessageKind.Text, text.Text.Text),
            TdApi.MessageContent.MessageVoiceNote => (MessageKind.Voice, string.Empty),
            TdApi.MessageContent.MessagePhoto => (MessageKind.Photo, string.Empty),
            TdApi.MessageContent.MessageVideo => (MessageKind.Video, string.Empty),
            TdApi.MessageContent.MessageDocument document => (MessageKind.Document, document.Document.FileName),
            TdApi.MessageContent.MessageSticker => (MessageKind.Sticker, string.Empty),
            TdApi.MessageContent.MessageAnimation => (MessageKind.Animation, string.Empty),
            TdApi.MessageContent.MessageAudio => (MessageKind.Audio, string.Empty),
            null => (MessageKind.Other, string.Empty),
            _ => (MessageKind.Other, string.Empty)
        };
    }

    private async Task<MessageRow> ToRowAsync(TdApi.Message message)
    {
        var (kind, text) = Describe(message.Content);
        var sender = message.IsOutgoing ? string.Empty : await ResolveSenderAsync(message.SenderId);
        var time = DateTimeOffset.FromUnixTimeSeconds(message.Date).ToLocalTime().ToString("HH:mm", CultureInfo.InvariantCulture);

        return new MessageRow(message.Id, message.ChatId, sender, kind, text, time, message.IsOutgoing);
    }

    private static (MessageKind Kind, string Text) Describe(TdApi.MessageContent content) => content switch
    {
        TdApi.MessageContent.MessageText text => (MessageKind.Text, text.Text.Text),
        TdApi.MessageContent.MessageVoiceNote => (MessageKind.Voice, string.Empty),
        TdApi.MessageContent.MessagePhoto => (MessageKind.Photo, string.Empty),
        TdApi.MessageContent.MessageVideo => (MessageKind.Video, string.Empty),
        TdApi.MessageContent.MessageDocument document => (MessageKind.Document, document.Document.FileName),
        TdApi.MessageContent.MessageSticker => (MessageKind.Sticker, string.Empty),
        TdApi.MessageContent.MessageAnimation => (MessageKind.Animation, string.Empty),
        TdApi.MessageContent.MessageAudio => (MessageKind.Audio, string.Empty),
        _ => (MessageKind.Other, string.Empty)
    };

    private async Task<string> ResolveSenderAsync(TdApi.MessageSender sender)
    {
        if (sender is not TdApi.MessageSender.MessageSenderUser user)
        {
            return string.Empty;
        }

        if (_userNames.TryGetValue(user.UserId, out var cached))
        {
            return cached;
        }

        try
        {
            var found = await _client!.ExecuteAsync(new TdApi.GetUser { UserId = user.UserId });
            var name = $"{found.FirstName} {found.LastName}".Trim();
            var label = found.Usernames?.ActiveUsernames.FirstOrDefault();
            var result = string.IsNullOrEmpty(name) ? label ?? string.Empty : name;
            _userNames[user.UserId] = result;
            return result;
        }
        catch (TdException exception)
        {
            Debug.WriteLine(exception);
            return string.Empty;
        }
    }

    private void RequireClient()
    {
        if (_client is null)
        {
            throw new InvalidOperationException("TelegramService.Start must be called first.");
        }
    }
}
