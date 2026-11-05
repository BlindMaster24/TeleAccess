namespace TeleAccess.Core;

public sealed class TeleCommand
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public required CommandScope Scope { get; init; }

    public string? DefaultHotkey { get; init; }

    public string? Hotkey { get; internal set; }

    /// <summary>Builds a label from the current state, for toggling commands.</summary>
    public Func<string>? TitleProvider { get; init; }

    public Func<bool>? CanExecute { get; init; }

    public Action? Execute { get; init; }

    public bool IsAvailable => CanExecute?.Invoke() ?? true;

    public string DisplayTitle => TitleProvider?.Invoke() ?? Title;

    internal void ResetToDefault() => Hotkey = HotkeyCombination.Normalize(DefaultHotkey);
}
