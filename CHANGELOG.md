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
- Speech announcements through Prism (`Prismatoid`) behind `ISpeechService`.
- Localization with English as the neutral resource and Russian as a satellite; error
  messages and plurals are resource-driven.
- Strict build: nullable, warnings as errors, NetAnalyzers plus Meziantou, Roslynator and
  Sonar analyzers, code style enforced in build.
- 34 unit tests on Microsoft.Testing.Platform with xUnit v3 and Shouldly.

### Not implemented yet

- Voice messages, file attachments, folders, calls.

## [0.1.0] - 2026-11-04

First commit: repository scaffold, strict build settings, documentation set.
