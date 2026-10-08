using System.Net;
using QandA.Api.Surveys;
using QandA.Api.Tests.Infrastructure;

namespace QandA.Api.Tests;

public class SurveyTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private Task<ApiClient> NewUserAsync(string name = "Test User") => ApiClient.SignedUpAsync(factory, name: name);

    private static Task<Paged<SurveySummary>> ListAsync(ApiClient client, string query = "scope=active") =>
        client.GetJsonAsync<Paged<SurveySummary>>($"/api/surveys?{query}&pageSize=50");

    [Fact]
    public async Task Draft_is_private_to_its_author()
    {
        var author = await NewUserAsync();
        var other = await NewUserAsync();
        var draft = await author.CreateSurveyAsync(new { title = "Secret plan", options = new[] { "A", "B" } });

        Assert.Equal(SurveyStatus.Draft, draft.Status);
        await (await other.GetAsync($"/api/surveys/{draft.Id}")).ShouldFailWith(HttpStatusCode.NotFound, "not_found");
        Assert.DoesNotContain((await ListAsync(other)).Items, s => s.Id == draft.Id);
        Assert.Contains((await ListAsync(author, "scope=mine&status=draft")).Items, s => s.Id == draft.Id);
    }

    [Fact]
    public async Task Published_survey_collects_votes_and_lists_as_active()
    {
        var author = await NewUserAsync();
        var voter = await NewUserAsync();
        var survey = await author.CreatePublishedAsync();
        var park = survey.Options[1];

        var afterVote = await (await voter.PutAsync($"/api/surveys/{survey.Id}/votes", new { optionIds = new[] { park.Id } }))
            .ReadAs<SurveyDetails>();

        Assert.Equal([park.Id], afterVote.MyVotes);
        Assert.Equal(1, afterVote.Options.Single(o => o.Id == park.Id).Votes);
        Assert.Equal(1, afterVote.VoterCount);
        var listed = Assert.Single((await ListAsync(voter)).Items, s => s.Id == survey.Id);
        Assert.True(listed.HasVoted);
        Assert.Contains((await ListAsync(voter, "scope=voted")).Items, s => s.Id == survey.Id);
    }

    [Fact]
    public async Task Only_the_author_sees_who_voted()
    {
        var author = await NewUserAsync();
        var voter = await NewUserAsync();
        var voterName = (await voter.MeAsync()).DisplayName;
        var survey = await author.CreatePublishedAsync();
        await (await voter.PutAsync($"/api/surveys/{survey.Id}/votes", new { optionIds = new[] { survey.Options[0].Id } })).ShouldBe(HttpStatusCode.OK);

        var asVoter = await voter.GetSurveyAsync(survey.Id);
        var asAuthor = await author.GetSurveyAsync(survey.Id);

        Assert.All(asVoter.Options, o => Assert.Null(o.Voters));
        Assert.Equal(1, asVoter.Options[0].Votes);
        Assert.Equal(voterName, Assert.Single(asAuthor.Options[0].Voters!).Name);
    }

    [Fact]
    public async Task Voting_again_replaces_the_previous_vote()
    {
        var author = await NewUserAsync();
        var voter = await NewUserAsync();
        var survey = await author.CreatePublishedAsync();

        await voter.PutAsync($"/api/surveys/{survey.Id}/votes", new { optionIds = new[] { survey.Options[0].Id } });
        var result = await (await voter.PutAsync($"/api/surveys/{survey.Id}/votes", new { optionIds = new[] { survey.Options[2].Id } }))
            .ReadAs<SurveyDetails>();

        Assert.Equal(1, result.TotalVotes);
        Assert.Equal(0, result.Options[0].Votes);
        Assert.Equal(1, result.Options[2].Votes);
    }

    [Fact]
    public async Task Empty_vote_withdraws_the_vote()
    {
        var author = await NewUserAsync();
        var voter = await NewUserAsync();
        var survey = await author.CreatePublishedAsync();
        await voter.PutAsync($"/api/surveys/{survey.Id}/votes", new { optionIds = new[] { survey.Options[0].Id } });

        var result = await (await voter.PutAsync($"/api/surveys/{survey.Id}/votes", new { optionIds = Array.Empty<int>() }))
            .ReadAs<SurveyDetails>();

        Assert.Equal(0, result.TotalVotes);
        Assert.Empty(result.MyVotes);
    }

    [Fact]
    public async Task Cannot_vote_for_more_options_than_allowed()
    {
        var author = await NewUserAsync();
        var voter = await NewUserAsync();
        var survey = await author.CreatePublishedAsync(maxVotes: 2);
        var three = survey.Options.Select(o => o.Id).ToArray();

        var tooMany = await voter.PutAsync($"/api/surveys/{survey.Id}/votes", new { optionIds = three });
        var exactlyTwo = await voter.PutAsync($"/api/surveys/{survey.Id}/votes", new { optionIds = three[..2] });

        await tooMany.ShouldFailWith(HttpStatusCode.BadRequest, "too_many_votes");
        Assert.Equal(2, (await exactlyTwo.ReadAs<SurveyDetails>()).MyVotes.Count);
    }

    [Fact]
    public async Task Duplicate_option_ids_count_once()
    {
        var author = await NewUserAsync();
        var voter = await NewUserAsync();
        var survey = await author.CreatePublishedAsync(maxVotes: 1);
        var id = survey.Options[0].Id;

        var result = await (await voter.PutAsync($"/api/surveys/{survey.Id}/votes", new { optionIds = new[] { id, id, id } }))
            .ReadAs<SurveyDetails>();

        Assert.Equal(1, result.TotalVotes);
    }

    [Fact]
    public async Task Cannot_vote_with_an_option_of_another_survey()
    {
        var author = await NewUserAsync();
        var first = await author.CreatePublishedAsync();
        var second = await author.CreatePublishedAsync();

        var response = await author.PutAsync($"/api/surveys/{first.Id}/votes", new { optionIds = new[] { second.Options[0].Id } });

        await response.ShouldFailWith(HttpStatusCode.BadRequest, "invalid_option");
    }

    [Fact]
    public async Task Cannot_vote_in_a_draft()
    {
        var author = await NewUserAsync();
        var draft = await author.CreateSurveyAsync(new { title = "Draft", options = new[] { "A", "B" } });

        var response = await author.PutAsync($"/api/surveys/{draft.Id}/votes", new { optionIds = new[] { draft.Options[0].Id } });

        await response.ShouldFailWith(HttpStatusCode.Conflict, "survey_not_published");
    }

    [Fact]
    public async Task Deadline_closes_the_survey()
    {
        var author = await NewUserAsync();
        var voter = await NewUserAsync();
        var survey = await author.CreatePublishedAsync(deadline: factory.Clock.GetUtcNow().AddHours(1));

        factory.Clock.Advance(TimeSpan.FromHours(1));

        var response = await voter.PutAsync($"/api/surveys/{survey.Id}/votes", new { optionIds = new[] { survey.Options[0].Id } });
        await response.ShouldFailWith(HttpStatusCode.Conflict, "survey_closed");
        var closed = await voter.GetSurveyAsync(survey.Id);
        Assert.Equal(SurveyStatus.Closed, closed.Status);
        Assert.False(closed.CanVote);
        Assert.DoesNotContain((await ListAsync(voter)).Items, s => s.Id == survey.Id);
        Assert.Contains((await ListAsync(author, "scope=mine&status=closed")).Items, s => s.Id == survey.Id);
    }

    [Fact]
    public async Task Parallel_votes_of_one_user_never_exceed_the_limit()
    {
        var author = await NewUserAsync();
        var voter = await NewUserAsync();
        var survey = await author.CreatePublishedAsync(maxVotes: 1);

        var responses = await Task.WhenAll(survey.Options.Select(o =>
            voter.PutAsync($"/api/surveys/{survey.Id}/votes", new { optionIds = new[] { o.Id } })));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        Assert.Single((await voter.GetSurveyAsync(survey.Id)).MyVotes);
    }

    public static TheoryData<object, string> InvalidSurveys => new()
    {
        { new { title = "  ", options = new[] { "A", "B" } }, "title_required" },
        { new { title = new string('x', 201), options = new[] { "A", "B" } }, "title_length" },
        { new { title = "Q", options = new[] { "Only one" } }, "options_min" },
        { new { title = "Q", options = Array.Empty<string>() }, "options_min" },
        { new { title = "Q", options = new[] { "A", " " } }, "option_empty" },
        { new { title = "Q", options = new[] { "Yes", "yes " } }, "option_duplicate" },
        { new { title = "Q", options = Enumerable.Range(1, 31).Select(i => $"Option {i}").ToArray() }, "options_max" },
        { new { title = "Q", options = new[] { "A", "B" }, maxVotesPerUser = 0 }, "max_votes_range" },
        { new { title = "Q", options = new[] { "A", "B" }, maxVotesPerUser = 3 }, "max_votes_exceeds_options" },
        { new { title = "Q", options = new[] { "A", "B" }, allowParticipantOptions = true, maxOptionsPerParticipant = 0 }, "max_options_range" },
        { new { title = "Q", options = new[] { "A", "B" }, deadline = ApiFactory.Start.AddMinutes(-1) }, "deadline_past" },
    };

    [Theory]
    [MemberData(nameof(InvalidSurveys))]
    public async Task Create_rejects_invalid_input(object input, string code)
    {
        var author = await NewUserAsync();

        var response = await author.PostAsync("/api/surveys", input);

        await response.ShouldFailWith(HttpStatusCode.BadRequest, code);
    }

    [Fact]
    public async Task Draft_can_be_edited_then_published_but_not_edited_afterwards()
    {
        var author = await NewUserAsync();
        var draft = await author.CreateSurveyAsync(new { title = "Draft", options = new[] { "A", "B" } });

        var edited = await (await author.PutAsync($"/api/surveys/{draft.Id}", new { title = "Final", options = new[] { "X", "Y", "Z" } }))
            .ReadAs<SurveyDetails>();
        var published = await (await author.PostAsync($"/api/surveys/{draft.Id}/publish")).ReadAs<SurveyDetails>();
        var editAfterPublish = await author.PutAsync($"/api/surveys/{draft.Id}", new { title = "Again", options = new[] { "A", "B" } });
        var publishTwice = await author.PostAsync($"/api/surveys/{draft.Id}/publish");

        Assert.Equal(["X", "Y", "Z"], edited.Options.Select(o => o.Text));
        Assert.Equal(SurveyStatus.Active, published.Status);
        await editAfterPublish.ShouldFailWith(HttpStatusCode.Conflict, "survey_published");
        await publishTwice.ShouldFailWith(HttpStatusCode.Conflict, "already_published");
    }

    [Fact]
    public async Task Only_the_author_can_change_or_delete_a_survey()
    {
        var author = await NewUserAsync();
        var stranger = await NewUserAsync();
        var survey = await author.CreatePublishedAsync();

        await (await stranger.DeleteAsync($"/api/surveys/{survey.Id}")).ShouldFailWith(HttpStatusCode.Forbidden, "forbidden");
        await (await author.DeleteAsync($"/api/surveys/{survey.Id}")).ShouldBe(HttpStatusCode.NoContent);
        await (await author.GetAsync($"/api/surveys/{survey.Id}")).ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Participants_cannot_add_options_unless_allowed()
    {
        var author = await NewUserAsync();
        var participant = await NewUserAsync();
        var survey = await author.CreatePublishedAsync(allowOptions: false);

        var byParticipant = await participant.PostAsync($"/api/surveys/{survey.Id}/options", new { text = "Zoo" });
        var byAuthor = await author.PostAsync($"/api/surveys/{survey.Id}/options", new { text = "Zoo" });

        await byParticipant.ShouldFailWith(HttpStatusCode.Forbidden, "forbidden");
        Assert.Equal(4, (await byAuthor.ReadAs<SurveyDetails>()).Options.Count);
    }

    [Fact]
    public async Task Participant_option_limit_duplicates_and_vote_on_add()
    {
        var author = await NewUserAsync();
        var participant = await NewUserAsync();
        var participantName = (await participant.MeAsync()).DisplayName;
        var survey = await author.CreatePublishedAsync(allowOptions: true, maxOptions: 1);

        var duplicate = await participant.PostAsync($"/api/surveys/{survey.Id}/options", new { text = " park " });
        var added = await (await participant.PostAsync($"/api/surveys/{survey.Id}/options", new { text = "Zoo", vote = true }))
            .ReadAs<SurveyDetails>();
        var overLimit = await participant.PostAsync($"/api/surveys/{survey.Id}/options", new { text = "Beach" });

        await duplicate.ShouldFailWith(HttpStatusCode.Conflict, "option_duplicate");
        var zoo = added.Options.Single(o => o.Text == "Zoo");
        Assert.Equal(participantName, zoo.AddedBy);
        Assert.Equal([zoo.Id], added.MyVotes);
        Assert.False(added.CanAddOption);
        await overLimit.ShouldFailWith(HttpStatusCode.Conflict, "option_limit");
    }

    [Fact]
    public async Task Adding_an_option_with_vote_respects_the_vote_limit()
    {
        var author = await NewUserAsync();
        var participant = await NewUserAsync();
        var survey = await author.CreatePublishedAsync(maxVotes: 1, allowOptions: true, maxOptions: 2);
        await participant.PutAsync($"/api/surveys/{survey.Id}/votes", new { optionIds = new[] { survey.Options[0].Id } });

        var response = await participant.PostAsync($"/api/surveys/{survey.Id}/options", new { text = "Zoo", vote = true });

        await response.ShouldFailWith(HttpStatusCode.Conflict, "too_many_votes");
        Assert.Equal(3, (await participant.GetSurveyAsync(survey.Id)).Options.Count);
    }

    [Fact]
    public async Task List_is_paged_and_page_size_is_capped()
    {
        var author = await NewUserAsync();
        for (var i = 0; i < 3; i++)
            await author.CreatePublishedAsync();

        var page = await author.GetJsonAsync<Paged<SurveySummary>>("/api/surveys?scope=mine&page=2&pageSize=2");
        var capped = await author.GetJsonAsync<Paged<SurveySummary>>("/api/surveys?scope=mine&pageSize=1000");

        Assert.Equal(3, page.Total);
        Assert.Single(page.Items);
        Assert.Equal(50, capped.PageSize);
    }

    [Fact]
    public async Task Guests_see_published_surveys_and_results_but_not_voters_or_emails()
    {
        var author = await NewUserAsync("Alice");
        var voter = await NewUserAsync();
        var survey = await author.CreatePublishedAsync();
        await voter.PutAsync($"/api/surveys/{survey.Id}/votes", new { optionIds = new[] { survey.Options[1].Id } });
        var guest = ApiClient.Anonymous(factory);

        var listed = await ListAsync(guest);
        var response = await guest.GetAsync($"/api/surveys/{survey.Id}");
        var asGuest = await response.ReadAs<SurveyDetails>();

        Assert.Contains(listed.Items, s => s.Id == survey.Id && !s.HasVoted);
        Assert.Equal("Alice", asGuest.Author.Name);
        Assert.Equal(1, asGuest.Options[1].Votes);
        Assert.All(asGuest.Options, o => Assert.Null(o.Voters));
        Assert.False(asGuest.CanVote);
        Assert.False(asGuest.CanAddOption);
        Assert.Empty(asGuest.MyVotes);
        Assert.DoesNotContain("@example.com", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Guests_cannot_vote_add_options_or_see_drafts()
    {
        var author = await NewUserAsync();
        var survey = await author.CreatePublishedAsync(allowOptions: true);
        var draft = await author.CreateSurveyAsync(new { title = "Draft", options = new[] { "A", "B" } });
        var guest = ApiClient.Anonymous(factory);

        await (await guest.PutAsync($"/api/surveys/{survey.Id}/votes", new { optionIds = new[] { survey.Options[0].Id } }))
            .ShouldBe(HttpStatusCode.Unauthorized);
        await (await guest.PostAsync($"/api/surveys/{survey.Id}/options", new { text = "Zoo" })).ShouldBe(HttpStatusCode.Unauthorized);
        await (await guest.PostAsync("/api/surveys", new { title = "Q", options = new[] { "A", "B" } })).ShouldBe(HttpStatusCode.Unauthorized);
        await (await guest.GetAsync($"/api/surveys/{draft.Id}")).ShouldFailWith(HttpStatusCode.NotFound, "not_found");
    }

    [Fact]
    public async Task Unknown_survey_id_returns_404()
    {
        var user = await NewUserAsync();

        await (await user.GetAsync($"/api/surveys/{Guid.NewGuid()}")).ShouldFailWith(HttpStatusCode.NotFound, "not_found");
        await (await user.GetAsync("/api/surveys/not-a-guid")).ShouldBe(HttpStatusCode.NotFound);
    }
}
