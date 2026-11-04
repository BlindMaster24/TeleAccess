using Shouldly;

namespace TeleAccess.Core.Tests;

public class CommandRegistryTests
{
    private static CommandRegistry Build(params (string Id, CommandScope Scope, string? Hotkey)[] items)
    {
        var registry = new CommandRegistry();

        foreach (var (id, scope, hotkey) in items)
        {
            registry.Add(new TeleCommand
            {
                Id = id,
                Title = id,
                Scope = scope,
                DefaultHotkey = hotkey
            });
        }

        return registry;
    }

    [Fact]
    public void Rejects_duplicate_ids()
    {
        var registry = Build(("message.delete", CommandScope.Message, "Ctrl+D"));

        Should.Throw<InvalidOperationException>(() => registry.Add(new TeleCommand
        {
            Id = "message.delete",
            Title = "again",
            Scope = CommandScope.Message
        }));
    }

    [Fact]
    public void Normalizes_default_hotkey_on_add()
    {
        var registry = Build(("message.delete", CommandScope.Message, "ctrl+d"));

        registry.Find("message.delete")!.Hotkey.ShouldBe("Ctrl+D");
    }

    [Fact]
    public void Refuses_conflict_inside_one_scope()
    {
        var registry = Build(
            ("message.delete", CommandScope.Message, "Ctrl+D"),
            ("message.edit", CommandScope.Message, "Ctrl+E"));

        var result = registry.TrySetHotkey("message.edit", "Ctrl+D", out var conflict);

        result.ShouldBe(SetHotkeyResult.Conflict);
        conflict!.Id.ShouldBe("message.delete");
        registry.Find("message.edit")!.Hotkey.ShouldBe("Ctrl+E");
    }

    [Fact]
    public void Allows_the_same_combination_in_different_scopes()
    {
        var registry = Build(
            ("message.delete", CommandScope.Message, "Ctrl+D"),
            ("chat.delete", CommandScope.ChatList, "Ctrl+E"));

        var result = registry.TrySetHotkey("chat.delete", "Ctrl+D", out _);

        result.ShouldBe(SetHotkeyResult.Ok);
        registry.Find("chat.delete")!.Hotkey.ShouldBe("Ctrl+D");
    }

    [Fact]
    public void Resolves_inside_scope_before_global()
    {
        var registry = Build(
            ("global.search", CommandScope.Global, "Ctrl+K"),
            ("message.search", CommandScope.Message, "Ctrl+K"));

        HotkeyCombination.TryParse("Ctrl+K", out var combination).ShouldBeTrue();

        registry.Resolve(CommandScope.Message, combination)!.Id.ShouldBe("message.search");
    }

    [Fact]
    public void Falls_back_to_global_command()
    {
        var registry = Build(("global.search", CommandScope.Global, "Ctrl+K"));

        HotkeyCombination.TryParse("Ctrl+K", out var combination).ShouldBeTrue();

        registry.Resolve(CommandScope.MessageList, combination)!.Id.ShouldBe("global.search");
    }

    [Fact]
    public void Returns_null_when_nothing_matches()
    {
        var registry = Build(("global.search", CommandScope.Global, "Ctrl+K"));

        HotkeyCombination.TryParse("Ctrl+J", out var combination).ShouldBeTrue();

        registry.Resolve(CommandScope.Global, combination).ShouldBeNull();
    }

    [Fact]
    public void Reset_restores_the_default()
    {
        var registry = Build(("message.delete", CommandScope.Message, "Ctrl+D"));
        registry.TrySetHotkey("message.delete", "Ctrl+Shift+D", out _).ShouldBe(SetHotkeyResult.Ok);

        registry.ResetCommand("message.delete");

        registry.Find("message.delete")!.Hotkey.ShouldBe("Ctrl+D");
    }

    [Fact]
    public void Reset_all_restores_every_default()
    {
        var registry = Build(
            ("message.delete", CommandScope.Message, "Ctrl+D"),
            ("message.edit", CommandScope.Message, "Ctrl+E"));

        registry.TrySetHotkey("message.delete", "Ctrl+1", out _);
        registry.TrySetHotkey("message.edit", "Ctrl+2", out _);

        registry.ResetAll();

        registry.Find("message.delete")!.Hotkey.ShouldBe("Ctrl+D");
        registry.Find("message.edit")!.Hotkey.ShouldBe("Ctrl+E");
    }

    [Fact]
    public void Clears_a_hotkey()
    {
        var registry = Build(("message.delete", CommandScope.Message, "Ctrl+D"));

        registry.TrySetHotkey("message.delete", null, out _).ShouldBe(SetHotkeyResult.Cleared);

        registry.Find("message.delete")!.Hotkey.ShouldBeNull();
    }

    [Fact]
    public void Rejects_unknown_command_and_invalid_hotkey()
    {
        var registry = Build(("message.delete", CommandScope.Message, "Ctrl+D"));

        registry.TrySetHotkey("nope", "Ctrl+D", out _).ShouldBe(SetHotkeyResult.UnknownCommand);
        registry.TrySetHotkey("message.delete", "Hyper+D", out _).ShouldBe(SetHotkeyResult.InvalidHotkey);
    }
}
