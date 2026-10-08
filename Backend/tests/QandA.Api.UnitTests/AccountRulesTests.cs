using QandA.Api.Auth;
using QandA.Api.Common;

namespace QandA.Api.UnitTests;

public class AccountRulesTests
{
    private static string CodeOf(Action action) => Assert.Throws<AppException>(action).Code;

    [Theory]
    [InlineData("alice@example.com")]
    [InlineData("  Alice.Smith+polls@Example.co.uk  ")]
    [InlineData("о.коваль@приклад.укр")]
    public void Valid_emails_are_trimmed_and_normalized(string raw)
    {
        var (email, normalized) = AccountRules.Email(raw);

        Assert.Equal(raw.Trim(), email);
        Assert.Equal(raw.Trim().ToUpperInvariant(), normalized);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("plainaddress")]
    [InlineData("@example.com")]
    [InlineData("alice@")]
    [InlineData("alice@example")]
    [InlineData("alice@example.")]
    [InlineData("a b@example.com")]
    [InlineData("alice@@example.com")]
    public void Invalid_emails_are_rejected(string? raw) =>
        Assert.Equal("email_invalid", CodeOf(() => AccountRules.Email(raw)));

    [Fact]
    public void An_email_longer_than_the_limit_is_rejected()
    {
        var email = new string('a', 250) + "@x.io";

        Assert.Equal("email_invalid", CodeOf(() => AccountRules.Email(email)));
    }

    [Theory]
    [InlineData("  Alice   Smith ", "Alice Smith")]
    [InlineData("Jo", "Jo")]
    [InlineData("Олена\tКоваль", "Олена Коваль")]
    public void Display_names_collapse_whitespace(string raw, string expected) =>
        Assert.Equal(expected, AccountRules.DisplayName(raw));

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("J")]
    [InlineData("  J  ")]
    public void Too_short_names_are_rejected(string? raw) =>
        Assert.Equal("name_length", CodeOf(() => AccountRules.DisplayName(raw)));

    [Fact]
    public void Too_long_names_are_rejected() =>
        Assert.Equal("name_length", CodeOf(() => AccountRules.DisplayName(new string('x', 51))));

    [Theory]
    [InlineData("Secret12")]
    [InlineData("пароль123")]
    [InlineData("Very long passphrase with 1 digit")]
    public void Good_passwords_pass(string password) => AccountRules.Password(password, "alice@example.com");

    [Theory]
    [InlineData(null, "password_length")]
    [InlineData("Abc123", "password_length")]
    [InlineData("abcdefgh", "password_weak")]
    [InlineData("12345678", "password_weak")]
    public void Weak_passwords_are_rejected(string? password, string code) =>
        Assert.Equal(code, CodeOf(() => AccountRules.Password(password, "alice@example.com")));

    [Theory]
    [InlineData("alice2026@example.com", "alice2026")]
    [InlineData("alice2026@example.com", "ALICE2026@EXAMPLE.COM")]
    public void Passwords_equal_to_the_email_or_its_name_part_are_rejected(string email, string password) =>
        Assert.Equal("password_equals_email", CodeOf(() => AccountRules.Password(password, email)));

    [Fact]
    public void The_field_name_is_reported_for_the_form() =>
        Assert.Equal("newPassword", Assert.Throws<AppException>(() => AccountRules.Password("short", "a@b.cd", "newPassword")).Field);

    [Theory]
    [InlineData("en", "en")]
    [InlineData(" RU ", "ru")]
    [InlineData("de", "uk")]
    [InlineData(null, "uk")]
    public void Unknown_languages_fall_back_to_ukrainian(string? raw, string expected) =>
        Assert.Equal(expected, AccountRules.Locale(raw));
}
