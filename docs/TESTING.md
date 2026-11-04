# Testing

## Stack

| Piece | Choice | Why |
| --- | --- | --- |
| Framework | **xUnit v3** 4.0.2 | current major, runs on Microsoft.Testing.Platform, parallel by default |
| Runner | **Microsoft.Testing.Platform** 2.5.x | the .NET 10 replacement for VSTest; `global.json` opts in via `"test": { "runner": "Microsoft.Testing.Platform" }` |
| Assertions | **Shouldly** 4.3.0 | readable failure messages, MIT (FluentAssertions moved to a paid licence) |
| Coverage | **Microsoft.Testing.Extensions.CodeCoverage** 18.12.0 | Cobertura output straight from the runner |
| Reports | **Microsoft.Testing.Extensions.TrxReport** 2.5.2 | TRX artifacts that CI and IDEs can read |
| Snapshots | plain text file + comparison | no extra dependency for one snapshot; Verify was not worth it yet |

Deliberately not added: Moq (SponsorLink), FluentAssertions (licence), NSubstitute (no
interface worth mocking yet), Bogus (no random data needed yet). Add them when a real test
needs them, not before.

## What is tested

- `TeleAccess.Core` behaviour: hotkey parsing and normalization, per-scope conflict
  detection, scope resolution with global fallback, reset and clear, SQLite persistence of
  settings and keymap.
- The real command catalog from the app: the default keymap is compared against
  `tests/TeleAccess.Core.Tests/Snapshots/DefaultKeymap.txt`, so a shortcut cannot drift
  away from `docs/HOTKEYS.md` unnoticed. Duplicate ids, conflicts inside a scope and
  missing titles fail the build.
- Localized plurals for English and Russian (two and three forms respectively).

Not tested, and honestly so: the WPF windows and the TDLib network layer. Both need a UI
harness or a live account; a fake would test the fake. They are covered by the manual
NVDA checklist in [CONTRIBUTING.md](../CONTRIBUTING.md).

## Commands

```shell
dotnet test                                                  # everything
dotnet test -- --coverage --coverage-output-format cobertura  # with coverage
dotnet format --verify-no-changes                            # style gate
```

## Mutation testing: blocked by tooling

Stryker.NET 5.0.0 cannot run this suite. It drives tests through VSTest, and
Microsoft.Testing.Platform — the runner xUnit v3 uses on .NET 10 — is not supported yet
(stryker-mutator/stryker-net#3094). A run reports "Number of tests found: 0" and aborts.

Two honest options when mutation coverage becomes important:

1. Wait for MTP support in Stryker and add `.github/workflows/mutation.yml` then.
2. Add a second, VSTest-based test project (NUnit or xUnit v2) that references the same
   `TeleAccess.Core`. Duplicated harness, no duplicated tests, and mutation works today.

Until one of those happens, nothing claims mutation coverage that does not exist.
