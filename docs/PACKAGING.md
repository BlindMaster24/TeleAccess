# Packaging: portable build and installer

Two artifacts ship from one codebase. The portable one comes first because it is the
smallest promise we can keep: a single file that runs on a clean Windows 10 machine
without installing anything.

## Portable

```shell
dotnet publish src/TeleAccess.App -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Result: `TeleAccess.App.exe`, about 78 MB, self-contained. Verified on this machine:
the publish directory contains the executable and PDB files only.

What the build does for us:

| Concern | How it is handled |
| --- | --- |
| .NET runtime on the user machine | bundled (`--self-contained true`) |
| TDLib native library | `tdlib.native` ships `runtimes/<rid>/native/tdjson.dll`; `RuntimeIdentifiers` in the project lets MSBuild pick the right one |
| Prism native library | `Prismatoid` ships `libprism` per RID, same mechanism |
| Extraction of native libraries | `IncludeNativeLibrariesForSelfExtract` when `PublishSingleFile` is set |
| ARM64 | `-r win-arm64` produces the same single file |

Rules:

1. Never require MSIX and never require the Windows App SDK runtime.
2. Publish both `win-x64` and `win-arm64` for a release.
3. Keep the file name stable: `TeleAccess.exe` once the assembly is renamed.

## Where user data lives

| Data | Location |
| --- | --- |
| Settings and keymap | `%APPDATA%\TeleAccess\teleaccess.db` (SQLite) |
| TDLib database and files | `%LOCALAPPDATA%\TeleAccess\td` |

Both directories are created on first run. A portable copy therefore keeps its data
across updates, and deleting the folder is a full reset.

## Installer

Only if a real need appears. When it does:

- **Tooling:** Inno Setup 6 (small, scriptable, per-user install without admin) or WiX 5
  (MSI, for managed deployment). No MSIX: it conflicts with the portable promise.
- **What the installer does:** copies the same self-contained files into
  `%LOCALAPPDATA%\Programs\TeleAccess`, creates a Start menu shortcut, registers an
  uninstall entry, and offers an optional startup shortcut.
- **What it must not do:** ask for administrator rights unnecessarily, install a runtime,
  or delete `%APPDATA%\TeleAccess` on uninstall without asking.
- **Accessibility of the installer:** silent-by-default wizard, every control labelled,
  keyboard-only navigation, and the uninstall dialog announces what will be removed.

## Release checklist

1. `dotnet build` and `dotnet test` pass.
2. `dotnet list package --outdated` shows no updates left.
3. Publish `win-x64` and `win-arm64`, run each on a clean profile.
4. Check with NVDA: sign-in, chat list, send a message, change a shortcut.
5. Tag the release and attach both executables.
