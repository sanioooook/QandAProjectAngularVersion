using QandA.Api.Auth;

namespace QandA.Api.Surveys;

public enum SurveyStatus { Draft, Active, Closed }

public enum SurveyScope { Active, Mine, Voted }

public record SurveyInput(
    string? Title,
    string? Description,
    List<string>? Options,
    DateTimeOffset? Deadline,
    int MaxVotesPerUser = 1,
    bool AllowParticipantOptions = false,
    int MaxOptionsPerParticipant = 1,
    bool Publish = false);

public record VoteInput(List<int>? OptionIds);

public record AddOptionInput(string? Text, bool Vote = false);

public record Paged<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

public record SurveySummary(
    Guid Id,
    string Title,
    PublicUserDto Author,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PublishedAt,
    DateTimeOffset? Deadline,
    SurveyStatus Status,
    int OptionCount,
    int VoterCount,
    bool HasVoted);

public record VoterDto(int UserId, string Name, string? AvatarUrl, DateTimeOffset VotedAt);

/// <param name="Voters">Only filled for the survey author; null for everyone else.</param>
public record OptionDto(int Id, string Text, int Votes, string? AddedBy, IReadOnlyList<VoterDto>? Voters);

public record SurveyDetails(
    Guid Id,
    string Title,
    string? Description,
    PublicUserDto Author,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PublishedAt,
    DateTimeOffset? Deadline,
    SurveyStatus Status,
    int MaxVotesPerUser,
    bool AllowParticipantOptions,
    int MaxOptionsPerParticipant,
    bool IsAuthor,
    int VoterCount,
    int TotalVotes,
    IReadOnlyList<int> MyVotes,
    int MyAddedOptions,
    bool CanVote,
    bool CanAddOption,
    IReadOnlyList<OptionDto> Options);
