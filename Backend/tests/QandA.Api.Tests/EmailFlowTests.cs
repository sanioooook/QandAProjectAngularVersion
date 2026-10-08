using System.Net;
using QandA.Api.Auth;
using QandA.Api.Surveys;
using QandA.Api.Tests.Infrastructure;

namespace QandA.Api.Tests;

/// <summary>Accounts with email delivery configured: confirmation, password reset, notifications.</summary>
public class EmailFlowTests(EmailApiFactory factory) : IClassFixture<EmailApiFactory>
{
    private ApiClient Anonymous() => ApiClient.Anonymous(factory);

    private async Task<(ApiClient Client, string Email)> RegisterAsync(string? locale = null)
    {
        var email = ApiClient.UniqueEmail();
        var client = Anonymous();
        await (await client.PostAsync("/api/auth/register", ApiClient.Registration(email, "Dana", locale: locale))).ShouldBe(HttpStatusCode.Created);
        return (client, email);
    }

    [Fact]
    public async Task Registration_sends_a_confirmation_email_in_the_chosen_language()
    {
        var (_, email) = await RegisterAsync("en");

        var message = Assert.Single(factory.Outbox.To(email));

        Assert.Equal("Confirm your email for QandA", message.Subject);
        Assert.Contains("http://qanda.test/confirm-email?token=", message.Text);
        Assert.Contains("Hi, Dana!", message.Text);
    }

    [Fact]
    public async Task Unconfirmed_users_can_read_but_not_take_part()
    {
        var author = await RegisterAndConfirmAsync();
        var survey = await author.CreatePublishedAsync();
        var (newcomer, _) = await RegisterAsync();

        var asNewcomer = await newcomer.GetSurveyAsync(survey.Id);
        var vote = await newcomer.PutAsync($"/api/surveys/{survey.Id}/votes", new { optionIds = new[] { survey.Options[0].Id } });
        var create = await newcomer.PostAsync("/api/surveys", new { title = "Q", options = new[] { "A", "B" } });

        Assert.False(asNewcomer.CanVote);
        await vote.ShouldFailWith(HttpStatusCode.Forbidden, "email_not_confirmed");
        await create.ShouldFailWith(HttpStatusCode.Forbidden, "email_not_confirmed");
    }

    [Fact]
    public async Task Confirming_the_email_unlocks_voting_and_the_link_works_once()
    {
        var (client, email) = await RegisterAsync();
        var token = factory.Outbox.LastToken(email, "confirm-email");

        await (await Anonymous().PostAsync("/api/auth/confirm-email", new { token })).ShouldBe(HttpStatusCode.NoContent);
        var reused = await Anonymous().PostAsync("/api/auth/confirm-email", new { token });

        Assert.True((await client.MeAsync()).EmailConfirmed);
        await reused.ShouldFailWith(HttpStatusCode.BadRequest, "token_invalid");
        Assert.NotNull(await client.CreatePublishedAsync());
    }

    [Fact]
    public async Task Resending_the_confirmation_invalidates_the_previous_link()
    {
        var (client, email) = await RegisterAsync();
        var first = factory.Outbox.LastToken(email, "confirm-email");

        await (await client.PostAsync("/api/auth/resend-confirmation")).ShouldBe(HttpStatusCode.NoContent);
        var second = factory.Outbox.LastToken(email, "confirm-email");

        Assert.NotEqual(first, second);
        await (await Anonymous().PostAsync("/api/auth/confirm-email", new { token = first })).ShouldFailWith(HttpStatusCode.BadRequest, "token_invalid");
        await (await Anonymous().PostAsync("/api/auth/confirm-email", new { token = second })).ShouldBe(HttpStatusCode.NoContent);
        await (await client.PostAsync("/api/auth/resend-confirmation")).ShouldFailWith(HttpStatusCode.Conflict, "already_confirmed");
    }

    [Fact]
    public async Task Confirmation_link_expires_after_three_days()
    {
        var (_, email) = await RegisterAsync();
        var token = factory.Outbox.LastToken(email, "confirm-email");

        factory.Clock.Advance(TokenService.Lifetime(Domain.TokenPurpose.EmailConfirmation) + TimeSpan.FromSeconds(1));

        await (await Anonymous().PostAsync("/api/auth/confirm-email", new { token })).ShouldFailWith(HttpStatusCode.BadRequest, "token_invalid");
    }

