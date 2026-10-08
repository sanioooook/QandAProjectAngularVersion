namespace QandA.Api.Domain;

public class User
{
    public int Id { get; set; }
    /// <summary>The login. Never shown to other users.</summary>
    public required string Email { get; set; }
    /// <summary>Upper-cased email, used for case-insensitive uniqueness.</summary>
    public required string NormalizedEmail { get; set; }
    /// <summary>Public name shown as survey author and voter.</summary>
    public required string DisplayName { get; set; }
    public string PasswordHash { get; set; } = "";
    /// <summary>Changes with the password; auth cookies carrying an older stamp are rejected.</summary>
    public string SecurityStamp { get; set; } = NewStamp();
    public DateTimeOffset? EmailConfirmedAt { get; set; }
    /// <summary>Language of the emails sent to the user (uk, en, ru).</summary>
    public string Locale { get; set; } = "uk";
    /// <summary>When the avatar was last set; null without one. Versions the avatar URL for caching.</summary>
    public DateTimeOffset? AvatarUpdatedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public static string NewStamp() => Guid.NewGuid().ToString("N");
}

/// <summary>Avatar image, kept apart from <see cref="User"/> so user queries never load the bytes.</summary>
public class UserAvatar
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public required string ContentType { get; set; }
    public required byte[] Data { get; set; }
}

public enum TokenPurpose { EmailConfirmation, PasswordReset }

/// <summary>Single-use emailed token. Only its SHA-256 hash is stored.</summary>
public class UserToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public TokenPurpose Purpose { get; set; }
    public required string TokenHash { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
}

public class Survey
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    /// <summary>Null while the survey is a private draft.</summary>
    public DateTimeOffset? PublishedAt { get; set; }
    /// <summary>After this moment no votes or new options are accepted.</summary>
    public DateTimeOffset? Deadline { get; set; }
    /// <summary>How many options a single participant may vote for at once.</summary>
    public int MaxVotesPerUser { get; set; } = 1;
    public bool AllowParticipantOptions { get; set; }
    /// <summary>How many options a single participant may add (when allowed).</summary>
    public int MaxOptionsPerParticipant { get; set; } = 1;

    public List<SurveyOption> Options { get; set; } = [];
    public List<Vote> Votes { get; set; } = [];
}

public class SurveyOption
{
    public int Id { get; set; }
    public Guid SurveyId { get; set; }
    public Survey Survey { get; set; } = null!;
    public required string Text { get; set; }
    public int Position { get; set; }
    /// <summary>Participant who added the option; null for options created by the author.</summary>
    public int? AddedById { get; set; }
    public User? AddedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public List<Vote> Votes { get; set; } = [];
}

public class Vote
{
    public int OptionId { get; set; }
    public SurveyOption Option { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    /// <summary>Denormalized from the option so per-survey queries need no join.</summary>
    public Guid SurveyId { get; set; }
    public Survey Survey { get; set; } = null!;
    public DateTimeOffset VotedAt { get; set; }
}
