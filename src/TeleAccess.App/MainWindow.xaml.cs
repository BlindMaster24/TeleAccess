using System.Globalization;
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
using TeleAccess.Core.Data;

namespace TeleAccess.App;

public partial class MainWindow : Window
{
    private readonly AppDatabase _database;
    private readonly SettingsRepository _settingsRepository;
    private readonly KeymapRepository _keymapRepository;
    private readonly PrismSpeechService _speech = new();
    private readonly TelegramService _telegram = new();
    private readonly CommandRegistry _registry;
    private readonly HotkeyService _hotkeys;
    private readonly DispatcherTimer _openChatTimer;
    private readonly AppSettings _settings;
    private ChatRow? _currentChat;
    private MessageRow? _currentMessage;
    private long _replyToMessageId;

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

        LoadSettingsIntoForm();
        _telegram.Start(_settings);
        Announce(Strings.Login_Starting);
    }

    protected override void OnClosed(EventArgs e)
    {
        _speech.Dispose();
        _telegram.Dispose();
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
        OpenChat: OnOpenChat,
        DeleteChat: OnDeleteChat,
        MarkUnread: OnMarkUnread,
        Send: OnSend,
        NewLine: OnNewLine,
        ReadAloud: OnReadAloud,
        Reply: OnReply,
        Edit: OnEdit,
        DeleteMessage: OnDeleteMessage,
        Forward: OnForward,
        Pin: OnPin,
        Copy: OnCopy,
        Voice: () => Announce(Strings.Status_NotImplemented),
        Hotkeys: OnHotkeys,
        Quit: Close);

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
        }
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

    private void OnChatsChanged(object? sender, EventArgs e) =>
        Dispatcher.Invoke(() => _ = RefreshChatsAsync(SearchBox.Text));

    private void OnChatMessagesChanged(object? sender, ChatMessagesChangedEventArgs e)
    {
        if (_currentChat is not null && _currentChat.Id == e.ChatId)
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
            var chats = await _telegram.LoadChatsAsync(query: string.IsNullOrWhiteSpace(query) ? null : query);
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

    private void OnSend()
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
        _ = SendAsync(text);
    }

    private async Task SendAsync(string text)
    {
        if (_currentChat is null)
        {
            return;
        }

        var replyTo = _replyToMessageId;
        _replyToMessageId = 0;

        try
        {
            await _telegram.SendTextAsync(_currentChat.Id, text, replyTo);
            Announce(Strings.Status_Sent);
            await RefreshMessagesAsync(_currentChat);
        }
        catch (Exception exception)
        {
            Announce(Errors.Describe(exception));
            Composer.Text = text;
        }
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
            await _telegram.DeleteMessagesAsync(_currentChat.Id, [_currentMessage.Id], _currentMessage.IsOutgoing);
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

            await _telegram.ForwardMessageAsync(_currentChat.Id, destination.Id, _currentMessage.Id);
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
            await _telegram.PinMessageAsync(_currentChat.Id, _currentMessage.Id, true);
            Announce(Strings.Status_Pinned);
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
