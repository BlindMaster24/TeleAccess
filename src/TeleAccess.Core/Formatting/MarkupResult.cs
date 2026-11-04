namespace TeleAccess.Core.Formatting;

public sealed record MarkupResult(string Text, IReadOnlyList<EntitySpan> Entities);
