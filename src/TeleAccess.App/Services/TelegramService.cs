using System.Diagnostics;
using System.Globalization;
using System.IO;
using TdLib;
using TdLib.Bindings;
using TeleAccess.Core.ChatLists;
using TeleAccess.Core.Drafts;
using TeleAccess.Core.Formatting;
using TeleAccess.Core.Invites;
using TeleAccess.Core.Notifications;
using TeleAccess.Core.Search;

namespace TeleAccess.App.Services;

public sealed class TelegramService : IDisposable
{
    private readonly Dictionary<long, string> _userNames = [];
    private const int MessageTextLimit = 4096;

    private readonly string _dataDirectory;
    private AppSettings _settings = new();
    private TdClient? _client;

    public event EventHandler<AuthStageChangedEventArgs>? AuthStageChanged;

    public event EventHandler? ChatsChanged;

    public event EventHandler<ChatMessagesChangedEventArgs>? ChatMessagesChanged;

    public event EventHandler? FoldersChanged;

    public event EventHandler<IncomingMessageEventArgs>? MessageArrived;

    public IReadOnlyList<FolderRow> Folders { get; private set; } = [];

    public AuthStage Stage { get; private set; } = AuthStage.Starting;

    public TelegramService(string? dataDirectory = null)
    {
        _dataDirectory = string.IsNullOrWhiteSpace(dataDirectory)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TeleAccess",
                "td")
            : dataDirectory;
    }

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

    public async Task<IReadOnlyList<ChatRow>> LoadChatsAsync(int limit = 60, string? query = null, ChatListEntry? list = null)
    {
        RequireClient();

        var chatList = ToChatList(list);

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
                ChatList = chatList,
                Limit = limit
            });

            var chats = await _client.ExecuteAsync(new TdApi.GetChats
            {
                ChatList = chatList,
                Limit = limit
            });

            ids.AddRange(chats.ChatIds);
        }

        var rows = new List<ChatRow>(ids.Count);

        foreach (var id in ids)
        {
            var chat = await _client.ExecuteAsync(new TdApi.GetChat { ChatId = id });
            var (kind, text) = PreviewOf(chat);
            rows.Add(new ChatRow(
                id,
                chat.Title,
                kind,
                text,
                chat.UnreadCount,
                chat.NotificationSettings.MuteFor > 0 || chat.DefaultDisableNotification,
                chat.Positions.Any(position => position.IsPinned),
                chat.ChatLists.Any(list => list is TdApi.ChatList.ChatListArchive),
                chat.ViewAsTopics));
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

    public async Task SendTextAsync(
        long chatId,
        string text,
        long replyToMessageId = 0,
        SendOptions? options = null,
        int topicId = 0)
    {
        RequireClient();

        var markup = SimpleMarkup.Parse(text);

        await SendAsync(chatId, new TdApi.InputMessageContent.InputMessageText
        {
            Text = new TdApi.FormattedText
            {
                Text = markup.Text,
                Entities = markup.Entities.Select(ToEntity).ToArray()
            },
            LinkPreviewOptions = null,
            ClearDraft = true
        }, replyToMessageId, options, topicId);
    }

    private static TdApi.TextEntity ToEntity(EntitySpan span) => span.Kind switch
    {
        EntityKind.Bold => new TdApi.TextEntity
        {
            Offset = span.Offset,
            Length = span.Length,
            Type = new TdApi.TextEntityType.TextEntityTypeBold()
        },
        EntityKind.Italic => new TdApi.TextEntity
        {
            Offset = span.Offset,
            Length = span.Length,
            Type = new TdApi.TextEntityType.TextEntityTypeItalic()
        },
        _ => new TdApi.TextEntity
        {
            Offset = span.Offset,
            Length = span.Length,
            Type = new TdApi.TextEntityType.TextEntityTypeCode()
        }
    };

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

    public Task RetryParametersAsync() => ApplyParametersAsync();

    public async Task OpenChatAsync(long chatId)
    {
        RequireClient();
        await _client!.ExecuteAsync(new TdApi.OpenChat { ChatId = chatId });
    }

    public async Task DeleteChatAsync(long chatId)
    {
        RequireClient();
        await _client!.ExecuteAsync(new TdApi.DeleteChat { ChatId = chatId });
    }

    public async Task ToggleMarkedUnreadAsync(long chatId, bool unread)
    {
        RequireClient();
        await _client!.ExecuteAsync(new TdApi.ToggleChatIsMarkedAsUnread
        {
            ChatId = chatId,
            IsMarkedAsUnread = unread
        });
    }

    public async Task<string> GetOwnDisplayNameAsync()
    {
        RequireClient();

        var me = await _client!.ExecuteAsync(new TdApi.GetMe());
        var name = $"{me.FirstName} {me.LastName}".Trim();
        return string.IsNullOrEmpty(name) ? me.Usernames?.ActiveUsernames.FirstOrDefault() ?? string.Empty : name;
    }

    public async Task SendAlbumAsync(
        long chatId,
        IReadOnlyList<AlbumItem> items,
        string caption,
        long replyToMessageId = 0,
        int topicId = 0)
    {
        RequireClient();
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
        {
            return;
        }

        var contents = items
            .Select((item, index) => ToAlbumContent(item, index == 0 ? caption : string.Empty))
            .ToArray();

        await _client!.ExecuteAsync(new TdApi.SendMessageAlbum
        {
            ChatId = chatId,
            TopicId = topicId == 0
                ? null
                : new TdApi.MessageTopic.MessageTopicForum { ForumTopicId = topicId },
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
            InputMessageContents = contents
        });
    }

    private static TdApi.InputMessageContent ToAlbumContent(AlbumItem item, string caption) =>
        item.IsPhoto
                ? new TdApi.InputMessageContent.InputMessagePhoto
                {
                    Photo = new TdApi.InputPhoto
                    {
                        Photo = new TdApi.InputFile.InputFileLocal { Path = item.Path },
                        Thumbnail = null,
                        Video = null,
                        AddedStickerFileIds = [],
                        Width = item.Width,
                        Height = item.Height
                    },
                    Caption = new TdApi.FormattedText { Text = caption, Entities = [] },
                    ShowCaptionAboveMedia = false,
                    SelfDestructType = null,
                    HasSpoiler = false
                }
                : new TdApi.InputMessageContent.InputMessageDocument
                {
                    Document = new TdApi.InputDocument
                    {
                        Document = new TdApi.InputFile.InputFileLocal { Path = item.Path },
                        Thumbnail = null,
                        DisableContentTypeDetection = false
                    },
                    Caption = new TdApi.FormattedText { Text = caption, Entities = [] }
                };

    public async Task SendDocumentAsync(
        long chatId,
        string path,
        string caption,
        long replyToMessageId = 0,
        SendOptions? options = null)
    {
        RequireClient();
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        await SendAsync(chatId, new TdApi.InputMessageContent.InputMessageDocument
        {
            Document = new TdApi.InputDocument
            {
                Document = new TdApi.InputFile.InputFileLocal { Path = path },
                Thumbnail = null,
                DisableContentTypeDetection = false
            },
            Caption = new TdApi.FormattedText { Text = caption, Entities = [] }
        }, replyToMessageId, options);
    }

    private async Task SendAsync(
        long chatId,
        TdApi.InputMessageContent content,
        long replyToMessageId,
        SendOptions? options,
        int topicId = 0)
    {
        options ??= SendOptions.Normal;

        TdApi.MessageSchedulingState? scheduling = null;

        if (options.SendAt is { } at)
        {
            scheduling = new TdApi.MessageSchedulingState.MessageSchedulingStateSendAtDate
            {
                SendDate = (int)at.ToUnixTimeSeconds(),
                RepeatPeriod = 0
            };
        }
        else if (options.WhenOnline)
        {
            scheduling = new TdApi.MessageSchedulingState.MessageSchedulingStateSendWhenOnline();
        }

        await _client!.ExecuteAsync(new TdApi.SendMessage
        {
            ChatId = chatId,
            TopicId = topicId == 0
                ? null
                : new TdApi.MessageTopic.MessageTopicForum { ForumTopicId = topicId },
            ReplyTo = replyToMessageId == 0
                ? null
                : new TdApi.InputMessageReplyTo.InputMessageReplyToMessage
                {
                    MessageId = replyToMessageId,
                    Quote = null,
                    ChecklistTaskId = 0,
                    PollOptionId = string.Empty
                },
            Options = new TdApi.MessageSendOptions
            {
                DisableNotification = options.Silent,
                SchedulingState = scheduling
            },
            ReplyMarkup = null,
            InputMessageContent = content
        });
    }

    public async Task SendVoiceAsync(
        long chatId,
        string path,
        int durationSeconds,
        long replyToMessageId = 0,
        SendOptions? options = null)
    {
        RequireClient();
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        await SendAsync(chatId, new TdApi.InputMessageContent.InputMessageVoiceNote
        {
            VoiceNote = new TdApi.InputVoiceNote
            {
                VoiceNote = new TdApi.InputFile.InputFileLocal { Path = path },
                Duration = durationSeconds,
                Waveform = []
            },
            Caption = new TdApi.FormattedText { Text = string.Empty, Entities = [] },
            SelfDestructType = null
        }, replyToMessageId, options);
    }

    public async Task SendAudioAsync(
        long chatId,
        string path,
        int durationSeconds,
        string title,
        string performer,
        long replyToMessageId = 0,
        SendOptions? options = null)
    {
        RequireClient();
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        await SendAsync(chatId, new TdApi.InputMessageContent.InputMessageAudio
        {
            Audio = new TdApi.InputAudio
            {
                Audio = new TdApi.InputFile.InputFileLocal { Path = path },
                AlbumCoverThumbnail = null,
                Duration = durationSeconds,
                Title = title,
                Performer = performer
            },
            Caption = new TdApi.FormattedText { Text = string.Empty, Entities = [] }
        }, replyToMessageId, options);
    }

    public async Task SendPhotoAsync(
        long chatId,
        string path,
        string caption,
        int width,
        int height,
        long replyToMessageId = 0)
    {
        RequireClient();

        await SendAsync(chatId, new TdApi.InputMessageContent.InputMessagePhoto
        {
            Photo = new TdApi.InputPhoto
            {
                Photo = new TdApi.InputFile.InputFileLocal { Path = path },
                Thumbnail = null,
                Width = width,
                Height = height
            },
            Caption = new TdApi.FormattedText { Text = caption, Entities = [] },
            ShowCaptionAboveMedia = false,
            SelfDestructType = null,
            HasSpoiler = false
        }, replyToMessageId, null);
    }

    public async Task SendContactAsync(
        long chatId,
        string phoneNumber,
        string firstName,
        string lastName,
        long replyToMessageId = 0)
    {
        RequireClient();

        await SendAsync(chatId, new TdApi.InputMessageContent.InputMessageContact
        {
            Contact = new TdApi.Contact
            {
                PhoneNumber = phoneNumber,
                FirstName = firstName,
                LastName = lastName,
                Vcard = string.Empty,
                UserId = 0
            }
        }, replyToMessageId, null);
    }

    public async Task SendLocationAsync(long chatId, double latitude, double longitude, long replyToMessageId = 0)
    {
        RequireClient();

        await SendAsync(chatId, new TdApi.InputMessageContent.InputMessageLocation
        {
            Location = new TdApi.Location
            {
                Latitude = latitude,
                Longitude = longitude,
                HorizontalAccuracy = 0
            }
        }, replyToMessageId, null);
    }

    public async Task SendChatActionAsync(long chatId, bool typing)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.SendChatAction
        {
            ChatId = chatId,
            TopicId = null,
            BusinessConnectionId = string.Empty,
            Action = typing
                ? new TdApi.ChatAction.ChatActionTyping()
                : new TdApi.ChatAction.ChatActionCancel()
        });
    }

    public async Task<string> TranslateAsync(string text, string languageCode)
    {
        RequireClient();
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);

        var translated = await _client!.ExecuteAsync(new TdApi.TranslateText
        {
            Text = new TdApi.FormattedText { Text = text, Entities = [] },
            ToLanguageCode = languageCode,
            Tone = null
        });

        return translated.Text;
    }

    public async Task SendVideoAsync(
        long chatId,
        string path,
        string caption,
        int width,
        int height,
        int duration,
        long replyToMessageId = 0)
    {
        RequireClient();

        await SendAsync(chatId, new TdApi.InputMessageContent.InputMessageVideo
        {
            Video = new TdApi.InputVideo
            {
                Video = new TdApi.InputFile.InputFileLocal { Path = path },
                Thumbnail = null,
                Cover = null,
                StartTimestamp = 0,
                AddedStickerFileIds = [],
                Duration = duration,
                Width = width,
                Height = height,
                SupportsStreaming = true
            },
            Caption = new TdApi.FormattedText { Text = caption, Entities = [] },
            ShowCaptionAboveMedia = false,
            SelfDestructType = null,
            HasSpoiler = false
        }, replyToMessageId, null);
    }

    public async Task SendAnimationAsync(
        long chatId,
        string path,
        string caption,
        int width,
        int height,
        int duration,
        long replyToMessageId = 0)
    {
        RequireClient();

        await SendAsync(chatId, new TdApi.InputMessageContent.InputMessageAnimation
        {
            Animation = new TdApi.InputAnimation
            {
                Animation = new TdApi.InputFile.InputFileLocal { Path = path },
                Thumbnail = null,
                AddedStickerFileIds = [],
                Duration = duration,
                Width = width,
                Height = height
            },
            Caption = new TdApi.FormattedText { Text = caption, Entities = [] },
            ShowCaptionAboveMedia = false,
            HasSpoiler = false
        }, replyToMessageId, null);
    }

    public async Task SendStickerAsync(long chatId, string path, string emoji, long replyToMessageId = 0)
    {
        RequireClient();

        await SendAsync(chatId, new TdApi.InputMessageContent.InputMessageSticker
        {
            Sticker = new TdApi.InputSticker
            {
                Sticker = new TdApi.InputFile.InputFileLocal { Path = path },
                Thumbnail = null,
                Width = 512,
                Height = 512
            },
            Emoji = emoji
        }, replyToMessageId, null);
    }

    public async Task SendPollAsync(long chatId, string question, IReadOnlyList<string> options, long replyToMessageId = 0)
    {
        RequireClient();
        ArgumentNullException.ThrowIfNull(options);

        var pollOptions = options
            .Where(option => !string.IsNullOrWhiteSpace(option))
            .Select(option => new TdApi.InputPollOption
            {
                Text = new TdApi.FormattedText { Text = option.Trim(), Entities = [] },
                Media = null
            })
            .ToArray();

        await SendAsync(chatId, new TdApi.InputMessageContent.InputMessagePoll
        {
            Question = new TdApi.FormattedText { Text = question, Entities = [] },
            Options = pollOptions,
            Description = new TdApi.FormattedText { Text = string.Empty, Entities = [] },
            Media = null,
            IsAnonymous = true,
            AllowsMultipleAnswers = false,
            AllowsRevoting = true,
            MembersOnly = false,
            CountryCodes = [],
            ShuffleOptions = false,
            HideResultsUntilCloses = false,
            Type = new TdApi.InputPollType.InputPollTypeRegular { AllowAddingOptions = false }
        }, replyToMessageId, null);
    }

    public async Task SendVenueAsync(
        long chatId,
        string title,
        string address,
        double latitude,
        double longitude,
        long replyToMessageId = 0)
    {
        RequireClient();

        await SendAsync(chatId, new TdApi.InputMessageContent.InputMessageVenue
        {
            Venue = new TdApi.Venue
            {
                Location = new TdApi.Location
                {
                    Latitude = latitude,
                    Longitude = longitude,
                    HorizontalAccuracy = 0
                },
                Title = title,
                Address = address,
                Provider = string.Empty,
                Id = string.Empty,
                Type = string.Empty
            }
        }, replyToMessageId, null);
    }

    public async Task<ChatInfoRow> GetChatInfoAsync(long chatId)
    {
        RequireClient();

        var chat = await _client!.ExecuteAsync(new TdApi.GetChat { ChatId = chatId });
        var members = 0;
        var description = string.Empty;

        switch (chat.Type)
        {
            case TdApi.ChatType.ChatTypeBasicGroup basic:
                var group = await _client.ExecuteAsync(new TdApi.GetBasicGroup { BasicGroupId = basic.BasicGroupId });
                members = group.MemberCount;
                break;

            case TdApi.ChatType.ChatTypeSupergroup supergroup:
                var info = await _client.ExecuteAsync(new TdApi.GetSupergroup { SupergroupId = supergroup.SupergroupId });
                members = info.MemberCount;
                var full = await _client.ExecuteAsync(new TdApi.GetSupergroupFullInfo { SupergroupId = supergroup.SupergroupId });
                description = full.Description;
                break;

            default:
                break;
        }

        var kind = chat.Type switch
        {
            TdApi.ChatType.ChatTypePrivate => "private",
            TdApi.ChatType.ChatTypeBasicGroup => "group",
            TdApi.ChatType.ChatTypeSupergroup super => super.IsChannel ? "channel" : "supergroup",
            TdApi.ChatType.ChatTypeSecret => "secret",
            _ => "chat"
        };

        return new ChatInfoRow(chat.Title, kind, members, description);
    }

    public async Task<IReadOnlyList<string>> GetMembersAsync(long chatId, int limit = 50)
    {
        RequireClient();

        var chat = await _client!.ExecuteAsync(new TdApi.GetChat { ChatId = chatId });
        TdApi.ChatMember[] members;

        switch (chat.Type)
        {
            case TdApi.ChatType.ChatTypeSupergroup supergroup:
                var found = await _client.ExecuteAsync(new TdApi.GetSupergroupMembers
                {
                    SupergroupId = supergroup.SupergroupId,
                    Filter = new TdApi.SupergroupMembersFilter.SupergroupMembersFilterRecent(),
                    Offset = 0,
                    Limit = limit
                });
                members = found.Members;
                break;

            case TdApi.ChatType.ChatTypeBasicGroup basic:
                var full = await _client.ExecuteAsync(new TdApi.GetBasicGroupFullInfo { BasicGroupId = basic.BasicGroupId });
                members = [.. full.Members.Take(limit)];
                break;

            default:
                return [];
        }

        return await Task.WhenAll(members.Select(ResolveMemberNameAsync));
    }

    private async Task<string> ResolveMemberNameAsync(TdApi.ChatMember member)
    {
        var name = await ResolveSenderAsync(member.MemberId);
        return string.IsNullOrEmpty(name) ? member.MemberId.DataType : name;
    }

    public async Task<IReadOnlyList<string>> GetContactsAsync(int limit = 100)
    {
        RequireClient();

        var users = await _client!.ExecuteAsync(new TdApi.GetContacts());
        var names = new List<string>(Math.Min(users.UserIds.Length, limit));

        foreach (var id in users.UserIds.Take(limit))
        {
            var user = await _client.ExecuteAsync(new TdApi.GetUser { UserId = id });
            var name = $"{user.FirstName} {user.LastName}".Trim();
            names.Add(string.IsNullOrEmpty(name) ? user.Usernames?.ActiveUsernames.FirstOrDefault() ?? id.ToString(CultureInfo.InvariantCulture) : name);
        }

        return names;
    }

    public async Task<IReadOnlyList<string>> GetForumTopicsAsync(long chatId, int limit = 50)
    {
        RequireClient();

        var topics = await _client!.ExecuteAsync(new TdApi.GetForumTopics
        {
            ChatId = chatId,
            Query = string.Empty,
            OffsetDate = 0,
            OffsetMessageId = 0,
            OffsetForumTopicId = 0,
            Limit = limit
        });

        return topics.Topics.Select(topic => topic.Info.Name).ToArray();
    }

    public async Task<long> CreateSecretChatAsync(string username)
    {
        RequireClient();
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        var found = await _client!.ExecuteAsync(new TdApi.SearchContacts { Query = username, Limit = 1 });

        if (found.UserIds.Length == 0)
        {
            return 0;
        }

        var chat = await _client.ExecuteAsync(new TdApi.CreateNewSecretChat { UserId = found.UserIds[0] });
        return chat.Id;
    }

    public async Task JoinChatAsync(long chatId)
    {
        RequireClient();
        await _client!.ExecuteAsync(new TdApi.JoinChat { ChatId = chatId });
    }

    public async Task LogOutAsync()
    {
        RequireClient();
        await _client!.ExecuteAsync(new TdApi.LogOut());
    }

    public async Task<string> DownloadAsync(int fileId)
    {
        RequireClient();

        if (fileId == 0)
        {
            return string.Empty;
        }

        var file = await _client!.ExecuteAsync(new TdApi.DownloadFile
        {
            FileId = fileId,
            Priority = 32,
            Offset = 0,
            Limit = 0,
            Synchronous = true
        });

        return file.Local?.Path ?? string.Empty;
    }

    public async Task<IReadOnlyList<MessageRow>> LoadOlderAsync(long chatId, long fromMessageId, int limit = 40)
    {
        RequireClient();

        var history = await _client!.ExecuteAsync(new TdApi.GetChatHistory
        {
            ChatId = chatId,
            FromMessageId = fromMessageId,
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

    public async Task<MessageRow?> GetByDateAsync(long chatId, DateTimeOffset date)
    {
        RequireClient();

        var message = await _client!.ExecuteAsync(new TdApi.GetChatMessageByDate
        {
            ChatId = chatId,
            Date = (int)date.ToUnixTimeSeconds()
        });

        return message.Id == 0 ? null : await ToRowAsync(message);
    }

    public async Task SetNameAsync(string firstName, string lastName)
    {
        RequireClient();
        await _client!.ExecuteAsync(new TdApi.SetName { FirstName = firstName, LastName = lastName });
    }

    public async Task SetUsernameAsync(string username)
    {
        RequireClient();
        await _client!.ExecuteAsync(new TdApi.SetUsername { Username = username });
    }

    public async Task SetBirthdateAsync(int day, int month, int year)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.SetBirthdate
        {
            Birthdate = new TdApi.Birthdate { Day = day, Month = month, Year = year }
        });
    }

    public async Task SetBioAsync(string bio)
    {
        RequireClient();
        await _client!.ExecuteAsync(new TdApi.SetBio { Bio = bio });
    }

    public async Task SetProxyAsync(string host, int port, string username, string password)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.AddProxy
        {
            Proxy = new TdApi.Proxy
            {
                Server = host,
                Port = port,
                Type = new TdApi.ProxyType.ProxyTypeSocks5
                {
                    Username = username,
                    Password = password
                }
            },
            Enable = true,
            Comment = "TeleAccess"
        });
    }

    public async Task<IReadOnlyList<string>> GetButtonsAsync(long chatId, long messageId)
    {
        RequireClient();

        var message = await _client!.ExecuteAsync(new TdApi.GetMessage { ChatId = chatId, MessageId = messageId });

        if (message.ReplyMarkup is not TdApi.ReplyMarkup.ReplyMarkupInlineKeyboard keyboard)
        {
            return [];
        }

        return keyboard.Rows.SelectMany(row => row)
            .Select(button => string.IsNullOrEmpty(button.Text) ? button.Type.DataType : button.Text)
            .ToArray();
    }

    public async Task PressButtonAsync(long chatId, long messageId, int index)
    {
        RequireClient();

        var message = await _client!.ExecuteAsync(new TdApi.GetMessage { ChatId = chatId, MessageId = messageId });

        if (message.ReplyMarkup is not TdApi.ReplyMarkup.ReplyMarkupInlineKeyboard keyboard)
        {
            return;
        }

        var buttons = keyboard.Rows.SelectMany(row => row).ToArray();

        if (index < 0 || index >= buttons.Length)
        {
            return;
        }

        if (buttons[index].Type is TdApi.InlineKeyboardButtonType.InlineKeyboardButtonTypeCallback callback)
        {
            await _client.ExecuteAsync(new TdApi.GetCallbackQueryAnswer
            {
                ChatId = chatId,
                MessageId = messageId,
                Payload = new TdApi.CallbackQueryPayload.CallbackQueryPayloadData { Data = callback.Data }
            });
        }
    }

    public async Task SetChatTitleAsync(long chatId, string title)
    {
        RequireClient();
        await _client!.ExecuteAsync(new TdApi.SetChatTitle { ChatId = chatId, Title = title });
    }

    public async Task<string> CreateInviteLinkAsync(long chatId, int expirationDays)
    {
        RequireClient();

        _ = InviteLinkExpiration.TryBuild(expirationDays, DateTimeOffset.Now, out var expiration);

        var link = await _client!.ExecuteAsync(new TdApi.CreateChatInviteLink
        {
            ChatId = chatId,
            Name = string.Empty,
            ExpirationDate = expiration,
            MemberLimit = 0,
            CreatesJoinRequest = false
        });

        return link.InviteLink;
    }

    public async Task<IReadOnlyList<string>> GetInviteLinksAsync(long chatId, int limit = 20)
    {
        RequireClient();

        var links = await _client!.ExecuteAsync(new TdApi.GetChatInviteLinks
        {
            ChatId = chatId,
            CreatorUserId = 0,
            IsRevoked = false,
            OffsetDate = 0,
            OffsetInviteLink = string.Empty,
            Limit = limit
        });

        return links.InviteLinks.Select(link => link.InviteLink).ToArray();
    }

    public async Task SetScopeMutedAsync(bool privateChats, bool groups, bool channels, int muteHours)
    {
        RequireClient();

        _ = MuteFor.TryBuild(muteHours, out var seconds);

        if (privateChats)
        {
            await ApplyScopeAsync(new TdApi.NotificationSettingsScope.NotificationSettingsScopePrivateChats(), seconds);
        }

        if (groups)
        {
            await ApplyScopeAsync(new TdApi.NotificationSettingsScope.NotificationSettingsScopeGroupChats(), seconds);
        }

        if (channels)
        {
            await ApplyScopeAsync(new TdApi.NotificationSettingsScope.NotificationSettingsScopeChannelChats(), seconds);
        }
    }

    public async Task<IReadOnlyList<string>> GetActiveStoriesAsync(long chatId)
    {
        RequireClient();

        var stories = await _client!.ExecuteAsync(new TdApi.GetChatActiveStories { ChatId = chatId });

        return stories.Stories
            .Select(story => DateTimeOffset.FromUnixTimeSeconds(story.Date).ToLocalTime().ToString("g", CultureInfo.CurrentCulture))
            .ToArray();
    }

    private async Task ApplyScopeAsync(TdApi.NotificationSettingsScope scope, int seconds)
    {
        var current = await _client!.ExecuteAsync(new TdApi.GetScopeNotificationSettings { Scope = scope });

        await _client.ExecuteAsync(new TdApi.SetScopeNotificationSettings
        {
            Scope = scope,
            NotificationSettings = new TdApi.ScopeNotificationSettings
            {
                MuteFor = seconds,
                SoundId = current.SoundId,
                ShowPreview = current.ShowPreview,
                UseDefaultMuteStories = current.UseDefaultMuteStories,
                MuteStories = current.MuteStories,
                StorySoundId = current.StorySoundId,
                ShowStoryPoster = current.ShowStoryPoster,
                DisablePinnedMessageNotifications = current.DisablePinnedMessageNotifications,
                DisableMentionNotifications = current.DisableMentionNotifications
            }
        });
    }

    public async Task<IReadOnlyList<MemberRow>> GetMemberRowsAsync(long chatId, int limit = 50)
    {
        RequireClient();

        var chat = await _client!.ExecuteAsync(new TdApi.GetChat { ChatId = chatId });
        TdApi.ChatMember[] members;

        switch (chat.Type)
        {
            case TdApi.ChatType.ChatTypeSupergroup supergroup:
                var found = await _client.ExecuteAsync(new TdApi.GetSupergroupMembers
                {
                    SupergroupId = supergroup.SupergroupId,
                    Filter = new TdApi.SupergroupMembersFilter.SupergroupMembersFilterRecent(),
                    Offset = 0,
                    Limit = limit
                });
                members = found.Members;
                break;

            case TdApi.ChatType.ChatTypeBasicGroup basic:
                var full = await _client.ExecuteAsync(new TdApi.GetBasicGroupFullInfo { BasicGroupId = basic.BasicGroupId });
                members = [.. full.Members.Take(limit)];
                break;

            default:
                return [];
        }

        return await Task.WhenAll(members.Select(ToMemberRowAsync));
    }

    private async Task<MemberRow> ToMemberRowAsync(TdApi.ChatMember member) =>
        new(member.MemberId, await ResolveSenderAsync(member.MemberId));

    public async Task SetMemberBannedAsync(long chatId, TdApi.MessageSender member, bool banned)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.SetChatMemberStatus
        {
            ChatId = chatId,
            MemberId = member,
            Status = banned
                ? new TdApi.ChatMemberStatus.ChatMemberStatusBanned { BannedUntilDate = 0 }
                : new TdApi.ChatMemberStatus.ChatMemberStatusMember()
        });
    }

    public async Task<int> CreateFolderAsync(string name)
    {
        RequireClient();

        var folder = await _client!.ExecuteAsync(new TdApi.CreateChatFolder
        {
            Folder = new TdApi.ChatFolder
            {
                Name = new TdApi.ChatFolderName
                {
                    Text = new TdApi.FormattedText { Text = name, Entities = [] },
                    AnimateCustomEmoji = false
                },
                Icon = new TdApi.ChatFolderIcon { Name = "Custom" },
                ColorId = 0,
                IsShareable = false,
                PinnedChatIds = [],
                IncludedChatIds = [],
                ExcludedChatIds = [],
                ExcludeMuted = false,
                ExcludeRead = false,
                ExcludeArchived = false,
                IncludeContacts = false,
                IncludeNonContacts = false,
                IncludeBots = false,
                IncludeGroups = false,
                IncludeChannels = false
            }
        });

        return folder.Id;
    }

    public async Task DeleteFolderAsync(int folderId)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.DeleteChatFolder
        {
            ChatFolderId = folderId,
            LeaveChatIds = []
        });
    }

    public async Task<(string? Text, long ReplyToMessageId)> GetDraftAsync(long chatId)
    {
        RequireClient();

        var chat = await _client!.ExecuteAsync(new TdApi.GetChat { ChatId = chatId });
        var draft = chat.DraftMessage;

        var text = draft?.Content is TdApi.DraftMessageContent.DraftMessageContentText content
            ? content.Text.Text
            : null;

        var replyTo = draft?.ReplyTo is TdApi.InputMessageReplyTo.InputMessageReplyToMessage message
            ? message.MessageId
            : 0;

        return (text, replyTo);
    }

    public async Task SetDraftAsync(long chatId, string? text, long replyToMessageId = 0)
    {
        RequireClient();

        var prepared = DraftPolicy.Normalize(text);
        var limited = prepared is null ? null : DraftPolicy.Limit(prepared, MessageTextLimit);

        TdApi.DraftMessage? draft = null;

        if (limited is not null || replyToMessageId != 0)
        {
            draft = new TdApi.DraftMessage
            {
                ReplyTo = replyToMessageId == 0
                    ? null
                    : new TdApi.InputMessageReplyTo.InputMessageReplyToMessage
                    {
                        MessageId = replyToMessageId,
                        Quote = null,
                        ChecklistTaskId = 0,
                        PollOptionId = string.Empty
                    },
                Date = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Content = new TdApi.DraftMessageContent.DraftMessageContentText
                {
                    Text = new TdApi.FormattedText { Text = limited ?? string.Empty, Entities = [] },
                    LinkPreviewOptions = null
                },
                EffectId = 0,
                SuggestedPostInfo = null
            };
        }

        await _client!.ExecuteAsync(new TdApi.SetChatDraftMessage
        {
            ChatId = chatId,
            TopicId = null,
            DraftMessage = draft
        });
    }

    public async Task<IReadOnlyList<TopicRow>> LoadTopicsAsync(long chatId, int limit = 100)
    {
        RequireClient();

        var topics = await _client!.ExecuteAsync(new TdApi.GetForumTopics
        {
            ChatId = chatId,
            Query = string.Empty,
            OffsetDate = 0,
            OffsetMessageId = 0,
            OffsetForumTopicId = 0,
            Limit = limit
        });

        return topics.Topics
            .Select(topic => new TopicRow(
                topic.Info.ForumTopicId,
                topic.Info.Name,
                topic.UnreadCount,
                topic.IsPinned,
                topic.Info.IsClosed,
                topic.Order,
                topic.LastMessage?.Id ?? 0))
            .ToArray();
    }

    public async Task<IReadOnlyList<MessageRow>> LoadTopicMessagesAsync(long chatId, int topicId, int limit = 40)
    {
        RequireClient();

        if (topicId == 0)
        {
            return [];
        }

        var history = await _client!.ExecuteAsync(new TdApi.GetForumTopicHistory
        {
            ChatId = chatId,
            ForumTopicId = topicId,
            FromMessageId = 0,
            Offset = 0,
            Limit = limit
        });

        var rows = new List<MessageRow>(history.Messages_.Length);

        foreach (var message in history.Messages_.Reverse())
        {
            rows.Add(await ToRowAsync(message));
        }

        return rows;
    }

    public async Task<IReadOnlyList<MessageRow>> SearchGlobalAsync(SearchQuery query, int limit = 40)
    {
        RequireClient();
        ArgumentNullException.ThrowIfNull(query);

        var found = await _client!.ExecuteAsync(new TdApi.SearchMessages
        {
            ChatList = null,
            Query = query.Text,
            Offset = string.Empty,
            Limit = limit,
            Filter = ToFilter(query.Filter),
            ChatTypeFilter = null,
            MinDate = query.MinDate,
            MaxDate = query.MaxDate
        });

        var rows = new List<MessageRow>(found.Messages.Length);

        foreach (var message in found.Messages)
        {
            rows.Add(await ToRowAsync(message));
        }

        return rows;
    }

    private static TdApi.SearchMessagesFilter? ToFilter(SearchFilterKind filter) => filter switch
    {
        SearchFilterKind.Photo => new TdApi.SearchMessagesFilter.SearchMessagesFilterPhoto(),
        SearchFilterKind.Video => new TdApi.SearchMessagesFilter.SearchMessagesFilterVideo(),
        SearchFilterKind.Document => new TdApi.SearchMessagesFilter.SearchMessagesFilterDocument(),
        SearchFilterKind.Audio => new TdApi.SearchMessagesFilter.SearchMessagesFilterAudio(),
        SearchFilterKind.Voice => new TdApi.SearchMessagesFilter.SearchMessagesFilterVoiceNote(),
        SearchFilterKind.Link => new TdApi.SearchMessagesFilter.SearchMessagesFilterUrl(),
        _ => null
    };

    public async Task ToggleTopicClosedAsync(long chatId, int topicId, bool closed)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.ToggleForumTopicIsClosed
        {
            ChatId = chatId,
            ForumTopicId = topicId,
            IsClosed = closed
        });
    }

    public async Task DeleteTopicAsync(long chatId, int topicId)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.DeleteForumTopic
        {
            ChatId = chatId,
            ForumTopicId = topicId
        });
    }

    public async Task ToggleTopicPinnedAsync(long chatId, int topicId, bool pinned)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.ToggleForumTopicIsPinned
        {
            ChatId = chatId,
            ForumTopicId = topicId,
            IsPinned = pinned
        });
    }

    public async Task UnbanMemberAsync(long chatId, TdApi.MessageSender member)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.SetChatMemberStatus
        {
            ChatId = chatId,
            MemberId = member,
            Status = new TdApi.ChatMemberStatus.ChatMemberStatusMember { MemberUntilDate = 0 }
        });
    }

    public async Task<IReadOnlyList<ProxyEntry>> GetProxiesAsync()
    {
        RequireClient();

        var proxies = await _client!.ExecuteAsync(new TdApi.GetProxies());

        return proxies.Proxies
            .Select(proxy => new ProxyEntry(proxy.Id, proxy.Proxy.Server, proxy.Proxy.Port, proxy.IsEnabled, proxy.Comment))
            .ToArray();
    }

    public async Task EnableProxyAsync(int proxyId)
    {
        RequireClient();
        await _client!.ExecuteAsync(new TdApi.EnableProxy { ProxyId = proxyId });
    }

    public async Task RemoveProxyAsync(int proxyId)
    {
        RequireClient();
        await _client!.ExecuteAsync(new TdApi.RemoveProxy { ProxyId = proxyId });
    }

    public async Task DisableProxyAsync()
    {
        RequireClient();
        await _client!.ExecuteAsync(new TdApi.DisableProxy());
    }

    public async Task<MessagePermissions> GetPermissionsAsync(long chatId, long messageId)
    {
        RequireClient();

        var properties = await _client!.ExecuteAsync(new TdApi.GetMessageProperties
        {
            ChatId = chatId,
            MessageId = messageId
        });

        return new MessagePermissions(
            properties.CanBeEdited,
            properties.CanBeDeletedForAllUsers,
            properties.CanBeForwarded);
    }

    public async Task RenameFolderAsync(int folderId, string name)
    {
        RequireClient();

        var folder = await _client!.ExecuteAsync(new TdApi.GetChatFolder { ChatFolderId = folderId });

        folder.Name = new TdApi.ChatFolderName
        {
            Text = new TdApi.FormattedText { Text = name, Entities = [] },
            AnimateCustomEmoji = false
        };

        await _client.ExecuteAsync(new TdApi.EditChatFolder
        {
            ChatFolderId = folderId,
            Folder = folder
        });
    }

    public async Task RenameTopicAsync(long chatId, int topicId, string name)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.EditForumTopic
        {
            ChatId = chatId,
            ForumTopicId = topicId,
            Name = name,
            EditIconCustomEmoji = false,
            IconCustomEmojiId = 0
        });
    }

    public async Task ReadTopicBadgesAsync(long chatId, int topicId)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.ReadAllForumTopicMentions
        {
            ChatId = chatId,
            ForumTopicId = topicId
        });

        await _client.ExecuteAsync(new TdApi.ReadAllForumTopicReactions
        {
            ChatId = chatId,
            ForumTopicId = topicId
        });
    }

    public async Task VoteAsync(long chatId, long messageId, int optionId)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.SetPollAnswer
        {
            ChatId = chatId,
            MessageId = messageId,
            OptionIds = [optionId]
        });
    }

    public async Task CreateForumTopicAsync(long chatId, string name)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.CreateForumTopic
        {
            ChatId = chatId,
            Name = name,
            IsNameImplicit = false,
            Icon = null
        });
    }

    public async Task<IReadOnlyList<MessageRow>> LoadScheduledAsync(long chatId)
    {
        RequireClient();

        var scheduled = await _client!.ExecuteAsync(new TdApi.GetChatScheduledMessages { ChatId = chatId });
        var rows = new List<MessageRow>(scheduled.Messages_.Length);

        foreach (var message in scheduled.Messages_)
        {
            rows.Add(await ToRowAsync(message));
        }

        return rows;
    }

    public async Task<IReadOnlyList<MessageRow>> SearchInChatAsync(long chatId, string query, int limit = 40)
    {
        RequireClient();

        var found = await _client!.ExecuteAsync(new TdApi.SearchChatMessages
        {
            ChatId = chatId,
            TopicId = null,
            Query = query,
            SenderId = null,
            FromMessageId = 0,
            Offset = 0,
            Limit = limit,
            Filter = null
        });

        var rows = new List<MessageRow>(found.Messages.Length);

        foreach (var message in found.Messages)
        {
            rows.Add(await ToRowAsync(message));
        }

        return rows;
    }

    public async Task<MessageRow?> GetPinnedAsync(long chatId)
    {
        RequireClient();

        try
        {
            var pinned = await _client!.ExecuteAsync(new TdApi.GetChatPinnedMessage { ChatId = chatId });
            return await ToRowAsync(pinned);
        }
        catch (TdException)
        {
            return null;
        }
    }

    public async Task SetMutedAsync(long chatId, bool muted)
    {
        RequireClient();

        var chat = await _client!.ExecuteAsync(new TdApi.GetChat { ChatId = chatId });
        var current = chat.NotificationSettings;

        await _client.ExecuteAsync(new TdApi.SetChatNotificationSettings
        {
            ChatId = chatId,
            NotificationSettings = new TdApi.ChatNotificationSettings
            {
                UseDefaultMuteFor = false,
                MuteFor = muted ? int.MaxValue : 0,
                UseDefaultSound = current.UseDefaultSound,
                SoundId = current.SoundId,
                UseDefaultShowPreview = current.UseDefaultShowPreview,
                ShowPreview = current.ShowPreview,
                UseDefaultMuteStories = current.UseDefaultMuteStories,
                MuteStories = current.MuteStories,
                UseDefaultStorySound = current.UseDefaultStorySound,
                StorySoundId = current.StorySoundId,
                UseDefaultShowStoryPoster = current.UseDefaultShowStoryPoster,
                ShowStoryPoster = current.ShowStoryPoster
            }
        });
    }

    public async Task SetPinnedAsync(long chatId, bool pinned)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.ToggleChatIsPinned
        {
            ChatList = new TdApi.ChatList.ChatListMain(),
            ChatId = chatId,
            IsPinned = pinned
        });
    }

    public async Task SetArchivedAsync(long chatId, bool archived)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.AddChatToList
        {
            ChatId = chatId,
            ChatList = archived
                ? new TdApi.ChatList.ChatListArchive()
                : new TdApi.ChatList.ChatListMain()
        });
    }

    public async Task AddReactionAsync(long chatId, long messageId, string emoji)
    {
        RequireClient();

        await _client!.ExecuteAsync(new TdApi.AddMessageReaction
        {
            ChatId = chatId,
            MessageId = messageId,
            ReactionType = new TdApi.ReactionType.ReactionTypeEmoji { Emoji = emoji },
            IsBig = false,
            UpdateRecentReactions = true
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

            case TdApi.Update.UpdateChatFolders folders:
                Folders = folders.ChatFolders.Select(folder => new FolderRow(folder.Id, folder.Name.Text.Text)).ToArray();
                FoldersChanged?.Invoke(this, EventArgs.Empty);
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

                if (!newMessage.Message.IsOutgoing)
                {
                    MessageArrived?.Invoke(
                        this,
                        new IncomingMessageEventArgs(newMessage.Message.ChatId, Describe(newMessage.Message.Content).Text));
                }

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

        var directory = _dataDirectory;

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

    private static TdApi.ChatList ToChatList(ChatListEntry? list) => list?.Kind switch
    {
        ChatListKind.Archive => new TdApi.ChatList.ChatListArchive(),
        ChatListKind.Folder => new TdApi.ChatList.ChatListFolder { ChatFolderId = list.FolderId },
        _ => new TdApi.ChatList.ChatListMain()
    };

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

        return new MessageRow(message.Id, message.ChatId, sender, kind, text, time, message.IsOutgoing, message.IsPinned, FileIdOf(message.Content));
    }

    private static int FileIdOf(TdApi.MessageContent content) => content switch
    {
        TdApi.MessageContent.MessageVoiceNote voice => voice.VoiceNote.Voice.Id,
        TdApi.MessageContent.MessageAudio audio => audio.Audio.Audio_.Id,
        TdApi.MessageContent.MessageDocument document => document.Document.Document_.Id,
        TdApi.MessageContent.MessageVideo video => video.Video.Video_.Id,
        TdApi.MessageContent.MessageAnimation animation => animation.Animation.Animation_.Id,
        _ => 0
    };

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
