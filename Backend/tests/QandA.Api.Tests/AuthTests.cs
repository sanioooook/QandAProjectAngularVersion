using System.Net;
using Microsoft.EntityFrameworkCore;
using QandA.Api.Auth;
using QandA.Api.Tests.Infrastructure;

namespace QandA.Api.Tests;

/// <summary>Accounts with email delivery switched off (the default setup).</summary>
public class AuthTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private ApiClient Anonymous() => ApiClient.Anonymous(factory);

    [Fact]
    public async Task Register_signs_the_user_in()
    {
        var email = ApiClient.UniqueEmail("alice");
        var client = Anonymous();

        var created = await (await client.PostAsync("/api/auth/register", ApiClient.Registration(email, "  Alice   Smith ")))
            .ReadAs<AccountDto>(HttpStatusCode.Created);

        Assert.Equal(email, created.Email);
        Assert.Equal("Alice Smith", created.DisplayName);
        Assert.False(created.EmailConfirmed);
        Assert.Equal(created, await client.MeAsync());
    }

    [Fact]
    public async Task Without_email_delivery_nothing_needs_confirming()
    {
        var config = await Anonymous().GetJsonAsync<AuthConfigDto>("/api/auth/config");
        var client = await ApiClient.SignedUpAsync(factory);

        var survey = await client.CreatePublishedAsync();

        Assert.Equal(new AuthConfigDto(EmailEnabled: false, ConfirmationRequired: false), config);
        Assert.True(survey.CanVote);
    }

    [Fact]
    public async Task Email_only_flows_are_unavailable_without_email_delivery()
    {
        var client = await ApiClient.SignedUpAsync(factory);

        await (await Anonymous().PostAsync("/api/auth/forgot-password", new { email = ApiClient.UniqueEmail() }))
            .ShouldFailWith(HttpStatusCode.Conflict, "email_disabled");
        await (await client.PostAsync("/api/auth/resend-confirmation")).ShouldFailWith(HttpStatusCode.Conflict, "email_disabled");
    }

    [Fact]
    public async Task Password_is_stored_as_a_hash_not_as_plain_text()
    {
        var email = ApiClient.UniqueEmail();
        await ApiClient.SignedUpAsync(factory, email);

        var hash = await factory.WithDbAsync(db => db.Users.Where(u => u.Email == email).Select(u => u.PasswordHash).SingleAsync());

        Assert.DoesNotContain(ApiClient.Password, hash);
        Assert.True(hash.Length > 40);
    }

    [Fact]
    public async Task Auth_cookie_is_http_only_and_same_site()
    {
        var response = await Anonymous().PostAsync("/api/auth/register", ApiClient.Registration());

        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"), c => c.StartsWith("qanda.auth="));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_rejects_taken_email_ignoring_case()
    {
        var email = ApiClient.UniqueEmail("bob");
        await ApiClient.SignedUpAsync(factory, email);

        var response = await Anonymous().PostAsync("/api/auth/register", ApiClient.Registration(email.ToUpperInvariant()));

        await response.ShouldFailWith(HttpStatusCode.Conflict, "email_taken");
    }

    [Theory]
    [InlineData("not-an-email", "Alice", "Secret123", "email_invalid")]
    [InlineData("two@@example.com", "Alice", "Secret123", "email_invalid")]
    [InlineData("no-domain-dot@example", "Alice", "Secret123", "email_invalid")]
    [InlineData("spaces in@example.com", "Alice", "Secret123", "email_invalid")]
    [InlineData(null, "Alice", "Secret123", "email_invalid")]
    [InlineData("ok@example.com", "A", "Secret123", "name_length")]
    [InlineData("ok@example.com", "   ", "Secret123", "name_length")]
    [InlineData("ok@example.com", "Alice", "Short1", "password_length")]
    [InlineData("ok@example.com", "Alice", "onlyletters", "password_weak")]
    [InlineData("ok@example.com", "Alice", "1234567890", "password_weak")]
    [InlineData("alice2026@example.com", "Alice", "ALICE2026", "password_equals_email")]
    public async Task Register_rejects_invalid_input(string? email, string name, string password, string code)
    {
        var response = await Anonymous().PostAsync("/api/auth/register", new { email, displayName = name, password });

        await response.ShouldFailWith(HttpStatusCode.BadRequest, code);
    }

    [Fact]
    public async Task Login_ignores_case_and_surrounding_spaces_of_the_email()
    {
        var email = ApiClient.UniqueEmail("carol");
        await ApiClient.SignedUpAsync(factory, email);
        var client = Anonymous();

        var account = await (await client.PostAsync("/api/auth/login", new { email = $"  {email.ToUpperInvariant()} ", password = ApiClient.Password }))
            .ReadAs<AccountDto>();

        Assert.Equal(email, account.Email);
        Assert.Equal(email, (await client.MeAsync()).Email);
    }

    [Fact]
    public async Task Login_with_wrong_password_and_unknown_email_fail_the_same_way()
    {
        var email = ApiClient.UniqueEmail();
        await ApiClient.SignedUpAsync(factory, email);

        var wrongPassword = await Anonymous().PostAsync("/api/auth/login", new { email, password = "Wrong12345" });
        var unknownEmail = await Anonymous().PostAsync("/api/auth/login", new { email = ApiClient.UniqueEmail(), password = ApiClient.Password });

        await wrongPassword.ShouldFailWith(HttpStatusCode.Unauthorized, "invalid_credentials");
        await unknownEmail.ShouldFailWith(HttpStatusCode.Unauthorized, "invalid_credentials");
    }

    [Fact]
    public async Task Login_without_credentials_is_a_validation_error()
    {
        var response = await Anonymous().PostAsync("/api/auth/login", new { email = "", password = "" });

        await response.ShouldFailWith(HttpStatusCode.BadRequest, "credentials_required");
    }

    [Fact]
    public async Task Logout_ends_the_session()
    {
        var client = await ApiClient.SignedUpAsync(factory);

        await (await client.PostAsync("/api/auth/logout")).ShouldBe(HttpStatusCode.NoContent);

        await (await client.GetAsync("/api/auth/me")).ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Changing_the_password_keeps_this_session_and_ends_the_others()
    {
        var email = ApiClient.UniqueEmail();
        var thisDevice = await ApiClient.SignedUpAsync(factory, email);
        var otherDevice = Anonymous();
        await (await otherDevice.PostAsync("/api/auth/login", new { email, password = ApiClient.Password })).ShouldBe(HttpStatusCode.OK);

        await (await thisDevice.PostAsync("/api/auth/change-password", new { currentPassword = ApiClient.Password, newPassword = "NewSecret456" }))
            .ShouldBe(HttpStatusCode.OK);

        await (await thisDevice.GetAsync("/api/auth/me")).ShouldBe(HttpStatusCode.OK);
        await (await otherDevice.GetAsync("/api/auth/me")).ShouldBe(HttpStatusCode.Unauthorized);
        await (await Anonymous().PostAsync("/api/auth/login", new { email, password = ApiClient.Password }))
            .ShouldFailWith(HttpStatusCode.Unauthorized, "invalid_credentials");
        await (await Anonymous().PostAsync("/api/auth/login", new { email, password = "NewSecret456" })).ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Changing_the_password_needs_the_current_one_and_a_valid_new_one()
    {
        var client = await ApiClient.SignedUpAsync(factory);

        await (await client.PostAsync("/api/auth/change-password", new { currentPassword = "Wrong12345", newPassword = "NewSecret456" }))
            .ShouldFailWith(HttpStatusCode.BadRequest, "invalid_current_password");
        await (await client.PostAsync("/api/auth/change-password", new { currentPassword = ApiClient.Password, newPassword = "weak" }))
            .ShouldFailWith(HttpStatusCode.BadRequest, "password_length");
    }

    [Theory]
    [InlineData("/api/auth/me")]
    [InlineData("/api/surveys?scope=mine")]
    [InlineData("/api/surveys?scope=voted")]
    public async Task Personal_endpoints_return_401_to_guests_without_redirecting(string url)
    {
        var response = await Anonymous().GetAsync(url);

        await response.ShouldBe(HttpStatusCode.Unauthorized);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task Forged_cookie_is_rejected()
    {
        var client = Anonymous();
        client.Http.DefaultRequestHeaders.Add("Cookie", "qanda.auth=forged-value");

        await (await client.GetAsync("/api/auth/me")).ShouldBe(HttpStatusCode.Unauthorized);
    }
}

public class AuthRateLimitTests(ThrottledApiFactory factory) : IClassFixture<ThrottledApiFactory>
{
    [Fact]
    public async Task Too_many_login_attempts_are_throttled()
    {
        var client = ApiClient.Anonymous(factory);
        var attempt = new { email = "someone@example.com", password = "Guess1234" };
        for (var i = 0; i < ThrottledApiFactory.Limit; i++)
            await (await client.PostAsync("/api/auth/login", attempt)).ShouldBe(HttpStatusCode.Unauthorized);

        var throttled = await client.PostAsync("/api/auth/login", attempt);

        await throttled.ShouldBe(HttpStatusCode.TooManyRequests);
    }
}
