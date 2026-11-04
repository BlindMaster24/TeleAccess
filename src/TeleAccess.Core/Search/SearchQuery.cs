namespace TeleAccess.Core.Search;

public sealed record SearchQuery(string Text, SearchFilterKind Filter, int MinDate, int MaxDate)
{
    public static SearchQuery Empty { get; } = new(string.Empty, SearchFilterKind.Any, 0, 0);
}
