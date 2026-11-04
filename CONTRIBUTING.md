# Contributing

TeleAccess is a Telegram client for blind users on Windows. Every contribution is judged
by one question: does it make the client work better with a screen reader?

## Before you start

1. `dotnet build` and `dotnet test` must pass before and after your change.
2. Read [AGENTS.md](AGENTS.md) for the rules that apply to every change.
3. Read [docs/HOTKEYS.md](docs/HOTKEYS.md): new actions need a command, a menu entry and
   a default shortcut.

## Making a change

- Write the failing test first when an honest test exists.
- Keep one public type per file: the build fails otherwise.
- No user-visible string in code. Add it to `src/TeleAccess.App/Localization/Strings.resx`
  and to `Strings.ru.resx`.
- Comments only in English, only when the code cannot say it.
- Update the affected document in the same commit.

## Accessibility check

Before opening a pull request:

1. Run the app with NVDA.
2. Sign in, open a chat, send a message.
3. Change a shortcut and confirm the change survives a restart.
4. Confirm every focus stop is announced with a meaningful name.

## Commits

Conventional commits in English, one logical change each:

```
feat(core): add per-scope conflict detection
fix(app): announce unavailable commands instead of staying silent
docs: describe the portable build
```

The body says why, what changed, and how you proved it.
