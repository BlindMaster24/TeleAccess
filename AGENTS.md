# AGENTS.md

TeleAccess is a **Telegram client for blind users** on Windows. This is not a general
messenger with an accessibility option: the interface is designed for people who work
with a screen reader. Every action must be reachable without a mouse, announced aloud,
and identical in the menu, the context menu and the keyboard shortcut. TeleAccess is only
for blind users; sighted users are not the audience and features are not designed for them.

## Stack

| Area | Choice |
| --- | --- |
| Telegram | TDLib via NuGet `TDLib` (tdsharp) + `tdlib.native`, both 1.8.69 |
| UI | WPF on .NET 10 (`net10.0-windows`) |
| Speech | `Prismatoid` 0.4.0 (Prism) behind `ISpeechService`, native library per RID |
| Tests | xUnit v3 (`xunit.v3` 4.0.2) on Microsoft.Testing.Platform + `Shouldly` 4.3.0 |
| Localization | `.resx` with English as the neutral resource |

Pin exact versions. Do not upgrade a package without running the full test suite.

## Commands

```shell
dotnet build                                    # whole solution
dotnet test                                     # all tests
dotnet test tests/TeleAccess.Core.Tests         # one project
dotnet format                                   # style gate
dotnet publish src/TeleAccess.App -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The publish line above is the portable artifact: one self-contained `.exe`, no MSIX,
no Windows App SDK runtime, no installer required.

## Layout

```
docs/                     architecture and research notes
src/TeleAccess.Core/      framework-free logic (commands, hotkeys, speech contracts)
src/TeleAccess.App/       WPF shell, TDLib service, resources
tests/TeleAccess.Core.Tests/
```

`TeleAccess.Core` must not reference WPF or TDLib. It is the tested part.

## Rules

### Accessibility

- Every interactive control gets an explicit `AutomationProperties.Name`.
- Dynamic announcements go through a live region with
  `AutomationProperties.LiveSetting` and through `ISpeechService`.
- A command that cannot run must say why; silence is a bug.
- Focus order follows the reading order; no focus traps.

### Commands, menus, hotkeys

- One action is one `TeleCommand`. Menus and context menus are generated from the
  registry, never written by hand.
- A shortcut conflict is only a conflict inside the same `CommandScope`. Assigning a
  taken combination is refused and spoken, never silently overwritten.
- `Escape` is reserved for cancel. `Shift+F10` opens the context menu.

### Localization

- No user-visible string in code. Everything goes to `Localization/Strings.resx`.
- `Strings.resx` is English and neutral; other languages are satellite files
  (`Strings.ru.resx`).
- The language list is ordered by English name and always contains English.
- Adding a language is: add a `.resx`, add one entry to `LocalizationManager` — nothing else.

### Comments

- English only.
- Write a comment only when the code cannot say it: a workaround for an external bug,
  a non-obvious constraint, a unit or format, a deliberate deviation.
- Never restate the code, never keep commented-out code, never use comments as a
  changelog.
- One line, at most 80 characters. If it needs more, rename or split the code instead.
- Public types and members get a one-sentence XML `<summary>`; add `<remarks>` only
  when behavior is genuinely surprising.

### Documentation upkeep

- Documentation is part of the change, not a follow-up. Any change to behavior,
  dependencies, commands, packaging or conventions updates the affected document in the
  same pull request.
- This file (AGENTS.md) is kept current: new rules, new commands, new directories and
  new dependencies are recorded here as they appear.
- New knowledge worth keeping (a TDLib quirk, a Windows bug, a decision with a reason)
  goes into a document under `docs/` and is linked from the table at the bottom.
- Documents are written in English only.
- A stale document is a bug: if it contradicts the code, fix it or delete it.

### Code style

- One decision lives in one place.
- Files are named after the thing they contain; one public type per file.
- Small functions, early returns, no deep nesting.
- Tests assert observable behavior, not private helpers.

### Security

- `api_id` and `api_hash` are never committed. They live in
  `%APPDATA%\TeleAccess\settings.json`.
- TDLib state stays in `%LOCALAPPDATA%\TeleAccess\td`.
- Never log message contents, phone numbers or codes.

## Definition of done

1. `dotnet build` and `dotnet test` pass.
2. New behavior has a test, or the pull request says why an honest test is impractical.
3. Every new action is in the command registry and therefore in the hotkey editor.
4. No new hardcoded UI string.
5. Comments, if any, are English and earn their place.

## Docs

- [docs/RESEARCH.md](docs/RESEARCH.md) — why this stack, what the alternatives cost
- [docs/API-INVENTORY.md](docs/API-INVENTORY.md) — what TDLib can and cannot do
- [docs/HOTKEYS.md](docs/HOTKEYS.md) — command scopes and the default keymap
- [docs/CODE-STYLE.md](docs/CODE-STYLE.md) — comment policy and style rules
- [docs/PACKAGING.md](docs/PACKAGING.md) — portable build and installer plan
- [docs/CI.md](docs/CI.md) — what every workflow proves and how to add one
- [docs/TESTING.md](docs/TESTING.md) — test stack, what is covered, mutation blocker
