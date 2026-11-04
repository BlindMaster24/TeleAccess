using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TeleAccess.App.Commands;
using TeleAccess.App.Input;
using TeleAccess.App.Localization;
using TeleAccess.App.Services;
using TeleAccess.App.Windows;
using TeleAccess.Core;
using TeleAccess.Core.Attachments;
using TeleAccess.Core.ChatLists;
using TeleAccess.Core.Chats;
using TeleAccess.Core.Data;
using TeleAccess.Core.Dates;
using TeleAccess.Core.Network;
using TeleAccess.Core.Notifications;
using TeleAccess.Core.Profiles;

namespace TeleAccess.App;

public partial class MainWindow : Window
{
    private readonly AppDatabase _database;
    private readonly SettingsRepository _settingsRepository;
    private readonly KeymapRepository _keymapRepository;
    private readonly PrismSpeechService _speech = new();
    private readonly VoiceRecorder _recorder = new();
    private readonly AudioPlayer _player = new();
    private GlobalHotkeyService? _globalHotkeys;
    private readonly TelegramService _telegram = new();
    private readonly CommandRegistry _registry;
    private readonly HotkeyService _hotkeys;
    private readonly DispatcherTimer _openChatTimer;
    private readonly AppSettings _settings;
    private ChatRow? _currentChat;
    private MessageRow? _currentMessage;
    private long _replyToMessageId;
    private bool _showingTransient;
    private ChatListEntry _chatList = new("Main", ChatListKind.Main);
    private IReadOnlyList<ChatListEntry> _listEntries = [new("Main", ChatListKind.Main), new("Archive", ChatListKind.Archive)];

    public MainWindow()
    {
        InitializeComponent();

        _database = new AppDatabase(AppSettings.DefaultDatabasePath);
        _settingsRepository = new SettingsRepository(_database);
        _keymapRepository = new KeymapRepository(_database);
        _settings = AppSettings.Load(_settingsRepository);

        _registry = CommandCatalog.Create(CreateActions());
        _keymapRepository.Load(_registry);

        _hotkeys = new HotkeyService(this, _registry) { ActiveScope = CurrentScope };
        _hotkeys.CommandInvoked += OnCommandInvoked;

        _openChatTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };
        _openChatTimer.Tick += OnOpenChatTimerTick;

        _telegram.AuthStageChanged += OnAuthStageChanged;
        _telegram.ChatsChanged += OnChatsChanged;
        _telegram.ChatMessagesChanged += OnChatMessagesChanged;
        _telegram.FoldersChanged += OnFoldersChanged;
        _telegram.MessageArrived += OnMessageArrived;

