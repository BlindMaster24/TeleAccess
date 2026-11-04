# TeleAccess

A Telegram client for Windows built for blind and low-vision users. Every action is
reachable from the keyboard, spoken through the screen reader, and identical in the
menu, the context menu and the assigned shortcut.

Status: early development. Sign-in, chat list, message list and the command system are
implemented; voice messages, files and calls are not.

## What makes it different

- **One command registry.** Menus, context menus and shortcuts are generated from the
  same list, so a button without a shortcut cannot exist.
- **Rebindable everything.** The shortcut editor is fully keyboard driven and refuses
  conflicts out loud instead of silently overwriting.
- **Speech first.** Announcements go through the screen reader and through Prism
  (`Prismatoid`), which reaches NVDA, SAPI, Speech Dispatcher and others.
- **Portable.** One self-contained executable, no installer, no MSIX, no runtime
  prerequisites.

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

## License

Not chosen yet. Third-party licenses: TDLib (Boost), tdsharp (MIT), Prismatoid
(MPL-2.0).
