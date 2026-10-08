using QandA.Api.Common;
using QandA.Api.Surveys;

namespace QandA.Api.UnitTests;

public class SurveyRulesTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 8, 12, 0, 0, TimeSpan.Zero);

    private static SurveyInput Input(
        string? title = "Lunch?", string[]? options = null, int maxVotes = 1, bool allowOptions = false,
        int maxOptions = 1, DateTimeOffset? deadline = null, string? description = null) =>
        new(title, description, [.. options ?? ["Pizza", "Sushi"]], deadline, maxVotes, allowOptions, maxOptions);

    private static string CodeOf(SurveyInput input) =>
        Assert.Throws<AppException>(() => SurveyRules.Validate(input, Now)).Code;

    [Fact]
    public void Status_is_derived_from_publish_date_and_deadline()
    {
        Assert.Equal(SurveyStatus.Draft, SurveyRules.StatusOf(null, null, Now));
        Assert.Equal(SurveyStatus.Draft, SurveyRules.StatusOf(null, Now.AddDays(-1), Now));
        Assert.Equal(SurveyStatus.Active, SurveyRules.StatusOf(Now.AddDays(-1), null, Now));
        Assert.Equal(SurveyStatus.Active, SurveyRules.StatusOf(Now.AddDays(-1), Now.AddTicks(1), Now));
        // The deadline moment itself already counts as closed.
        Assert.Equal(SurveyStatus.Closed, SurveyRules.StatusOf(Now.AddDays(-1), Now, Now));
    }

    [Fact]
    public void Valid_input_is_trimmed()
    {
        var valid = SurveyRules.Validate(Input(title: "  Lunch?  ", options: [" Pizza ", "Sushi"], description: "   "), Now);

        Assert.Equal("Lunch?", valid.Title);
        Assert.Equal(["Pizza", "Sushi"], valid.Options);
        Assert.Null(valid.Description);
    }

    [Fact]
    public void Deadline_is_normalized_to_utc()
    {
        var kyiv = new DateTimeOffset(2026, 10, 22, 23, 59, 59, 999, TimeSpan.FromHours(3));

        var valid = SurveyRules.Validate(Input(deadline: kyiv), Now);

        Assert.Equal(TimeSpan.Zero, valid.Deadline!.Value.Offset);
        Assert.Equal(kyiv, valid.Deadline);
    }

    [Fact]
    public void Options_per_participant_are_ignored_when_participants_cannot_add_options() =>
        Assert.Equal(1, SurveyRules.Validate(Input(allowOptions: false, maxOptions: 99), Now).MaxOptionsPerParticipant);

    [Fact]
    public void More_votes_than_options_are_fine_when_participants_can_add_options() =>
        Assert.Equal(5, SurveyRules.Validate(Input(maxVotes: 5, allowOptions: true), Now).MaxVotesPerUser);

    [Fact]
    public void Exactly_thirty_options_are_allowed() =>
        Assert.Equal(30, SurveyRules.Validate(Input(options: [.. Enumerable.Range(1, 30).Select(i => $"#{i}")]), Now).Options.Count);

    public static TheoryData<SurveyInput, string> Invalid => new()
    {
        { Input(title: null), "title_required" },
        { Input(title: " "), "title_required" },
        { Input(title: new string('q', 201)), "title_length" },
        { Input(description: new string('d', 1001)), "description_length" },
        { Input(options: ["Only"]), "options_min" },
        { Input(options: [.. Enumerable.Range(1, 31).Select(i => $"#{i}")]), "options_max" },
        { Input(options: ["A", ""]), "option_empty" },
        { Input(options: ["A", new string('o', 201)]), "option_length" },
        { Input(options: ["Pizza", " PIZZA "]), "option_duplicate" },
        { Input(maxVotes: 0), "max_votes_range" },
        { Input(maxVotes: 31, allowOptions: true), "max_votes_range" },
        { Input(maxVotes: 3), "max_votes_exceeds_options" },
        { Input(allowOptions: true, maxOptions: 0), "max_options_range" },
        { Input(allowOptions: true, maxOptions: 11), "max_options_range" },
        { Input(deadline: Now), "deadline_past" },
        { Input(deadline: Now.AddSeconds(-1)), "deadline_past" },
    };

    [Theory]
    [MemberData(nameof(Invalid))]
    public void Invalid_input_is_rejected_with_a_code(SurveyInput input, string code) => Assert.Equal(code, CodeOf(input));
}
