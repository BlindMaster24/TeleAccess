namespace TeleAccess.Core;

public sealed class TeleCommand
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public required CommandScope Scope { get; init; }

    public string? DefaultHotkey { get; init; }

    public string? Hotkey { get; internal set; }

    public Func<bool>? CanExecute { get; init; }

    public Action? Execute { get; init; }

    public bool IsAvailable => CanExecute?.Invoke() ?? true;

    internal void ResetToDefault() => Hotkey = HotkeyCombination.Normalize(DefaultHotkey);
}
