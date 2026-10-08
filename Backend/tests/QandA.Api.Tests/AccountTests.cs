using System.Net;
using System.Net.Http.Headers;
using QandA.Api.Auth;
using QandA.Api.Surveys;
using QandA.Api.Tests.Infrastructure;

namespace QandA.Api.Tests;

public class AccountTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    // Smallest valid files of each kind, enough for the signature check.
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 13, 0x49, 0x48, 0x44, 0x52];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0, 16, 0x4A, 0x46, 0x49, 0x46];
    private static readonly byte[] Webp = [.. "RIFF"u8, 0x24, 0, 0, 0, .. "WEBPVP8 "u8];

    private static async Task<HttpResponseMessage> UploadAsync(ApiClient client, byte[] data, string declaredType = "image/png")
    {
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(data);
        file.Headers.ContentType = new MediaTypeHeaderValue(declaredType);
        form.Add(file, "file", "avatar.png");
        return await client.Http.PutAsync("/api/account/avatar", form, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Profile_update_changes_name_and_language_independently()
    {
        var client = await ApiClient.SignedUpAsync(factory, name: "Old Name");

        var renamed = await (await client.PutAsync("/api/account/profile", new { displayName = "  New   Name " })).ReadAs<AccountDto>();
        var relocalized = await (await client.PutAsync("/api/account/profile", new { locale = "EN" })).ReadAs<AccountDto>();

        Assert.Equal("New Name", renamed.DisplayName);
        Assert.Equal("uk", renamed.Locale);
        Assert.Equal("New Name", relocalized.DisplayName);
        Assert.Equal("en", relocalized.Locale);
    }

    [Fact]
    public async Task Profile_update_validates_the_name_and_ignores_unknown_languages()
    {
        var client = await ApiClient.SignedUpAsync(factory);

        await (await client.PutAsync("/api/account/profile", new { displayName = "x" })).ShouldFailWith(HttpStatusCode.BadRequest, "name_length");
        var account = await (await client.PutAsync("/api/account/profile", new { locale = "klingon" })).ReadAs<AccountDto>();

        Assert.Equal("uk", account.Locale);
    }

    [Theory]
    [InlineData("png", "image/png")]
    [InlineData("jpeg", "image/jpeg")]
    [InlineData("webp", "image/webp")]
    public async Task Avatar_is_stored_and_served_publicly_with_the_detected_type(string kind, string expectedType)
    {
        var data = kind switch { "png" => Png, "jpeg" => Jpeg, _ => Webp };
        var client = await ApiClient.SignedUpAsync(factory);

        // The declared type lies on purpose: the signature decides.
        var account = await (await UploadAsync(client, data, declaredType: "text/plain")).ReadAs<AccountDto>();
        var served = await ApiClient.Anonymous(factory).GetAsync(account.AvatarUrl!);

        await served.ShouldBe(HttpStatusCode.OK);
        Assert.Equal(expectedType, served.Content.Headers.ContentType!.MediaType);
        Assert.Equal(data, await served.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
        Assert.Contains("immutable", served.Headers.CacheControl!.ToString());
    }

    [Fact]
    public async Task Replacing_the_avatar_changes_its_url_and_removing_it_clears_it()
    {
        var client = await ApiClient.SignedUpAsync(factory);
        var first = await (await UploadAsync(client, Png)).ReadAs<AccountDto>();
        factory.Clock.Advance(TimeSpan.FromSeconds(1));

        var second = await (await UploadAsync(client, Jpeg)).ReadAs<AccountDto>();
        var removed = await (await client.DeleteAsync("/api/account/avatar")).ReadAs<AccountDto>();

        Assert.NotEqual(first.AvatarUrl, second.AvatarUrl);
        Assert.Null(removed.AvatarUrl);
        await (await client.GetAsync(second.AvatarUrl!)).ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Files_that_are_not_images_are_rejected_whatever_they_claim()
    {
        var client = await ApiClient.SignedUpAsync(factory);

        await (await UploadAsync(client, "<svg onload=alert(1)>"u8.ToArray(), "image/png")).ShouldFailWith(HttpStatusCode.BadRequest, "avatar_invalid");
        await (await UploadAsync(client, "GIF89a"u8.ToArray(), "image/gif")).ShouldFailWith(HttpStatusCode.BadRequest, "avatar_invalid");
        await (await UploadAsync(client, [], "image/png")).ShouldFailWith(HttpStatusCode.BadRequest, "avatar_invalid");
    }

    [Fact]
    public async Task Oversized_avatars_are_rejected()
    {
        var client = await ApiClient.SignedUpAsync(factory);
        var big = new byte[Domain.Limits.AvatarMaxBytes + 1];
        Png.CopyTo(big, 0);

        var response = await UploadAsync(client, big);

        await response.ShouldFailWith(HttpStatusCode.BadRequest, "avatar_too_large");
    }

    [Fact]
    public async Task Survey_authors_and_voters_carry_their_avatar()
    {
        var author = await ApiClient.SignedUpAsync(factory);
        var voter = await ApiClient.SignedUpAsync(factory);
        var authorAccount = await (await UploadAsync(author, Png)).ReadAs<AccountDto>();
        var voterAccount = await (await UploadAsync(voter, Jpeg)).ReadAs<AccountDto>();
        var survey = await author.CreatePublishedAsync();
        await voter.PutAsync($"/api/surveys/{survey.Id}/votes", new { optionIds = new[] { survey.Options[0].Id } });

        var details = await author.GetSurveyAsync(survey.Id);
        var listed = await author.GetJsonAsync<Paged<SurveySummary>>("/api/surveys?scope=mine");

        Assert.Equal(authorAccount.AvatarUrl, details.Author.AvatarUrl);
        Assert.Equal(voterAccount.AvatarUrl, Assert.Single(details.Options[0].Voters!).AvatarUrl);
        Assert.Equal(authorAccount.AvatarUrl, listed.Items.Single(s => s.Id == survey.Id).Author.AvatarUrl);
    }

    [Fact]
    public async Task Guests_cannot_change_profiles()
    {
        var guest = ApiClient.Anonymous(factory);

        await (await guest.PutAsync("/api/account/profile", new { displayName = "Hacker" })).ShouldBe(HttpStatusCode.Unauthorized);
        await (await UploadAsync(guest, Png)).ShouldBe(HttpStatusCode.Unauthorized);
        await (await guest.GetAsync("/api/users/999999/avatar")).ShouldBe(HttpStatusCode.NotFound);
    }
}