    [Theory]
    [InlineData("")]
    [InlineData("garbage")]
    [InlineData(null)]
    public async Task Malformed_tokens_are_rejected(string? token)
    {
        await (await Anonymous().PostAsync("/api/auth/confirm-email", new { token })).ShouldFailWith(HttpStatusCode.BadRequest, "token_invalid");
        await (await Anonymous().PostAsync("/api/auth/reset-password", new { token, password = "NewSecret456" }))
            .ShouldFailWith(HttpStatusCode.BadRequest, "token_invalid");
    }

    [Fact]
    public async Task Password_reset_sets_a_new_password_signs_in_and_ends_old_sessions()
    {
        var (oldSession, email) = await RegisterAsync();
        var browser = Anonymous();

        await (await browser.PostAsync("/api/auth/forgot-password", new { email, locale = "ru" })).ShouldBe(HttpStatusCode.NoContent);
        var resetMail = factory.Outbox.To(email).Last();
        var token = factory.Outbox.LastToken(email, "reset-password");
        var account = await (await browser.PostAsync("/api/auth/reset-password", new { token, password = "NewSecret456" })).ReadAs<AccountDto>();

        Assert.Equal("Сброс пароля в QandA", resetMail.Subject);
        Assert.True(account.EmailConfirmed); // following the link proves the address
        Assert.Equal(email, (await browser.MeAsync()).Email);
        await (await oldSession.GetAsync("/api/auth/me")).ShouldBe(HttpStatusCode.Unauthorized);
        Assert.Contains(factory.Outbox.To(email), m => m.Subject == "Пароль у QandA змінено");
        await (await Anonymous().PostAsync("/api/auth/reset-password", new { token, password = "Another789" }))
            .ShouldFailWith(HttpStatusCode.BadRequest, "token_invalid");
    }

    [Fact]
    public async Task Reset_link_expires_after_an_hour_and_a_weak_password_does_not_burn_it()
    {
        var (_, email) = await RegisterAsync();
        await Anonymous().PostAsync("/api/auth/forgot-password", new { email });
        var token = factory.Outbox.LastToken(email, "reset-password");

        await (await Anonymous().PostAsync("/api/auth/reset-password", new { token, password = "weak" }))
            .ShouldFailWith(HttpStatusCode.BadRequest, "password_length");
        factory.Clock.Advance(TimeSpan.FromMinutes(61));

        await (await Anonymous().PostAsync("/api/auth/reset-password", new { token, password = "NewSecret456" }))
            .ShouldFailWith(HttpStatusCode.BadRequest, "token_invalid");
    }

    [Fact]
    public async Task Forgot_password_does_not_reveal_whether_the_email_is_registered()
    {
        var unknown = ApiClient.UniqueEmail();

        var response = await Anonymous().PostAsync("/api/auth/forgot-password", new { email = unknown });

        await response.ShouldBe(HttpStatusCode.NoContent);
        Assert.Empty(factory.Outbox.To(unknown));
    }

    [Fact]
    public async Task Emails_follow_the_language_saved_on_the_account()
    {
        var (client, email) = await RegisterAsync("uk");
        Assert.Equal("Підтвердіть email у QandA", factory.Outbox.To(email).Single().Subject);

        await (await client.PutAsync("/api/account/profile", new { locale = "en" })).ShouldBe(HttpStatusCode.OK);
        await (await client.PostAsync("/api/auth/resend-confirmation")).ShouldBe(HttpStatusCode.NoContent);
        await (await client.PostAsync("/api/auth/change-password", new { currentPassword = ApiClient.Password, newPassword = "NewSecret456" }))
            .ShouldBe(HttpStatusCode.OK);
        // No language in the request: the account's language is used.
        await (await Anonymous().PostAsync("/api/auth/forgot-password", new { email })).ShouldBe(HttpStatusCode.NoContent);

        Assert.Equal(
            ["Підтвердіть email у QandA", "Confirm your email for QandA", "Your QandA password was changed", "Reset your QandA password"],
            factory.Outbox.To(email).Select(m => m.Subject));
    }

    [Fact]
    public async Task Email_config_is_advertised_to_the_client()
    {
        var config = await Anonymous().GetJsonAsync<AuthConfigDto>("/api/auth/config");

        Assert.Equal(new AuthConfigDto(EmailEnabled: true, ConfirmationRequired: true), config);
    }

    private async Task<ApiClient> RegisterAndConfirmAsync()
    {
        var (client, email) = await RegisterAsync();
        await Anonymous().PostAsync("/api/auth/confirm-email", new { token = factory.Outbox.LastToken(email, "confirm-email") });
        return client;
    }
}
