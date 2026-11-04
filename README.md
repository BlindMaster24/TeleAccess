# TeleAccess

**TeleAccess is a Telegram client for blind users.**

Not a general-purpose messenger with an accessibility checkbox: the whole interface is
designed for people who work with a screen reader, from the first keystroke. Every
action is reachable without a mouse, spoken out loud, and identical in the menu, the
context menu and the keyboard shortcut. Nothing is hidden behind a hover, a drag, or an
icon that has no name.

## Who this is for

- Blind users who drive Windows with NVDA, JAWS or Narrator.

That is the whole audience. TeleAccess is only for blind users. If you see well and want
a regular Telegram client, Telegram Desktop or Unigram will serve you better; the
interface here is built around speech and keystrokes, not around pixels.

## What makes it different

- **One command registry.** Menus, context menus and shortcuts are generated from the
  same list, so a button without a shortcut cannot exist.
- **Rebindable everything.** The shortcut editor is fully keyboard driven and refuses
  conflicts out loud instead of silently overwriting.
- **Speech first.** Announcements go through the screen reader and through Prism
  (`Prismatoid`), which reaches NVDA, SAPI, Speech Dispatcher and others.
- **Portable.** One self-contained executable, no installer, no MSIX, no runtime
  prerequisites.

## Status

Early development. Implemented: sign-in with api_id/api_hash, chat list, message list,
sending (normal, silent and scheduled), voice messages, file attachments, audio, reply,
edit, delete, forward, pin, copy, reactions, search inside a chat, scheduled messages,
chat deletion, mute, pin, archive, Telegram folders, and the full command and shortcut
system, photos, video, animation, stickers, polls, contacts, locations, translation and
global search, chat information, member lists, forum topic lists, contact lists, secret
chat creation, joining chats, incoming voice and audio playback, attachment download,
history paging, new-message announcements with an earcon, training mode, and a settings
window (language, credentials, sign out). Not implemented yet: calls.

## Build

```shell
dotnet build
dotnet test
```

## Requirements

- Windows 10 or later
- .NET 10 SDK
- `api_id` and `api_hash` from <https://my.telegram.org/apps>, entered on first run

## Documentation

| Document | Contents |
| --- | --- |
| [AGENTS.md](AGENTS.md) | instructions for coding agents |
| [docs/RESEARCH.md](docs/RESEARCH.md) | why TDLib + WPF and what the alternatives cost |
| [docs/API-INVENTORY.md](docs/API-INVENTORY.md) | what TDLib can and cannot do |
| [docs/HOTKEYS.md](docs/HOTKEYS.md) | command scopes and the default keymap |
| [docs/CODE-STYLE.md](docs/CODE-STYLE.md) | comment policy |
| [docs/PACKAGING.md](docs/PACKAGING.md) | portable build and installer plan |
| [docs/CI.md](docs/CI.md) | workflows, gates and artifacts |
| [docs/TOPICS.md](docs/TOPICS.md) | forum topics research and plan |
| [docs/TESTING.md](docs/TESTING.md) | test stack, coverage and mutation status |

## License

Not chosen yet. Third-party licenses: TDLib (Boost), tdsharp (MIT), Prismatoid
(MPL-2.0).
