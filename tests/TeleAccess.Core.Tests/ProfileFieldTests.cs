using Shouldly;
using TeleAccess.Core.Profiles;

namespace TeleAccess.Core.Tests;

public class ProfileFieldTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 4, 12, 0, 0, TimeSpan.FromHours(3));

    [Theory]
    [InlineData("durov", "durov")]
    [InlineData(" @tele_access_1 ", "tele_access_1")]
    [InlineData("abcde", "abcde")]
    [InlineData("Abc_123", "Abc_123")]
    public void Accepts_valid_usernames(string input, string expected)
    {
        Username.TryNormalize(input, out var username).ShouldBeTrue();
        username.ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abcd")]
    [InlineData("1abcd")]
    [InlineData("_abcd")]
    [InlineData("ab-cd")]
    [InlineData("ab cd")]
    [InlineData("abcdefghijabcdefghijabcdefghijabc")]
    public void Rejects_invalid_usernames(string input)
    {
        Username.TryNormalize(input, out _).ShouldBeFalse();
    }

    [Fact]
    public void Rejects_null_username()
    {
        Username.TryNormalize(null, out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("1990-05-17", 17, 5, 1990)]
    [InlineData("17.05.1990", 17, 5, 1990)]
    [InlineData("01/01/2000", 1, 1, 2000)]
    public void Parses_birthdates(string input, int day, int month, int year)
    {
        Birthdate.TryParse(input, _now, out var birthdate).ShouldBeTrue();
        birthdate.ShouldBe(new BirthdateValue(day, month, year));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("tomorrow")]
    [InlineData("1899-12-31")]
    [InlineData("2030-01-01")]
    [InlineData("1990-13-01")]
    public void Rejects_invalid_birthdates(string input)
    {
        Birthdate.TryParse(input, _now, out _).ShouldBeFalse();
    }

    [Fact]
    public void Rejects_null_birthdate()
    {
        Birthdate.TryParse(null, _now, out _).ShouldBeFalse();
    }
}
