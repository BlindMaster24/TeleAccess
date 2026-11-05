using Shouldly;

namespace TeleAccess.Core.Tests;

public class CommandTitleTests
{
    [Fact]
    public void Falls_back_to_the_static_title()
    {
        var command = new TeleCommand
        {
            Id = "chatList.pin",
            Title = "Pin chat",
            Scope = CommandScope.ChatList
        };

        command.DisplayTitle.ShouldBe("Pin chat");
    }

    [Fact]
    public void Uses_the_provider_when_present()
    {
        var pinned = false;

        var command = new TeleCommand
        {
            Id = "chatList.pin",
            Title = "Pin chat",
            Scope = CommandScope.ChatList,
            TitleProvider = () => pinned ? "Unpin chat" : "Pin chat"
        };

        command.DisplayTitle.ShouldBe("Pin chat");
        pinned = true;
        command.DisplayTitle.ShouldBe("Unpin chat");
    }

    [Fact]
    public void Provider_does_not_change_the_stable_id_or_title()
    {
        var command = new TeleCommand
        {
            Id = "chatList.pin",
            Title = "Pin chat",
            Scope = CommandScope.ChatList,
            TitleProvider = () => "Unpin chat"
        };

        command.Id.ShouldBe("chatList.pin");
        command.Title.ShouldBe("Pin chat");
        command.DisplayTitle.ShouldBe("Unpin chat");
    }
}
