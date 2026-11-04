# Research: why this stack

Written before the first line of code, based on the TDLib schema, the Microsoft
documentation and the existing clients, not on memory.

## 1. The niche is already occupied

| Project | Platform | What it offers | Source |
| --- | --- | --- | --- |
| **TAccess** (`mlapps88/t-access-accessible-telegram-client`) | Windows 10/11 | Portable client built for NVDA, JAWS and Narrator; global search, hotkeys | GitHub |
| **Unigram** + NVDA add-on | Windows | Full client; the add-on fills in missing control names | Open |
| **Telegram Desktop** | Windows | Initial accessibility support merged 2025-10-22 (PR #29808); add-ons `keyang556/tdesktopnvda`, `JustiCode/TelegramJusti` | Open |
| Zendalona Accessible Telegram Desktop | Windows/Linux | Patches the official client for screen readers | Open |
| Zerogram / AccessibleGram | iOS | VoiceOver first | — |
| Telelight / blindGram | Android | TalkBack, OCR for photos | — |

Conclusion: a Windows client only makes sense with a deliberate difference — language,
workflow, or a feature the others get wrong. "Another accessible Telegram" is not enough.

## 2. TDLib is mandatory

Telegram Database Library handles the network, encryption, caching and updates. Writing
MTProto by hand is both a security risk and a ban risk.

- `.NET`: NuGet `TDLib` (egramtel/tdsharp) 1.8.69, actively tracks upstream; native
  binaries come from `tdlib.native`. API is generated from the schema, targets .NET
  Standard 2.0, therefore compatible with .NET 10.
- `Rust`: `tdlib-rs` works, but the first build compiles C++ TDLib (CMake, OpenSSL) on
  every machine.
- Both ecosystems can use TDLib, so TDLib itself does not decide the argument.

## 3. What decides it is UI Automation

A screen reader never looks at pixels. It walks the UI Automation tree: name, role and
value. Anything that does not expose a proper tree does not exist for a blind user.

| Option | UIA | Risks |
| --- | --- | --- |
| **WPF** (.NET 10) | Automation peers built into every standard control; the most exercised path for NVDA and JAWS | dated look, fixed by the .NET 9+ Fluent theme; explicit `AutomationProperties.Name` still required |
| **WinUI 3** | UIA exists but has known holes: JAWS skips complex `ListViewItem` (#8109), panes read as "DesktopChildSiteBridge" (#7853), custom title bar breaks in scan mode (#10668) | MSIX or Windows App SDK runtime; portability is possible (`WindowsPackageType=None` + self-contained) but is extra work |
| **Avalonia** | UIA exists formally; accessibility documentation is thin; the AoT build once broke Narrator (#18272) | not for a project whose only goal is accessibility |
| **Tauri/Rust** (WebView2) | HTML semantics read well while they work | open regression #15624: after losing and regaining focus the WebView2 content becomes unreadable and keyboard-unreachable; #12901: NVDA silent in a frameless window |

## 4. Decision

**TDLib + .NET 10 + WPF.**

1. WPF has the most predictable accessibility on Windows.
2. A single self-contained `.exe`: no MSIX, no WebView2 runtime, fewer ways to break.
3. .NET bindings for TDLib are current (1.8.69); the Rust path requires a C++ build.
4. Tauri would add difficulty exactly where mistakes cost the most.

WinUI 3 remains a deliberate alternative: modern look, but explicit names on every
element, no custom title bar, flat `ListViewItem`s and a portable unpackaged config.

## 5. Is WPF going away? Checked against Microsoft documentation

- WPF migration docs, verbatim: "WPF is supported on .NET and receives **active
  investment**, including performance improvements, **accessibility updates**, and new
  features."
- .NET 10 is LTS, released 2025-11-11, supported until **2028-11-15**. WPF received
  Fluent theme, performance and clipboard work; .NET 11 RC1 continues Fluent fixes.
- WPF is open source (dotnet/wpf, MIT) and ships with every .NET release.

Meanwhile the "newest" Windows UI stack has its own graveyard: UWP is in maintenance
mode, WinUI 2 is dead, Silverlight is dead. WinUI 3 / Windows App SDK 2.6.x is what
Microsoft recommends for new applications today, and that is fine — but "newer" and
"long-lived" are different axes.

## 6. Sources

- TDLib: <https://github.com/tdlib/td>
- .NET bindings: <https://github.com/egramtel/tdsharp>
- Rust bindings: <https://github.com/FedericoBruzzone/tdlib-rs>
- .NET support policy: <https://dotnet.microsoft.com/platform/support/policy>
- WPF migration and investment: <https://learn.microsoft.com/dotnet/desktop/wpf/migration/>
- What's new in WPF for .NET 10: <https://learn.microsoft.com/dotnet/desktop/wpf/whats-new/net100>
- WinUI 3: <https://learn.microsoft.com/windows/apps/winui/winui3/>
- Tauri/WebView2 accessibility regression: <https://github.com/tauri-apps/tauri/issues/15624>
- Telegram Desktop accessibility PR: <https://github.com/telegramdesktop/tdesktop/pull/29808>
- TAccess: <https://github.com/mlapps88/t-access-accessible-telegram-client>
