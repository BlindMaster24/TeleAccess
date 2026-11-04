using System.IO;
using Shouldly;
using TeleAccess.App.Commands;

namespace TeleAccess.Core.Tests;

public class CommandCatalogTests
{
    private static CommandRegistry Build()
    {
        var noop = () => { };
        var actions = new UiActions(
            HasChat: () => true,
            HasMessage: () => true,
            FocusChats: noop,
            FocusMessages: noop,
            FocusComposer: noop,
            FocusSearch: noop,
            Search: noop,
            Refresh: noop,
            Hotkeys: noop,
            Quit: noop,
            SearchGlobal: noop,
            ChatInfo: noop,
            ChatMembers: noop,
            ChatTopics: noop,
            ChatJoin: noop,
            Contacts: noop,
            Settings: noop,
            SecretChat: noop,
            Training: noop,
            SetName: noop,
            SetBio: noop,
            SetUsername: noop,
            SetBirthdate: noop,
            Proxy: noop,
            MuteScopes: noop,
            Stories: noop,
            AddAccount: noop,
            SwitchAccount: noop,
            RemoveAccount: noop,
            UnbanMember: noop,
            DisableProxy: noop,
            EnableProxy: noop,
            RemoveProxy: noop,
            Chat: new ChatActions(noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop),
            Message: new MessageActions(noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop, noop),
            Composer: new ComposerActions(noop, noop, noop, noop, noop, noop, noop, noop, noop));

        return CommandCatalog.Create(actions);
    }

    [Fact]
    public void Default_keymap_matches_the_documented_snapshot()
    {
        var registry = Build();

        var actual = string.Join(
            Environment.NewLine,
            registry.Commands.Select(c => $"{c.Id} | {c.Scope} | {c.Hotkey ?? "none"}"));

        var expected = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Snapshots", "DefaultKeymap.txt"))
            .ReplaceLineEndings()
            .TrimEnd();

        actual.ShouldBe(expected, "docs/HOTKEYS.md and CommandCatalog must agree; update both together.");
    }

    [Fact]
    public void Every_command_has_a_localized_title()
    {
        foreach (var command in Build().Commands)
        {
            command.Title.ShouldNotBeNullOrWhiteSpace($"command {command.Id} has no title");
        }
    }

    [Fact]
    public void No_duplicate_ids()
    {
        var registry = Build();
        var duplicates = registry.Commands.GroupBy(c => c.Id, StringComparer.Ordinal).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();

        duplicates.ShouldBeEmpty();
    }

    [Fact]
    public void No_conflicts_inside_a_scope()
    {
        var registry = Build();

        var conflicts = registry.Commands
            .Where(c => c.Hotkey is not null)
            .GroupBy(c => c.Scope + "|" + c.Hotkey, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key + " used by " + g.Select(c => c.Id).Aggregate((a, b) => a + ", " + b))
            .ToArray();

        conflicts.ShouldBeEmpty();
    }

    [Fact]
    public void Every_command_can_be_found_by_id()
    {
        var registry = Build();

        foreach (var command in registry.Commands)
        {
            registry.Find(command.Id).ShouldNotBeNull();
        }
    }
}
