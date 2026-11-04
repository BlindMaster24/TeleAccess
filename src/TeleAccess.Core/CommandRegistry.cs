namespace TeleAccess.Core;

public sealed class CommandRegistry
{
    private readonly List<TeleCommand> _commands = [];
    private readonly Dictionary<string, TeleCommand> _byId = new(StringComparer.Ordinal);

    public IReadOnlyList<TeleCommand> Commands => _commands;

    public void Add(TeleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!_byId.TryAdd(command.Id, command))
        {
            throw new InvalidOperationException($"Command '{command.Id}' is already registered.");
        }

        command.ResetToDefault();
        _commands.Add(command);
    }

    public TeleCommand? Find(string id) => _byId.GetValueOrDefault(id);

    public TeleCommand? Resolve(CommandScope scope, HotkeyCombination combination)
    {
        var match = FindInScope(scope, combination);
        if (match is not null || scope == CommandScope.Global)
        {
            return match;
        }

        return FindInScope(CommandScope.Global, combination);
    }

    public SetHotkeyResult TrySetHotkey(string id, string? hotkey, out TeleCommand? conflict)
    {
        conflict = null;

        var command = Find(id);
        if (command is null)
        {
            return SetHotkeyResult.UnknownCommand;
        }

        if (string.IsNullOrWhiteSpace(hotkey))
        {
            command.Hotkey = null;
            return SetHotkeyResult.Cleared;
        }

        if (!HotkeyCombination.TryParse(hotkey, out var parsed))
        {
            return SetHotkeyResult.InvalidHotkey;
        }

        var taken = FindInScope(command.Scope, parsed);

        if (taken is not null && !ReferenceEquals(taken, command))
        {
            conflict = taken;
            return SetHotkeyResult.Conflict;
        }

        command.Hotkey = parsed.ToString();
        return SetHotkeyResult.Ok;
    }

    public void ResetCommand(string id) => Find(id)?.ResetToDefault();

    public void ResetAll()
    {
        foreach (var command in _commands)
        {
            command.ResetToDefault();
        }
    }

    private TeleCommand? FindInScope(CommandScope scope, HotkeyCombination combination) =>
        _commands.FirstOrDefault(c =>
            c.Scope == scope &&
            c.Hotkey is not null &&
            HotkeyCombination.TryParse(c.Hotkey, out var own) &&
            own.IsSameAs(combination));
}
