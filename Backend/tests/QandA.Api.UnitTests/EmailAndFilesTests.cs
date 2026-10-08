using QandA.Api.Account;
using QandA.Api.Auth;
using QandA.Api.Email;

namespace QandA.Api.UnitTests;

public class EmailTemplatesTests
{
    private const string Link = "https://qanda.test/confirm-email?token=abc";

    public static TheoryData<string, string> Locales => new() { { "uk", "Привіт" }, { "en", "Hi" }, { "ru", "Привет" } };

    [Theory]
    [MemberData(nameof(Locales))]
    public void Every_email_exists_in_every_language_and_contains_the_link(string locale, string greeting)
    {
        EmailMessage[] messages =
        [
            EmailTemplates.Confirmation("a@b.cd", "Dana", Link, locale),
            EmailTemplates.PasswordReset("a@b.cd", "Dana", Link, locale),
            EmailTemplates.PasswordChanged("a@b.cd", "Dana", Link, locale),
        ];

        Assert.All(messages, m =>
        {
            Assert.Equal("a@b.cd", m.To);
            Assert.StartsWith($"{greeting}, Dana!", m.Text);
            Assert.Contains(Link, m.Text);
            Assert.Contains("href=\"https://qanda.test/confirm-email?token=abc\"", m.Html);
        });
        Assert.Equal(3, messages.Select(m => m.Subject).Distinct().Count());
    }

    [Fact]
    public void Unknown_language_falls_back_to_ukrainian() =>
        Assert.Equal("Підтвердіть email у QandA", EmailTemplates.Confirmation("a@b.cd", "Dana", Link, "de").Subject);

    [Fact]
    public void Names_are_html_encoded()
    {
        var html = EmailTemplates.Confirmation("a@b.cd", "<script>alert(1)</script>", Link, "en").Html;

        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
    }
}

public class TokenAndAvatarTests
{
    [Fact]
    public void Token_hash_is_stable_hex_sha256()
    {
        var hash = TokenService.Hash("token");

        Assert.Equal(64, hash.Length);
        Assert.Equal(hash, TokenService.Hash("token"));
        Assert.NotEqual(hash, TokenService.Hash("Token"));
        Assert.Matches("^[0-9a-f]+$", hash);
    }

    [Fact]
    public void Reset_links_live_shorter_than_confirmation_links() =>
        Assert.True(TokenService.Lifetime(Domain.TokenPurpose.PasswordReset) < TokenService.Lifetime(Domain.TokenPurpose.EmailConfirmation));

    [Theory]
    [InlineData(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0 }, "image/png")]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xDB }, "image/jpeg")]
    [InlineData(new byte[] { 0x52, 0x49, 0x46, 0x46, 1, 2, 3, 4, 0x57, 0x45, 0x42, 0x50 }, "image/webp")]
    [InlineData(new byte[] { 0x52, 0x49, 0x46, 0x46, 1, 2, 3, 4, 0x57, 0x41, 0x56, 0x45 }, null)] // RIFF WAVE audio
    [InlineData(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, null)] // GIF
    [InlineData(new byte[] { 0x3C, 0x73, 0x76, 0x67 }, null)] // "<svg"
    [InlineData(new byte[] { 0x89, 0x50, 0x4E }, null)] // truncated PNG
    [InlineData(new byte[0], null)]
    public void Image_type_comes_from_the_file_signature(byte[] data, string? expected) =>
        Assert.Equal(expected, Avatars.DetectImageType(data));

    [Fact]
    public void Avatar_url_is_versioned_by_its_update_time()
    {
        var at = new DateTimeOffset(2026, 10, 8, 12, 0, 0, TimeSpan.Zero);

        Assert.Null(Avatars.Url(7, null));
        Assert.Equal($"/api/users/7/avatar?v={at.ToUnixTimeMilliseconds()}", Avatars.Url(7, at));
        Assert.NotEqual(Avatars.Url(7, at), Avatars.Url(7, at.AddSeconds(1)));
    }
}
