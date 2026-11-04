using Shouldly;
using TeleAccess.Core.Hotkeys;

namespace TeleAccess.Core.Tests;

public class VirtualKeysTests
{
    [Theory]
    [InlineData("A", 0x41)]
    [InlineData("v", 0x56)]
    [InlineData("0", 0x30)]
    [InlineData("9", 0x39)]
    [InlineData("F1", 0x70)]
    [InlineData("F12", 0x7B)]
    [InlineData("f24", 0x87)]
    [InlineData("Enter", 0x0D)]
    [InlineData("Esc", 0x1B)]
    [InlineData("Space", 0x20)]
    [InlineData("PageUp", 0x21)]
    [InlineData("Down", 0x28)]
    [InlineData(",", 0xBC)]
    [InlineData(".", 0xBE)]
    public void Maps_known_keys(string key, int expected)
    {
        VirtualKeys.TryMap(key, out var virtualKey).ShouldBeTrue();
        virtualKey.ShouldBe((ushort)expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Hyper")]
    [InlineData("F25")]
    [InlineData("F0")]
    [InlineData("!!")]
    public void Rejects_unknown_keys(string key)
    {
        VirtualKeys.TryMap(key, out _).ShouldBeFalse();
    }
}
