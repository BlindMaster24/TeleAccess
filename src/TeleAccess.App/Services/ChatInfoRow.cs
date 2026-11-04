namespace TeleAccess.App.Services;

public sealed record ChatInfoRow(string Title, string Kind, int MemberCount, string Description)
{
    public string Display => MemberCount > 0
        ? $"{Title}. {Kind}. {MemberCount}"
        : $"{Title}. {Kind}";
}
