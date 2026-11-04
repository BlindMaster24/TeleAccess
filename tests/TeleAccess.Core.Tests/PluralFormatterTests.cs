using System.Globalization;
using Shouldly;

namespace TeleAccess.Core.Tests;

public class PluralFormatterTests
{
    private const string English = "{0:plural:{} unread message|{} unread messages}";
    private const string Russian = "{0:plural:{} непрочитанное сообщение|{} непрочитанных сообщения|{} непрочитанных сообщений}";

    [Theory]
    [InlineData(1, "1 unread message")]
    [InlineData(2, "2 unread messages")]
    [InlineData(11, "11 unread messages")]
    public void Formats_english_plurals(int count, string expected)
    {
        PluralFormatter.Format(English, count, CultureInfo.GetCultureInfo("en")).ShouldBe(expected);
    }

    [Theory]
    [InlineData(1, "1 непрочитанное сообщение")]
    [InlineData(3, "3 непрочитанных сообщения")]
    [InlineData(5, "5 непрочитанных сообщений")]
    [InlineData(21, "21 непрочитанное сообщение")]
    public void Formats_russian_plurals(int count, string expected)
    {
        PluralFormatter.Format(Russian, count, CultureInfo.GetCultureInfo("ru")).ShouldBe(expected);
    }
}
