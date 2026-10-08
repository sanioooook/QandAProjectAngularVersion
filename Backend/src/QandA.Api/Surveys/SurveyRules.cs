using QandA.Api.Common;
using QandA.Api.Domain;

namespace QandA.Api.Surveys;

/// <summary>Survey rules that need neither the database nor HTTP. Pure functions, covered by unit tests.</summary>
public static class SurveyRules
{
    /// <summary>Status is derived, not stored: no publish date means draft, a passed deadline means closed.</summary>
    public static SurveyStatus StatusOf(DateTimeOffset? publishedAt, DateTimeOffset? deadline, DateTimeOffset now) =>
        publishedAt is null ? SurveyStatus.Draft
        : deadline is not null && deadline <= now ? SurveyStatus.Closed
        : SurveyStatus.Active;

    public record ValidInput(
        string Title, string? Description, List<string> Options, DateTimeOffset? Deadline,
        int MaxVotesPerUser, bool AllowParticipantOptions, int MaxOptionsPerParticipant, bool Publish);

    public static ValidInput Validate(SurveyInput input, DateTimeOffset now)
    {
        var title = (input.Title ?? "").Trim();
        if (title.Length == 0)
            throw AppException.Validation("title_required", "The question is required.", "title");
        if (title.Length > Limits.TitleMax)
            throw AppException.Validation("title_length", $"The question may be at most {Limits.TitleMax} characters.", "title");

        var description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        if (description?.Length > Limits.DescriptionMax)
            throw AppException.Validation("description_length", $"The description may be at most {Limits.DescriptionMax} characters.", "description");

        var options = (input.Options ?? []).Select(o => (o ?? "").Trim()).ToList();
        if (options.Any(o => o.Length == 0))
            throw AppException.Validation("option_empty", "Options cannot be empty.", "options");
        if (options.Any(o => o.Length > Limits.OptionTextMax))
            throw AppException.Validation("option_length", $"An option may be at most {Limits.OptionTextMax} characters.", "options");
        if (options.Count < Limits.OptionsMin)
            throw AppException.Validation("options_min", $"Add at least {Limits.OptionsMin} options.", "options");
        if (options.Count > Limits.OptionsMax)
            throw AppException.Validation("options_max", $"A survey may have at most {Limits.OptionsMax} options.", "options");
        if (options.Distinct(StringComparer.OrdinalIgnoreCase).Count() != options.Count)
            throw AppException.Validation("option_duplicate", "Options must be unique.", "options");

        if (input.MaxVotesPerUser < 1 || input.MaxVotesPerUser > Limits.MaxVotesPerUserCap)
            throw AppException.Validation("max_votes_range", $"Votes per participant must be 1-{Limits.MaxVotesPerUserCap}.", "maxVotesPerUser");
        if (!input.AllowParticipantOptions && input.MaxVotesPerUser > options.Count)
            throw AppException.Validation("max_votes_exceeds_options", "Votes per participant cannot exceed the number of options.", "maxVotesPerUser");
        if (input.AllowParticipantOptions
            && (input.MaxOptionsPerParticipant < 1 || input.MaxOptionsPerParticipant > Limits.MaxOptionsPerParticipantCap))
            throw AppException.Validation("max_options_range", $"Options per participant must be 1-{Limits.MaxOptionsPerParticipantCap}.", "maxOptionsPerParticipant");

        var deadline = input.Deadline?.ToUniversalTime();
        if (deadline is not null && deadline <= now)
            throw AppException.Validation("deadline_past", "The deadline must be in the future.", "deadline");

        return new ValidInput(title, description, options, deadline, input.MaxVotesPerUser, input.AllowParticipantOptions,
            input.AllowParticipantOptions ? input.MaxOptionsPerParticipant : 1, input.Publish);
    }
}