        Composer.TextChanged += OnComposerTextChanged;
        Loaded += (_, _) => RegisterGlobalHotkey();
        LoadSettingsIntoForm();
        _telegram.Start(_settings);
        Announce(Strings.Login_Starting);
    }

    protected override void OnClosed(EventArgs e)
    {
        _speech.Dispose();
        _telegram.Dispose();
        _globalHotkeys?.Dispose();
        _recorder.Dispose();
        _player.Dispose();
        base.OnClosed(e);
    }

    private UiActions CreateActions() => new(
        HasChat: () => _currentChat is not null,
        HasMessage: () => _currentMessage is not null,
        FocusChats: () => ChatList.Focus(),
        FocusMessages: () => MessageList.Focus(),
        FocusComposer: () => Composer.Focus(),
        FocusSearch: () => SearchBox.Focus(),
        Search: OnSearch,
        Refresh: () => _ = RefreshChatsAsync(SearchBox.Text),
        Hotkeys: OnHotkeys,
        SearchGlobal: OnSearchGlobal,
        ChatInfo: OnChatInfo,
        ChatMembers: OnChatMembers,
        ChatTopics: OnChatTopics,
        ChatJoin: OnChatJoin,
        Contacts: OnContacts,
        Settings: OnSettings,
        SecretChat: OnSecretChat,
        Training: OnTraining,
        SetName: OnSetName,
        SetBio: OnSetBio,
        Proxy: OnProxy,
        MuteScopes: OnMuteScopes,
        Stories: OnStories,
        Quit: Close,
        Chat: CreateChatActions(),
        Message: CreateMessageActions(),
        Composer: CreateComposerActions());

    private ChatActions CreateChatActions() => new(
        Open: OnOpenChat,
        Delete: OnDeleteChat,
        MarkUnread: OnMarkUnread,
        Mute: OnMute,
        Pin: OnPinChat,
        Archive: OnArchive,
        ShowScheduled: OnScheduled,
        SwitchList: OnSwitchList,
        Rename: OnRename,
        InviteLink: OnInviteLink);

    private MessageActions CreateMessageActions() => new(
        ReadAloud: OnReadAloud,
        Reply: OnReply,
        Edit: OnEdit,
        Delete: OnDeleteMessage,
        DeleteForMe: OnDeleteForMe,
        Forward: OnForward,
        Pin: OnPin,
        Copy: OnCopy,
        React: OnReact,
        SearchInChat: OnSearchInChat,
        JumpToPinned: OnJumpToPinned,
        Translate: OnTranslate,
        Save: OnSaveFile,
        Older: OnOlder,
        Select: OnSelect,
        SelectAll: OnSelectAll,
        JumpToDate: OnJumpToDate,
        Buttons: OnButtons);

    private ComposerActions CreateComposerActions() => new(
        Send: OnSend,
        SendSilent: OnSendSilent,
        SendScheduled: OnSendScheduled,
        Attach: OnAttach,
        NewLine: OnNewLine,
        Voice: OnVoiceToggle,
        SendContact: OnSendContact,
        SendLocation: OnSendLocation,
        SendPoll: OnSendPoll);

    private CommandScope CurrentScope()
    {
        if (Composer.IsKeyboardFocusWithin)
        {
            return CommandScope.Composer;
        }

        if (MessageList.IsKeyboardFocusWithin)
        {
            return CommandScope.Message;
        }

        return ChatList.IsKeyboardFocusWithin ? CommandScope.ChatList : CommandScope.Global;
    }

    private void OnCommandInvoked(object? sender, CommandInvokedEventArgs e)
    {
        if (!e.Command.IsAvailable)
        {
            Announce(string.Format(Strings.Culture, Strings.Status_Unavailable, e.Command.Title));
            return;
        }

        if (_training)
        {
            _speech.Speak(e.Command.Title);
        }
    }

    private void OnMessageArrived(object? sender, IncomingMessageEventArgs e)
    {
        if (e.ChatId == _currentChat?.Id)
        {
            return;
        }

        Dispatcher.Invoke(() =>
        {
            System.Media.SystemSounds.Asterisk.Play();

            var title = (ChatList.ItemsSource as IReadOnlyList<ChatRow>)?
                .FirstOrDefault(chat => chat.Id == e.ChatId)?.Title ?? string.Empty;

            Announce(string.Format(Strings.Culture, Strings.Status_NewMessage, title, e.Text));
            _ = RefreshChatsAsync(SearchBox.Text);
        });
    }

    private void Announce(string text)
    {
        Status.Text = text;
        _speech.Speak(text);
    }

    private void LoadSettingsIntoForm()
    {
        ApiIdBox.Text = _settings.ApiId > 0 ? _settings.ApiId.ToString(CultureInfo.InvariantCulture) : string.Empty;
        ApiHashBox.Text = _settings.ApiHash;
        PhoneBox.Text = _settings.PhoneNumber;
    }

    private void OnAuthStageChanged(object? sender, AuthStageChangedEventArgs e) =>
        Dispatcher.Invoke(() => ApplyStage(e.Stage));

    private void OnFoldersChanged(object? sender, EventArgs e) => Dispatcher.Invoke(RebuildLists);

    private void RebuildLists()
    {
        var entries = new List<ChatListEntry> { new("Main", ChatListKind.Main) };

        foreach (var folder in _telegram.Folders)
        {
            entries.Add(new ChatListEntry(folder.Title, ChatListKind.Folder, folder.Id));
        }

        entries.Add(new ChatListEntry("Archive", ChatListKind.Archive));

        if (!entries.Contains(_chatList))
        {
            _chatList = entries[0];
        }

        _listEntries = entries;
    }

    private void OnChatsChanged(object? sender, EventArgs e) =>
        Dispatcher.Invoke(() => _ = RefreshChatsAsync(SearchBox.Text));

    private void OnChatMessagesChanged(object? sender, ChatMessagesChangedEventArgs e)
    {
        if (_currentChat is not null && _currentChat.Id == e.ChatId && !_showingTransient)
        {
            Dispatcher.Invoke(() => _ = RefreshMessagesAsync(_currentChat));
        }
    }

    private void ApplyStage(AuthStage stage)
    {
        var ready = stage == AuthStage.Ready;
        LoginPanel.Visibility = ready ? Visibility.Collapsed : Visibility.Visible;
        MainPanel.Visibility = ready ? Visibility.Visible : Visibility.Collapsed;

        switch (stage)
        {
            case AuthStage.NeedParameters:
                Announce(Strings.Login_HintNeedParameters);
                ApiIdBox.Focus();
                break;

            case AuthStage.NeedPhone:
                Announce(Strings.Login_HintNeedPhone);
                PhoneBox.Focus();
                break;

            case AuthStage.NeedCode:
                Announce(Strings.Login_HintNeedCode);
                CodeBox.Focus();
                break;

            case AuthStage.NeedPassword:
                Announce(Strings.Login_HintNeedPassword);
                PasswordBox.Focus();
                break;

            case AuthStage.Ready:
                Announce(Strings.Login_HintReady);
                _ = RefreshChatsAsync();
                ChatList.Focus();
                break;

            case AuthStage.Closed:
                Announce(Strings.Login_Closed);
                break;

            default:
                break;
        }
    }

    private async void OnContinueClick(object sender, RoutedEventArgs e)
    {
        try
        {
            switch (_telegram.Stage)
            {
                case AuthStage.NeedParameters:
                    await SaveCredentialsAsync();
                    break;

                case AuthStage.NeedPhone:
                    _settings.PhoneNumber = PhoneBox.Text.Trim();
                    _settings.Save(_settingsRepository);
                    await _telegram.SubmitPhoneAsync(_settings.PhoneNumber);
                    break;

                case AuthStage.NeedCode:
                    await _telegram.SubmitCodeAsync(CodeBox.Text.Trim());
                    CodeBox.Clear();
                    break;

                case AuthStage.NeedPassword:
                    await _telegram.SubmitPasswordAsync(PasswordBox.Password);
                    PasswordBox.Clear();
                    break;

                default:
                    break;
            }
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private async Task SaveCredentialsAsync()
    {
        if (!int.TryParse(ApiIdBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var apiId) ||
            apiId <= 0 ||
            string.IsNullOrWhiteSpace(ApiHashBox.Text))
        {
            Announce(Strings.Error_NotConfigured);
            ApiIdBox.Focus();
            return;
        }

        _settings.ApiId = apiId;
        _settings.ApiHash = ApiHashBox.Text.Trim();
        _settings.Save(_settingsRepository);

        await _telegram.RetryParametersAsync();
    }

    private async Task RefreshChatsAsync(string? query = null)
    {
        if (_telegram.Stage != AuthStage.Ready)
        {
            return;
        }

        try
        {
            var chats = await _telegram.LoadChatsAsync(
                query: string.IsNullOrWhiteSpace(query) ? null : query,
                list: _chatList);
            var selected = (ChatList.SelectedItem as ChatRow)?.Id;
            ChatList.ItemsSource = chats;

            if (chats.Count == 0)
            {
                Announce(string.IsNullOrWhiteSpace(query) ? Strings.Main_NoChats : Strings.Status_SearchEmpty);
                return;
            }

            var restored = chats.FirstOrDefault(c => c.Id == selected);
            if (restored is not null)
            {
                ChatList.SelectedItem = restored;
            }
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e) => _ = RefreshChatsAsync(SearchBox.Text);

    private void OnSearch() => _ = RefreshChatsAsync(SearchBox.Text);

    private void OnChatSelected(object sender, SelectionChangedEventArgs e)
    {
        if (ChatList.SelectedItem is not ChatRow chat)
        {
            return;
        }

        _currentChat = chat;
        _currentMessage = null;
        _replyToMessageId = 0;
        _selected.Clear();
        _showingTransient = false;
        _openChatTimer.Stop();
        _openChatTimer.Start();
    }

    private void OnOpenChatTimerTick(object? sender, EventArgs e)
    {
        _openChatTimer.Stop();

        if (_currentChat is not null)
        {
            _ = RefreshMessagesAsync(_currentChat);
        }
    }

    private void OnOpenChat() => Composer.Focus();

    private async Task RefreshMessagesAsync(ChatRow chat)
    {
        _showingTransient = false;

        try
        {
            await _telegram.OpenChatAsync(chat.Id);
            var messages = await _telegram.LoadMessagesAsync(chat.Id);

            MessageList.ItemsSource = messages;

            if (messages.Count == 0)
            {
                MessageList.ItemsSource = Array.Empty<MessageRow>();
            }

            await _telegram.MarkChatReadAsync(chat.Id);

            Announce(messages.Count == 0
                ? $"{chat.Title}. {Strings.Main_NoMessages}"
                : $"{chat.Title}. {TeleAccess.Core.PluralFormatter.Format(Strings.Plural_UnreadMessages, messages.Count, Strings.Culture)}");

            MessageList.Focus();
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnMessageSelected(object sender, SelectionChangedEventArgs e) => _currentMessage = MessageList.SelectedItem as MessageRow;

    private void OnMessageListKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F10 && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            var menu = CommandCatalog.BuildContextMenu(_registry, CommandScope.Message);
            menu.PlacementTarget = MessageList;
            menu.IsOpen = true;
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape && _replyToMessageId != 0)
        {
            _replyToMessageId = 0;
            Announce(Strings.Hotkeys_Cancelled);
            e.Handled = true;
        }
    }

    private void OnSend() => SendPending(SendOptions.Normal);

    private void OnSendSilent() => SendPending(new SendOptions(Silent: true));

    private void OnSendScheduled()
    {
        var at = ScheduleWindow.Ask(this);
        if (at is null)
        {
            return;
        }

        SendPending(new SendOptions(SendAt: at.Value), at.Value);
    }

    private void SendPending(SendOptions options, DateTimeOffset? announceAt = null)
    {
        var text = Composer.Text.Trim();

        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        if (text.Length == 0)
        {
            Announce(Strings.Status_NoSelection);
            return;
        }

        Composer.Clear();
        _ = SendAsync(text, options, announceAt);
    }

    private async Task SendAsync(string text, SendOptions options, DateTimeOffset? announceAt)
    {
        if (_currentChat is null)
        {
            return;
        }

        var replyTo = _replyToMessageId;
        _replyToMessageId = 0;

        try
        {
            await _telegram.SendTextAsync(_currentChat.Id, text, replyTo, options);

            if (announceAt is { } at)
            {
                Announce(string.Format(Strings.Culture, Strings.Status_ScheduledFor, at.ToString("g", Strings.Culture)));
            }
            else
            {
                Announce(options.Silent ? Strings.Status_MutedChatSend : Strings.Status_Sent);
            }

            await RefreshMessagesAsync(_currentChat);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
            Composer.Text = text;
        }
    }

    private void OnMute() => _ = ToggleMuteAsync();

    private async Task ToggleMuteAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        var muted = !_currentChat.IsMuted;

        try
        {
            await _telegram.SetMutedAsync(_currentChat.Id, muted);
            Announce(muted ? Strings.Status_Muted : Strings.Status_Unmuted);
            await RefreshChatsAsync(SearchBox.Text);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnPinChat() => _ = TogglePinAsync();

    private async Task TogglePinAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        var pinned = !_currentChat.IsPinned;

        try
        {
            await _telegram.SetPinnedAsync(_currentChat.Id, pinned);
            Announce(pinned ? Strings.Status_ChatPinned : Strings.Status_ChatUnpinned);
            await RefreshChatsAsync(SearchBox.Text);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnArchive() => _ = ToggleArchiveAsync();

    private async Task ToggleArchiveAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        var archived = !_currentChat.IsArchived;

        try
        {
            await _telegram.SetArchivedAsync(_currentChat.Id, archived);
            Announce(archived ? Strings.Status_Archived : Strings.Status_Unarchived);
            await RefreshChatsAsync(SearchBox.Text);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnScheduled() => _ = ShowScheduledAsync();

    private async Task ShowScheduledAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        try
        {
            var rows = await _telegram.LoadScheduledAsync(_currentChat.Id);

            if (rows.Count == 0)
            {
                Announce(Strings.Status_NotScheduled);
                return;
            }

            ShowTransient(rows);
            Announce(PluralFormatter.Format(Strings.Status_ScheduledMessages, rows.Count, Strings.Culture));
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnReact() => _ = ReactAsync();

    private async Task ReactAsync()
    {
        if (_currentChat is null || _currentMessage is null)
        {
            Announce(Strings.Status_NoSelection);
            return;
        }

        var emoji = PromptWindow.Ask(this, Strings.Prompt_Reaction, "\U0001F44D");
        if (string.IsNullOrWhiteSpace(emoji))
        {
            return;
        }

        try
        {
            await _telegram.AddReactionAsync(_currentChat.Id, _currentMessage.Id, emoji.Trim());
            Announce(Strings.Status_ReactionSent);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnSearchInChat() => _ = SearchInChatAsync();

    private async Task SearchInChatAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        var query = PromptWindow.Ask(this, Strings.Prompt_SearchInChat);
        if (string.IsNullOrWhiteSpace(query))
        {
            return;
        }

        try
        {
            var rows = await _telegram.SearchInChatAsync(_currentChat.Id, query);

            if (rows.Count == 0)
            {
                Announce(Strings.Status_SearchEmpty);
                return;
            }

            ShowTransient(rows);
            Announce(PluralFormatter.Format(Strings.Status_FoundMessages, rows.Count, Strings.Culture));
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void ShowTransient(IReadOnlyList<MessageRow> rows)
    {
        _showingTransient = true;
        MessageList.ItemsSource = rows;
        MessageList.Focus();
    }

    private DateTimeOffset _lastTyping;

    private void OnComposerTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_currentChat is null || DateTimeOffset.UtcNow - _lastTyping < TimeSpan.FromSeconds(4))
        {
            return;
        }

        _lastTyping = DateTimeOffset.UtcNow;
        _ = _telegram.SendChatActionAsync(_currentChat.Id, typing: true);
    }

    private void OnSearchGlobal() => _ = SearchGlobalAsync();

    private async Task SearchGlobalAsync()
    {
        var query = PromptWindow.Ask(this, Strings.Prompt_GlobalSearch);
        if (string.IsNullOrWhiteSpace(query))
        {
            return;
        }

        try
        {
            var rows = await _telegram.SearchGlobalAsync(query);

            if (rows.Count == 0)
            {
                Announce(Strings.Status_SearchEmpty);
                return;
            }

            ShowTransient(rows);
            Announce(PluralFormatter.Format(Strings.Status_FoundMessages, rows.Count, Strings.Culture));
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnTranslate() => _ = TranslateAsync();

    private async Task TranslateAsync()
    {
        if (_currentMessage is null || _currentMessage.Text.Length == 0)
        {
            Announce(Strings.Status_NoSelection);
            return;
        }

        var language = PromptWindow.Ask(this, Strings.Prompt_TranslateTo, _settings.Language);
        if (string.IsNullOrWhiteSpace(language))
        {
            return;
        }

        try
        {
            var translated = await _telegram.TranslateAsync(_currentMessage.Text, language.Trim());
            Announce(translated);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnSendContact() => _ = SendContactAsync();

    private async Task SendContactAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        var phone = PromptWindow.Ask(this, Strings.Prompt_ContactPhone);
        if (string.IsNullOrWhiteSpace(phone))
        {
            return;
        }

        var name = PromptWindow.Ask(this, Strings.Prompt_ContactName);
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        try
        {
            await _telegram.SendContactAsync(_currentChat.Id, phone.Trim(), name.Trim(), string.Empty, _replyToMessageId);
            _replyToMessageId = 0;
            Announce(Strings.Status_ContactSent);
            await RefreshMessagesAsync(_currentChat);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnSendLocation() => _ = SendLocationAsync();

    private async Task SendLocationAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        var answer = PromptWindow.Ask(this, Strings.Prompt_Location);
        if (string.IsNullOrWhiteSpace(answer))
        {
            return;
        }

        var parts = answer.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2 ||
            !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) ||
            !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
        {
            Announce(Strings.Error_BadLocation);
            return;
        }

        try
        {
            await _telegram.SendLocationAsync(_currentChat.Id, latitude, longitude, _replyToMessageId);
            _replyToMessageId = 0;
            Announce(Strings.Status_LocationSent);
            await RefreshMessagesAsync(_currentChat);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnSendPoll() => _ = SendPollAsync();

    private async Task SendPollAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        var question = PromptWindow.Ask(this, Strings.Prompt_PollQuestion);
        if (string.IsNullOrWhiteSpace(question))
        {
            return;
        }

        var options = PromptWindow.Ask(this, Strings.Prompt_PollOptions);
        if (string.IsNullOrWhiteSpace(options))
        {
            return;
        }

        try
        {
            await _telegram.SendPollAsync(
                _currentChat.Id,
                question.Trim(),
                options.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
                _replyToMessageId);

            _replyToMessageId = 0;
            Announce(Strings.Status_PollSent);
            await RefreshMessagesAsync(_currentChat);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnChatInfo() => _ = ShowChatInfoAsync();

    private async Task ShowChatInfoAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        try
        {
            var info = await _telegram.GetChatInfoAsync(_currentChat.Id);
            Announce(string.IsNullOrEmpty(info.Description)
                ? info.Display
                : $"{info.Display}. {info.Description}");
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnChatMembers() => _ = ShowMembersAsync();

    private async Task ShowMembersAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        try
        {
            var members = await _telegram.GetMembersAsync(_currentChat.Id);

            if (members.Count == 0)
            {
                Announce(Strings.Status_NoMembers);
                return;
            }

            Composer.Text = string.Join(Environment.NewLine, members);
            Announce(PluralFormatter.Format(Strings.Plural_SelectedMessages, members.Count, Strings.Culture));
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnChatTopics() => _ = ShowTopicsAsync();

    private async Task ShowTopicsAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        try
        {
            var topics = await _telegram.GetForumTopicsAsync(_currentChat.Id);

            if (topics.Count == 0)
            {
                Announce(Strings.Status_NoTopics);
                return;
            }

            Composer.Text = string.Join(Environment.NewLine, topics);
            Announce(string.Join(". ", topics));
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnChatJoin() => _ = JoinChatAsync();

    private async Task JoinChatAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        try
        {
            await _telegram.JoinChatAsync(_currentChat.Id);
            Announce(Strings.Status_Joined);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnContacts() => _ = ShowContactsAsync();

    private async Task ShowContactsAsync()
    {
        try
        {
            var contacts = await _telegram.GetContactsAsync();

            if (contacts.Count == 0)
            {
                Announce(Strings.Status_NoContacts);
                return;
            }

            Composer.Text = string.Join(Environment.NewLine, contacts);
            Announce(string.Join(". ", contacts));
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnSecretChat() => _ = StartSecretChatAsync();

    private async Task StartSecretChatAsync()
    {
        var username = PromptWindow.Ask(this, Strings.Prompt_SecretChatUser);
        if (string.IsNullOrWhiteSpace(username))
        {
            return;
        }

        try
        {
            var chatId = await _telegram.CreateSecretChatAsync(username.Trim());

            if (chatId == 0)
            {
                Announce(Strings.Status_NoUser);
                return;
            }

            Announce(Strings.Status_SecretChatStarted);
            await RefreshChatsAsync();
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnSettings()
    {
        var window = new SettingsWindow(_settings, _settingsRepository) { Owner = this };

        if (window.ShowDialog() != true)
        {
            return;
        }

        Announce(Strings.Settings_Saved);
        LocalizationManager.Apply(_settings.Language);

        if (window.SignOutRequested)
        {
            _ = SignOutAsync();
        }
    }

    private async Task SignOutAsync()
    {
        try
        {
            await _telegram.LogOutAsync();
            Announce(Strings.Status_SignedOut);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private bool _training;
    private readonly HashSet<long> _selected = [];

    private void RegisterGlobalHotkey()
    {
        if (!HotkeyCombination.TryParse("Ctrl+Alt+T", out var combination))
        {
            return;
        }

        _globalHotkeys = new GlobalHotkeyService(this, combination, () => Dispatcher.Invoke(() =>
        {
            Activate();
            ChatList.Focus();
            Announce(Strings.Status_Window);
        }));
    }

    private void OnSetName() => _ = SetNameAsync();

    private async Task SetNameAsync()
    {
        var answer = PromptWindow.Ask(this, Strings.Prompt_FullName);

        if (string.IsNullOrWhiteSpace(answer))
        {
            return;
        }

        if (!NameSplitter.TrySplit(answer, out var firstName, out var lastName))
        {
            Announce(Strings.Error_BadName);
            return;
        }

        try
        {
            await _telegram.SetNameAsync(firstName, lastName);
            Announce(Strings.Status_NameSaved);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnSetBio() => _ = SetBioAsync();

    private async Task SetBioAsync()
    {
        var answer = PromptWindow.Ask(this, Strings.Prompt_Bio);

        if (answer is null)
        {
            return;
        }

        try
        {
            await _telegram.SetBioAsync(answer.Trim());
            Announce(Strings.Status_BioSaved);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnProxy() => _ = SetProxyAsync();

    private async Task SetProxyAsync()
    {
        var answer = PromptWindow.Ask(this, Strings.Prompt_Proxy, "socks5://user:pass@host:1080");

        if (string.IsNullOrWhiteSpace(answer))
        {
            return;
        }

        if (!ProxyUri.TryParse(answer, out var proxy))
        {
            Announce(Strings.Error_BadProxy);
            return;
        }

        try
        {
            await _telegram.SetProxyAsync(proxy.Host, proxy.Port, proxy.Username, proxy.Password);
            Announce(string.Format(Strings.Culture, Strings.Status_ProxySet, proxy.Host, proxy.Port));
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnTraining()
    {
        _training = !_training;
        Announce(Strings.Cmd_Training);
    }

    private void OnSaveFile() => _ = SaveFileAsync();

    private async Task SaveFileAsync()
    {
        if (_currentMessage is null)
        {
            Announce(Strings.Status_NoSelection);
            return;
        }

        if (!_currentMessage.HasFile)
        {
            Announce(Strings.Status_NoFile);
            return;
        }

        try
        {
            var path = await _telegram.DownloadAsync(_currentMessage.FileId);

            if (string.IsNullOrEmpty(path))
            {
                Announce(Strings.Status_NoFile);
                return;
            }

            Announce(string.Format(Strings.Culture, Strings.Status_Saved, path));
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnButtons() => _ = ShowButtonsAsync();

    private async Task ShowButtonsAsync()
    {
        if (_currentChat is null || _currentMessage is null)
        {
            Announce(Strings.Status_NoSelection);
            return;
        }

        try
        {
            var buttons = await _telegram.GetButtonsAsync(_currentChat.Id, _currentMessage.Id);

            if (buttons.Count == 0)
            {
                Announce(Strings.Status_NoButtons);
                return;
            }

            Composer.Text = string.Join(Environment.NewLine, buttons.Select((text, index) => (index + 1) + ". " + text));
            Announce(string.Join(". ", buttons));

            var answer = PromptWindow.Ask(this, Strings.Prompt_ButtonNumber);

            if (string.IsNullOrWhiteSpace(answer) || !int.TryParse(answer, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            {
                return;
            }

            await _telegram.PressButtonAsync(_currentChat.Id, _currentMessage.Id, number - 1);
            Announce(Strings.Status_ButtonPressed);
            await RefreshMessagesAsync(_currentChat);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnJumpToDate() => _ = JumpToDateAsync();

    private async Task JumpToDateAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        var answer = PromptWindow.Ask(this, Strings.Prompt_JumpToDate);

        if (string.IsNullOrWhiteSpace(answer))
        {
            return;
        }

        if (!DateQueryParser.TryParse(answer, DateTimeOffset.Now, out var date))
        {
            Announce(Strings.Error_BadDate);
            return;
        }

        try
        {
            var message = await _telegram.GetByDateAsync(_currentChat.Id, new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeZoneInfo.Local.GetUtcOffset(date.ToDateTime(TimeOnly.MinValue))));

            if (message is null)
            {
                Announce(Strings.Main_NoMessages);
                return;
            }

            ShowTransient([message]);
            Announce(message.Display);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnOlder() => _ = LoadOlderAsync();

    private async Task LoadOlderAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        var loaded = MessageList.ItemsSource as IReadOnlyList<MessageRow>;

        if (loaded is null || loaded.Count == 0)
        {
            Announce(Strings.Status_NoOlder);
            return;
        }

        try
        {
            var older = await _telegram.LoadOlderAsync(_currentChat.Id, loaded[0].Id);

            if (older.Count == 0)
            {
                Announce(Strings.Status_NoOlder);
                return;
            }

            MessageList.ItemsSource = older.Concat(loaded).ToArray();
            MessageList.SelectedIndex = older.Count - 1;
            Announce(PluralFormatter.Format(Strings.Status_FoundMessages, older.Count, Strings.Culture));
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private async Task PlayMessageAsync(MessageRow message)
    {
        try
        {
            var path = await _telegram.DownloadAsync(message.FileId);

            if (string.IsNullOrEmpty(path))
            {
                Announce(message.Display);
                return;
            }

            _player.Play(path);
            Announce($"{Strings.Status_Playing}. {message.Display}");
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnSelect()
    {
        if (_currentMessage is null)
        {
            Announce(Strings.Status_NoSelection);
            return;
        }

        if (!_selected.Add(_currentMessage.Id))
        {
            _selected.Remove(_currentMessage.Id);
        }

        Announce(PluralFormatter.Format(Strings.Plural_SelectedMessages, _selected.Count, Strings.Culture));
    }

    private void OnSelectAll()
    {
        if (MessageList.ItemsSource is not IReadOnlyList<MessageRow> loaded)
        {
            return;
        }

        _selected.Clear();

        foreach (var row in loaded)
        {
            _selected.Add(row.Id);
        }

        Announce(PluralFormatter.Format(Strings.Plural_SelectedMessages, _selected.Count, Strings.Culture));
    }

    private void OnRename() => _ = RenameChatAsync();

    private async Task RenameChatAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        var answer = PromptWindow.Ask(this, Strings.Prompt_ChatTitle, _currentChat.Title);

        if (!ChatTitle.TryNormalize(answer, out var title))
        {
            Announce(Strings.Error_BadTitle);
            return;
        }

        try
        {
            await _telegram.SetChatTitleAsync(_currentChat.Id, title);
            Announce(Strings.Status_TitleSaved);
            await RefreshChatsAsync(SearchBox.Text);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnInviteLink() => _ = CreateInviteLinkAsync();

    private async Task CreateInviteLinkAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        var answer = PromptWindow.Ask(this, Strings.Prompt_InviteDays, "7");

        if (!int.TryParse(answer, NumberStyles.Integer, CultureInfo.InvariantCulture, out var days))
        {
            Announce(Strings.Error_BadNumber);
            return;
        }

        try
        {
            var link = await _telegram.CreateInviteLinkAsync(_currentChat.Id, days);
            var existing = await _telegram.GetInviteLinksAsync(_currentChat.Id);

            Composer.Text = string.Join(Environment.NewLine, existing.Prepend(link).Distinct(StringComparer.Ordinal));
            Announce(link);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnMuteScopes() => _ = MuteScopesAsync();

    private async Task MuteScopesAsync()
    {
        var scopes = PromptWindow.Ask(this, Strings.Prompt_MuteScopes, "p,g,c");

        if (string.IsNullOrWhiteSpace(scopes))
        {
            return;
        }

        var hoursAnswer = PromptWindow.Ask(this, Strings.Prompt_MuteHours, "8");

        if (!int.TryParse(hoursAnswer, NumberStyles.Integer, CultureInfo.InvariantCulture, out var hours))
        {
            Announce(Strings.Error_BadNumber);
            return;
        }

        if (!MuteFor.TryBuild(hours, out _))
        {
            Announce(Strings.Error_BadNumber);
            return;
        }

        var letters = scopes.Trim().ToUpperInvariant();

        try
        {
            await _telegram.SetScopeMutedAsync(
                letters.Contains('P', StringComparison.Ordinal),
                letters.Contains('G', StringComparison.Ordinal),
                letters.Contains('C', StringComparison.Ordinal),
                hours);

            Announce(hours == 0 ? Strings.Status_Unmuted : Strings.Status_Muted);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnStories() => _ = ShowStoriesAsync();

    private async Task ShowStoriesAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        try
        {
            var stories = await _telegram.GetActiveStoriesAsync(_currentChat.Id);

            if (stories.Count == 0)
            {
                Announce(Strings.Status_NoStories);
                return;
            }

            Composer.Text = string.Join(Environment.NewLine, stories);
            Announce(string.Join(". ", stories));
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnDeleteForMe() => _ = DeleteForMeAsync();

    private async Task DeleteForMeAsync()
    {
        if (_currentChat is null || _currentMessage is null)
        {
            Announce(Strings.Status_NoSelection);
            return;
        }

        try
        {
            await _telegram.DeleteMessagesAsync(_currentChat.Id, [_currentMessage.Id], false);
            Announce(Strings.Status_DeletedLocal);
            await RefreshMessagesAsync(_currentChat);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnJumpToPinned() => _ = JumpToPinnedAsync();

    private async Task JumpToPinnedAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        try
        {
            var pinned = await _telegram.GetPinnedAsync(_currentChat.Id);

            if (pinned is null)
            {
                Announce(Strings.Status_NoPinned);
                return;
            }

            ShowTransient([pinned]);
            Announce($"{Strings.Status_Pinned}. {pinned.Display}");
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnSwitchList()
    {
        _chatList = ChatListCycle.Next(_listEntries, _chatList);
        Announce(string.Format(Strings.Culture, Strings.Status_ListSwitched, _chatList.Title));
        _ = RefreshChatsAsync(SearchBox.Text);
    }

    private void OnVoiceToggle() => _ = ToggleVoiceAsync();

    private async Task ToggleVoiceAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        if (!_recorder.IsRecording)
        {
            try
            {
                _recorder.Start();
                Announce(Strings.Status_VoiceRecording);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
                Announce(Strings.Error_NoMicrophone);
            }

            return;
        }

        var duration = (int)Math.Round(_recorder.Duration.TotalSeconds);
        var path = _recorder.StopAndEncode();

        if (path is null)
        {
            Announce(Strings.Hotkeys_Cancelled);
            return;
        }

        try
        {
            await _telegram.SendVoiceAsync(_currentChat.Id, path, Math.Max(duration, 1), _replyToMessageId);
            _replyToMessageId = 0;
            Announce(Strings.Status_VoiceSent);
            await RefreshMessagesAsync(_currentChat);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private void OnAttach() => _ = AttachAsync();

    private async Task AttachAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = Strings.Cmd_Attach,
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != true || !System.IO.File.Exists(dialog.FileName))
        {
            return;
        }

        try
        {
            await SendAttachmentAsync(dialog.FileName);
            Composer.Clear();
            await RefreshMessagesAsync(_currentChat);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private static async Task<(int Width, int Height)> ProbeVideoAsync(string path)
    {
        var player = new System.Windows.Media.MediaPlayer();
        var opened = new TaskCompletionSource<bool>();

        player.MediaOpened += (_, _) => opened.TrySetResult(true);
        player.MediaFailed += (_, _) => opened.TrySetResult(false);
        player.Open(new Uri(path));

        var finished = await Task.WhenAny(opened.Task, Task.Delay(TimeSpan.FromSeconds(3)));
        var success = finished == opened.Task && await opened.Task;
        var size = success ? (player.NaturalVideoWidth, player.NaturalVideoHeight) : (0, 0);

        player.Close();
        return size;
    }

    private async Task SendAttachmentAsync(string path)
    {
        var chatId = _currentChat!.Id;
        var caption = Composer.Text.Trim();
        var replyTo = _replyToMessageId;

        _replyToMessageId = 0;

        await SendByExtensionAsync(chatId, path, caption, replyTo);
    }

    private async Task SendByExtensionAsync(long chatId, string path, string caption, long replyTo)
    {
        switch (AttachmentClassifier.Classify(path))
        {
            case AttachmentKind.Photo:
                await SendImageAsync(chatId, path, caption, replyTo);
                return;

            case AttachmentKind.Animation:
            case AttachmentKind.Video:
                await SendMovingImageAsync(chatId, path, caption, replyTo, AttachmentClassifier.Classify(path));
                return;

            case AttachmentKind.Sticker:
                await _telegram.SendStickerAsync(chatId, path, string.Empty, replyTo);
                Announce(Strings.Status_StickerSent);
                return;

            case AttachmentKind.Audio:
                var title = System.IO.Path.GetFileNameWithoutExtension(path);
                await _telegram.SendAudioAsync(chatId, path, 0, title, string.Empty, replyTo);
                Announce(Strings.Status_AudioSent);
                return;

            default:
                await _telegram.SendDocumentAsync(chatId, path, caption, replyTo);
                Announce(Strings.Status_Attached);
                return;
        }
    }

    private async Task SendImageAsync(long chatId, string path, string caption, long replyTo)
    {
        var frame = System.Windows.Media.Imaging.BitmapDecoder.Create(
            new Uri(path),
            System.Windows.Media.Imaging.BitmapCreateOptions.DelayCreation,
            System.Windows.Media.Imaging.BitmapCacheOption.None).Frames[0];

        await _telegram.SendPhotoAsync(chatId, path, caption, frame.PixelWidth, frame.PixelHeight, replyTo);
        Announce(Strings.Status_Attached);
    }

    private async Task SendMovingImageAsync(
        long chatId,
        string path,
        string caption,
        long replyTo,
        AttachmentKind kind)
    {
        if (kind == AttachmentKind.Animation)
        {
            var frame = System.Windows.Media.Imaging.BitmapDecoder.Create(
                new Uri(path),
                System.Windows.Media.Imaging.BitmapCreateOptions.DelayCreation,
                System.Windows.Media.Imaging.BitmapCacheOption.None).Frames[0];

            await _telegram.SendAnimationAsync(chatId, path, caption, frame.PixelWidth, frame.PixelHeight, 0, replyTo);
            Announce(Strings.Status_AnimationSent);
            return;
        }

        var (width, height) = await ProbeVideoAsync(path);

        if (width <= 0 || height <= 0)
        {
            await _telegram.SendDocumentAsync(chatId, path, caption, replyTo);
            Announce(Strings.Status_Attached);
            return;
        }

        await _telegram.SendVideoAsync(chatId, path, caption, width, height, 0, replyTo);
        Announce(Strings.Status_VideoSent);
    }
    private void OnNewLine()
    {
        var caret = Composer.CaretIndex;
        Composer.Text = Composer.Text.Insert(caret, Environment.NewLine);
        Composer.CaretIndex = caret + Environment.NewLine.Length;
    }

    private void OnReadAloud()
    {
        if (_currentMessage is null)
        {
            Announce(Strings.Status_NoSelection);
            return;
        }

        var playable = _currentMessage.Kind is MessageKind.Voice or MessageKind.Audio;

        if (playable && _currentMessage.HasFile)
        {
            _ = PlayMessageAsync(_currentMessage);
            return;
        }

        Announce(_currentMessage.Display);
    }

    private void OnReply()
    {
        if (_currentMessage is null)
        {
            Announce(Strings.Status_NoSelection);
            return;
        }

        _replyToMessageId = _currentMessage.Id;
        Composer.Focus();
        Announce(string.Format(Strings.Culture, Strings.Status_Unavailable, Strings.Cmd_Reply) + ": " + _currentMessage.Display);
    }

    private void OnDeleteMessage() => _ = DeleteMessageAsync();

    private async Task DeleteMessageAsync()
    {
        if (_currentChat is null || _currentMessage is null)
        {
            Announce(Strings.Status_NoSelection);
            return;
        }

        try
        {
            var ids = _selected.Count > 0 ? _selected.ToArray() : new[] { _currentMessage.Id };
            _selected.Clear();
            await _telegram.DeleteMessagesAsync(_currentChat.Id, ids, _currentMessage.IsOutgoing);
            Announce(_currentMessage.IsOutgoing ? Strings.Status_DeletedForAll : Strings.Status_DeletedLocal);
            await RefreshMessagesAsync(_currentChat);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnEdit() => _ = EditMessageAsync();

    private async Task EditMessageAsync()
    {
        if (_currentChat is null || _currentMessage is null)
        {
            Announce(Strings.Status_NoSelection);
            return;
        }

        var text = PromptWindow.Ask(this, Strings.Prompt_EditText, _currentMessage.Text);
        if (text is null || text.Length == 0)
        {
            return;
        }

        try
        {
            await _telegram.EditMessageAsync(_currentChat.Id, _currentMessage.Id, text);
            Announce(Strings.Status_Edited);
            await RefreshMessagesAsync(_currentChat);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnForward() => _ = ForwardAsync();

    private async Task ForwardAsync()
    {
        if (_currentChat is null || _currentMessage is null)
        {
            Announce(Strings.Status_NoSelection);
            return;
        }

        var target = PromptWindow.Ask(this, Strings.Prompt_ForwardTo);
        if (string.IsNullOrWhiteSpace(target))
        {
            return;
        }

        try
        {
            var chats = await _telegram.LoadChatsAsync(query: target);
            var destination = chats.Count > 0 ? chats[0] : null;
            if (destination is null)
            {
                Announce(Strings.Status_SearchEmpty);
                return;
            }

            var ids = _selected.Count > 0 ? _selected.ToArray() : new[] { _currentMessage.Id };
            _selected.Clear();

            foreach (var id in ids)
            {
                await _telegram.ForwardMessageAsync(_currentChat.Id, destination.Id, id);
            }

            Announce(Strings.Status_Forwarded);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnPin() => _ = PinAsync();

    private async Task PinAsync()
    {
        if (_currentChat is null || _currentMessage is null)
        {
            Announce(Strings.Status_NoSelection);
            return;
        }

        try
        {
            var pin = !_currentMessage.IsPinned;
            await _telegram.PinMessageAsync(_currentChat.Id, _currentMessage.Id, pin);
            Announce(pin ? Strings.Status_Pinned : Strings.Status_Unpinned);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnCopy()
    {
        if (_currentMessage is null || _currentMessage.Text.Length == 0)
        {
            Announce(Strings.Status_NoSelection);
            return;
        }

        try
        {
            Clipboard.SetText(_currentMessage.Text);
            Announce(Strings.Status_Copied);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnDeleteChat() => _ = DeleteChatAsync();

    private async Task DeleteChatAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        var answer = PromptWindow.Ask(this, Strings.Prompt_ConfirmDeleteChat, Strings.Prompt_Yes);
        if (answer is null || !answer.Trim().Equals(Strings.Prompt_Yes, StringComparison.OrdinalIgnoreCase))
        {
            Announce(Strings.Hotkeys_Cancelled);
            return;
        }

        try
        {
            await _telegram.DeleteChatAsync(_currentChat.Id);
            _currentChat = null;
            _currentMessage = null;
            Announce(Strings.Status_ChatDeleted);
            await RefreshChatsAsync(SearchBox.Text);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnMarkUnread() => _ = MarkUnreadAsync();

    private async Task MarkUnreadAsync()
    {
        if (_currentChat is null)
        {
            Announce(Strings.Status_NoChat);
            return;
        }

        try
        {
            await _telegram.ToggleMarkedUnreadAsync(_currentChat.Id, true);
            Announce(Strings.Status_MarkedUnread);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
        }
    }

    private void OnHotkeys()
    {
        _hotkeys.Suspended = true;
        try
        {
            var window = new HotkeysWindow(_registry, _keymapRepository) { Owner = this };
            window.ShowDialog();
        }
        finally
        {
            _hotkeys.Suspended = false;
            MessageList.Focus();
        }
    }
}
