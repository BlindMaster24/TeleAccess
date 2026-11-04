using Shouldly;
using TeleAccess.Core.Network;

namespace TeleAccess.Core.Tests;

public class ProxyUriTests
{
    [Theory]
    [InlineData("socks5://127.0.0.1:1080", "127.0.0.1", 1080, "", "")]
    [InlineData("SOCKS5://proxy.example:9050", "proxy.example", 9050, "", "")]
    [InlineData("socks5://user:secret@127.0.0.1:1080", "127.0.0.1", 1080, "user", "secret")]
    [InlineData("127.0.0.1", "127.0.0.1", 1080, "", "")]
    [InlineData("socks://127.0.0.1:1", "127.0.0.1", 1, "", "")]
    public void Parses_socks_proxies(string text, string host, int port, string user, string password)
    {
        ProxyUri.TryParse(text, out var proxy).ShouldBeTrue();
        proxy.Host.ShouldBe(host);
        proxy.Port.ShouldBe(port);
        proxy.Username.ShouldBe(user);
        proxy.Password.ShouldBe(password);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("http://127.0.0.1:8080")]
    [InlineData("socks5://host:0")]
    [InlineData("socks5://host:70000")]
    [InlineData("socks5://@:1080")]
    public void Rejects_invalid_uris(string text)
    {
        ProxyUri.TryParse(text, out _).ShouldBeFalse();
    }

    [Fact]
    public void Rejects_null()
    {
        ProxyUri.TryParse(null, out _).ShouldBeFalse();
    }
}
