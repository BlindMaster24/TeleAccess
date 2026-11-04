# Code style

## Comments

Sources: Clean Code (comment rules), Microsoft C# XML documentation guidance.

Rules:

1. English only.
2. Explain intent, not mechanics. If the code says it, do not repeat it.
3. A comment is justified by one of: a workaround for an external bug, a non-obvious
   constraint, a unit or format, a deliberate deviation from the obvious approach.
4. One line, at most 80 characters. Longer means the code needs a better name or a
   smaller function.
5. No commented-out code, no changelog comments, no author or date tags, no closing
   brace comments.
6. Public API: one-sentence `<summary>`. `<remarks>` only for surprising behavior.
7. Keep the comment next to the reason. When the reason disappears, the comment goes
   with it.

Real examples from this repository:

```csharp
// tdsharp renames the Messages.Messages property to Messages_ to avoid a name clash.
var messages = history.Messages_;
```

```csharp
// CsWinRT cannot project this type, so the generated name carries a trailing underscore.
```

Anti-patterns:

```csharp
// Increments the counter.
counter++;
```

```csharp
// Calls the service.
_service.DoWork();
```

## Style

- One public type per file, file named after the type.
- `TeleAccess.Core` stays free of WPF and TDLib.
- Prefer early returns; no nesting deeper than three levels.
- No user-visible strings in code: use `Localization/Strings.resx`.
- Async methods end in `Async`; do not fire and forget without a reason.
- Public members get XML `<summary>` in English.
