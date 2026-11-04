# Changelog

All notable changes are recorded here. Format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), versions follow
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Command registry: one action is one command, so menus, context menus and shortcuts are
  generated from a single list.
- Rebindable shortcuts with per-scope conflict detection, stored in
  `%APPDATA%\TeleAccess\teleaccess.db` (SQLite).
- TDLib integration: sign-in with api_id/api_hash, chat list, message history, sending,
  reply, edit, delete, forward, pin, chat deletion.
- Message send options: silent delivery (`Ctrl+Enter`) and scheduled delivery
  (`Ctrl+Shift+Enter`) through a window with explicit year, month, day, hour and minute
  pickers plus a manual parser for relative, time-only and absolute input.
- Scheduled queue, search inside a chat, reactions, chat mute, pin and archive.
- File attachments: `Ctrl+O` sends any local file as a document, with the composer text as caption.
- Voice messages: WASAPI capture, Opus encoding through Concentus, OGG muxing, sent as `inputMessageVoiceNote`; `Ctrl+Shift+V` starts and stops recording.
- OGG/Opus container writer with the OpusHead/OpusTags headers and per-page CRC.
- Audio files (mp3, m4a, flac, aac, wav, ogg, opus) are sent as audio messages, everything else as documents.
- Chat lists cycle with `Ctrl+Shift+L`: main, every Telegram folder, archive.
- Photos (with real dimensions), videos (dimensions probed from the media pipeline), animations, stickers, contacts, locations and polls can be sent; the typing action is reported to the other side.
- Attachment routing is one pure classifier in Core, covered by 16 tests.
- Message translation (`Ctrl+Shift+T`), search across all chats (`Ctrl+Shift+F`) and polls (`Ctrl+Alt+P`).
- Chat information (type, member count, description), member lists, forum topic lists, contact lists, secret chat creation, joining chats.
- Incoming voice notes and audio play through NAudio (`MediaFoundationReader`), `Enter` plays, `Escape` stops.
- Attachments download with `Ctrl+S`, history pages back with `Ctrl+Up`, new messages elsewhere announce with an earcon, and `Ctrl+F1` turns on training mode.
- Global search filters: `type:` and `date:` tokens become a TDLib filter and a date range; the token parser is pure and covered by thirteen tests.
- Member search by name (`searchChatMembers`) and the replied-message jump (`getRepliedMessage`) are available as commands.
- Message permissions come from `getMessageProperties`, so edit is refused when Telegram says no and delete uses `can_be_deleted_for_all_users` instead of guessing from the sender.
- Folders can be renamed through `getChatFolder` plus `editChatFolder`.
- Albums: picking several files at once sends them as one `sendMessageAlbum`, photos with real dimensions and the composer text as the caption of the first item.
- Profile fields: username (`setUsername`) and birthdate (`setBirthdate`) validated by pure helpers with twenty-three tests.
- Proxy manager: list configured proxies, enable or remove one by number (`getProxies`, `enableProxy`, `removeProxy`); all unassigned by default.
- Topic history now goes through `getForumTopicHistory`; topics can be renamed (`editForumTopic`) and their mention/reaction badges cleared; polls accept a vote through `setPollAnswer`.
- Topic lifecycle: close/reopen, delete and pin/unpin, all unassigned by default.
- Member unban and proxy disable are available next to their counterparts.
- The anti-pattern example in the style guide no longer contains a marker token.
- Text formatting: `**bold**`, `__bold__`, `_italic_` and backticks become Telegram entities; the parser is pure and covered by twelve tests.
- Forum topics UI: a forum chat opens with its topic list, `Enter` opens a topic, `Backspace` returns, `Ctrl+Alt+N` creates one, and sending inside a topic passes `MessageTopicForum`.
- Forum topics: the service loads topics with unread, pin and closed state plus the last message id, and reads history inside a topic through getMessageThreadHistory; ordering and unread aggregation live in TopicSorter with six tests.
- Multi-account: each account has its own TDLib directory, switching recreates the client in place, the active account is remembered in SQLite; ids and paths are covered by eight tests.
- Eight rare commands (topics, stories, proxy, bio, scope mutes, account management) ship without a default shortcut and are assigned in the editor.
- Drafts: the composer text is saved with `setChatDraftMessage` after a pause, restored from `chat.draftMessage` on open and cleared on send. The draft also carries the reply target, is capped at 4096 characters, and is not applied over text the user already typed — the same three rules Unigram uses in `DialogViewModel`. DraftPolicy is covered by thirteen tests.
- Chat folders: create with `Ctrl+Alt+F` and delete the current one with `Ctrl+Alt+D`; name preparation and case-insensitive uniqueness are covered by seven tests.
- Member moderation: `Ctrl+Alt+B` lists members with numbers and bans the chosen one through `setChatMemberStatus`; index parsing is covered by eleven tests.
- Notification scopes: mute private chats, groups and channels for N hours through get/setScopeNotificationSettings, with hour conversion tested.
- Active stories of a chat are listed with dates.
- Invite links: create with a lifetime and list the existing ones, with day-to-unix conversion tested.
- Chat renaming with `F2` through `setChatTitle`, title normalization covered by five tests.
- Bot buttons: `Ctrl+B` lists inline keyboard labels and presses one by number through `getCallbackQueryAnswer`; cursor arithmetic is covered by six tests.
- SOCKS5 proxy configuration through `addProxy`, parsed by a Core helper with nine tests.
- Profile editing: name and bio through `setName` and `setBio`, with the name split covered by six tests.
- Jump to date with `Ctrl+G` through `getChatMessageByDate`; the query parser is pure and tested.
- Multi-select: `Space` toggles, `Ctrl+A` selects all loaded, delete and forward act on the selection.
- Settings window: language, api_id, api_hash, phone, sign out (`Ctrl+Comma`).
- Localization tests: Russian must cover every English key, no empty values, no duplicate keys, every shipped string resolved.
- Pinned message jump (`Ctrl+Shift+P`), delete for me only (`Ctrl+Shift+D`) and pin/unpin toggle.
- Speech announcements through Prism (`Prismatoid`) behind `ISpeechService`.
- Localization with English as the neutral resource and Russian as a satellite; error
  messages and plurals are resource-driven.
- Strict build: nullable, warnings as errors, NetAnalyzers plus Meziantou, Roslynator and
  Sonar analyzers, code style enforced in build.
- 34 unit tests on Microsoft.Testing.Platform with xUnit v3 and Shouldly.

### Not implemented yet

- Calls only.

## [0.1.0] - 2026-11-04

First commit: repository scaffold, strict build settings, documentation set.
