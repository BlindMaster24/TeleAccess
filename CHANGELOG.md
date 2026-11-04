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
  (`Ctrl+Shift+Enter`) through a window with quick presets, a calendar picker and a
  manual parser for relative, time-only and absolute input.
- Scheduled queue, search inside a chat, reactions, chat mute, pin and archive.
- File attachments: `Ctrl+O` sends any local file as a document, with the composer text as caption.
- Voice messages: WASAPI capture, Opus encoding through Concentus, OGG muxing, sent as `inputMessageVoiceNote`; `Ctrl+Shift+V` starts and stops recording.
- OGG/Opus container writer with the OpusHead/OpusTags headers and per-page CRC.
- Audio files (mp3, m4a, flac, aac, wav, ogg, opus) are sent as audio messages, everything else as documents.
- Chat lists cycle with `Ctrl+Shift+L`: main, every Telegram folder, archive.
- Photos (with real dimensions), contacts and locations can be sent; the typing action is reported to the other side.
- Message translation (`Ctrl+Shift+T`) and search across all chats (`Ctrl+Shift+F`).
- Pinned message jump (`Ctrl+Shift+P`), delete for me only (`Ctrl+Shift+D`) and pin/unpin toggle.
- Speech announcements through Prism (`Prismatoid`) behind `ISpeechService`.
- Localization with English as the neutral resource and Russian as a satellite; error
  messages and plurals are resource-driven.
- Strict build: nullable, warnings as errors, NetAnalyzers plus Meziantou, Roslynator and
  Sonar analyzers, code style enforced in build.
- 34 unit tests on Microsoft.Testing.Platform with xUnit v3 and Shouldly.

### Not implemented yet

- Calls, video, animation, stickers, polls, forums, secret chat UI.

## [0.1.0] - 2026-11-04

First commit: repository scaffold, strict build settings, documentation set.
