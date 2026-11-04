using System.IO;
using Shouldly;
using TeleAccess.App.Localization;
using TeleAccess.App.Services;

namespace TeleAccess.Core.Tests;

public class PresentationTests
{
    [Fact]
    public void Message_row_shows_sender_when_present()
    {
        var row = new MessageRow(1, 2, "Anna", MessageKind.Text, "hello", "10:30", false, false, 0);

        row.Display.ShouldBe("Anna, 10:30. hello");
    }

    [Fact]
    public void Message_row_omits_sender_when_empty()
    {
        var row = new MessageRow(1, 2, string.Empty, MessageKind.Text, "hello", "10:30", true, false, 0);

        row.Display.ShouldBe("10:30. hello");
    }

    [Fact]
    public void Chat_row_shows_unread_count()
    {
        var row = new ChatRow(1, "Work", MessageKind.Text, "hi", 3, false, false, false);

        row.Display.ShouldBe("Work (3)");
    }

    [Fact]
    public void Chat_row_hides_zero_unread()
    {
        var row = new ChatRow(1, "Work", MessageKind.Text, "hi", 0, true, true, false);

        row.Display.ShouldBe("Work");
    }

    [Fact]
    public void Credentials_prefer_configured_settings()
    {
        var settings = new AppSettings { ApiId = 42, ApiHash = "hash" };

        var (apiId, apiHash) = TelegramCredentials.Resolve(settings);

        apiId.ShouldBe(42);
        apiHash.ShouldBe("hash");
    }

    [Fact]
    public void Credentials_fall_back_to_compiled_defaults()
    {
        var (apiId, apiHash) = TelegramCredentials.Resolve(new AppSettings());

        apiId.ShouldBe(TelegramCredentials.DefaultApiId);
        apiHash.ShouldBe(TelegramCredentials.DefaultApiHash);
    }

    [Fact]
    public void Errors_map_timeouts_to_the_network_message()
    {
        LocalizationManager.Apply("en");

        Errors.Describe(new TimeoutException()).ShouldBe(Strings.Error_Network);
    }

    [Fact]
    public void Errors_keep_the_message_for_unknown_failures()
    {
        LocalizationManager.Apply("en");

        Errors.Describe(new InvalidOperationException("boom")).ShouldContain("boom");
    }

    [Fact]
    public void Errors_map_io_failures_to_the_file_message()
    {
        LocalizationManager.Apply("en");

        Errors.Describe(new IOException()).ShouldBe(Strings.Error_File);
    }
}
