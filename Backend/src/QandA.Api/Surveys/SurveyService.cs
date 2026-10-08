using Microsoft.EntityFrameworkCore;
using QandA.Api.Account;
using QandA.Api.Auth;
using QandA.Api.Common;
using QandA.Api.Data;
using QandA.Api.Domain;

namespace QandA.Api.Surveys;

public class SurveyService(AppDbContext db, ParticipationPolicy participation, TimeProvider clock)
{
    public async Task<Paged<SurveySummary>> ListAsync(
        int? userId, SurveyScope scope, SurveyStatus? status, int page, int pageSize, CancellationToken ct)
    {
        if (userId is null && scope != SurveyScope.Active)
            throw AppException.Unauthorized("unauthorized", "Sign in to see your surveys.");

        var now = clock.GetUtcNow();
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, Limits.PageSizeMax);

        var query = db.Surveys.AsNoTracking();
        query = scope switch
        {
            SurveyScope.Mine => query.Where(s => s.AuthorId == userId),
            SurveyScope.Voted => query.Where(s => s.PublishedAt != null && s.Votes.Any(v => v.UserId == userId)),
            _ => query.Where(s => s.PublishedAt != null && (s.Deadline == null || s.Deadline > now)),
        };
        query = status switch
        {
            SurveyStatus.Draft => query.Where(s => s.PublishedAt == null),
            SurveyStatus.Active => query.Where(s => s.PublishedAt != null && (s.Deadline == null || s.Deadline > now)),
            SurveyStatus.Closed => query.Where(s => s.PublishedAt != null && s.Deadline != null && s.Deadline <= now),
            _ => query,
        };

        var total = await query.CountAsync(ct);
        var rows = await query
            .OrderByDescending(s => s.PublishedAt ?? s.CreatedAt)
            .ThenByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.Title,
                s.AuthorId,
                AuthorName = s.Author.DisplayName,
                AuthorAvatar = s.Author.AvatarUpdatedAt,
                s.CreatedAt,
                s.PublishedAt,
                s.Deadline,
                OptionCount = s.Options.Count,
                VoterCount = s.Votes.Select(v => v.UserId).Distinct().Count(),
                HasVoted = s.Votes.Any(v => v.UserId == userId),
            })
            .ToListAsync(ct);

        var items = rows.Select(r => new SurveySummary(
            r.Id, r.Title, new PublicUserDto(r.AuthorId, r.AuthorName, Avatars.Url(r.AuthorId, r.AuthorAvatar)), r.CreatedAt, r.PublishedAt, r.Deadline,
            SurveyRules.StatusOf(r.PublishedAt, r.Deadline, now), r.OptionCount, r.VoterCount, r.HasVoted)).ToList();

        return new Paged<SurveySummary>(items, total, page, pageSize);
    }

    public async Task<SurveyDetails> GetAsync(Guid id, int? userId, CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        var s = await db.Surveys.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id, x.Title, x.Description, x.AuthorId, AuthorName = x.Author.DisplayName, AuthorAvatar = x.Author.AvatarUpdatedAt, x.CreatedAt, x.PublishedAt,
                x.Deadline, x.MaxVotesPerUser, x.AllowParticipantOptions, x.MaxOptionsPerParticipant,
            })
            .SingleOrDefaultAsync(ct);

        // Drafts are private: for anyone but the author they do not exist.
        if (s is null || (s.PublishedAt is null && s.AuthorId != userId))
            throw AppException.NotFound("Survey not found.");

        var isAuthor = s.AuthorId == userId;
        var options = await db.Options.AsNoTracking()
            .Where(o => o.SurveyId == id)
            .OrderBy(o => o.Position).ThenBy(o => o.Id)
            .Select(o => new { o.Id, o.Text, Votes = o.Votes.Count, AddedBy = o.AddedBy != null ? o.AddedBy.DisplayName : null })
            .ToListAsync(ct);

        var votes = db.Votes.AsNoTracking().Where(v => v.SurveyId == id);
        var myVotes = await votes.Where(v => v.UserId == userId).Select(v => v.OptionId).ToListAsync(ct);
        var voterCount = await votes.Select(v => v.UserId).Distinct().CountAsync(ct);
        var myAdded = await db.Options.CountAsync(o => o.SurveyId == id && o.AddedById == userId, ct);

        ILookup<int, VoterDto>? voters = null;
        if (isAuthor)
        {
            voters = (await votes
                    .OrderBy(v => v.VotedAt)
                    .Select(v => new { v.OptionId, v.UserId, v.User.DisplayName, v.User.AvatarUpdatedAt, v.VotedAt })
                    .ToListAsync(ct))
                .ToLookup(v => v.OptionId, v => new VoterDto(v.UserId, v.DisplayName, Avatars.Url(v.UserId, v.AvatarUpdatedAt), v.VotedAt));
        }

        var status = SurveyRules.StatusOf(s.PublishedAt, s.Deadline, now);
        var mayParticipate = await participation.IsAllowedAsync(userId, ct);
        var canAddOption = status == SurveyStatus.Active
            && mayParticipate
            && options.Count < Limits.OptionsMax
            && (isAuthor || (s.AllowParticipantOptions && myAdded < s.MaxOptionsPerParticipant));

        return new SurveyDetails(
            s.Id, s.Title, s.Description, new PublicUserDto(s.AuthorId, s.AuthorName, Avatars.Url(s.AuthorId, s.AuthorAvatar)), s.CreatedAt, s.PublishedAt, s.Deadline,
            status, s.MaxVotesPerUser, s.AllowParticipantOptions, s.MaxOptionsPerParticipant, isAuthor,
            voterCount, options.Sum(o => o.Votes), myVotes, myAdded,
            CanVote: status == SurveyStatus.Active && mayParticipate,
            CanAddOption: canAddOption,
            options.Select(o => new OptionDto(o.Id, o.Text, o.Votes, o.AddedBy, voters?[o.Id].ToList())).ToList());
    }

    public async Task<SurveyDetails> CreateAsync(SurveyInput input, int userId, CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        await participation.EnsureAllowedAsync(userId, ct);
        var valid = SurveyRules.Validate(input, now);
        var survey = new Survey
        {
            Title = valid.Title,
            AuthorId = userId,
            CreatedAt = now,
        };
        Apply(survey, valid, now);
        db.Surveys.Add(survey);
        await db.SaveChangesAsync(ct);
        return await GetAsync(survey.Id, userId, ct);
    }

    public async Task<SurveyDetails> UpdateAsync(Guid id, SurveyInput input, int userId, CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        var survey = await LoadOwnAsync(id, userId, ct, includeOptions: true);
        if (survey.PublishedAt is not null)
            throw AppException.Conflict("survey_published", "A published survey can no longer be edited.");

        var valid = SurveyRules.Validate(input, now);
        db.Options.RemoveRange(survey.Options);
        survey.Options.Clear();
        Apply(survey, valid, now);
        await db.SaveChangesAsync(ct);
        return await GetAsync(id, userId, ct);
    }

    public async Task<SurveyDetails> PublishAsync(Guid id, int userId, CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        var survey = await LoadOwnAsync(id, userId, ct);
        await participation.EnsureAllowedAsync(userId, ct);
        if (survey.PublishedAt is not null)
            throw AppException.Conflict("already_published", "The survey is already published.");
        if (survey.Deadline is not null && survey.Deadline <= now)
            throw AppException.Validation("deadline_past", "The deadline has already passed.", "deadline");

        survey.PublishedAt = now;
        await db.SaveChangesAsync(ct);
        return await GetAsync(id, userId, ct);
    }

    public async Task DeleteAsync(Guid id, int userId, CancellationToken ct)
    {
        var survey = await LoadOwnAsync(id, userId, ct);
        db.Surveys.Remove(survey);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Replaces the user's votes in the survey with <paramref name="input"/> (empty list withdraws the vote).</summary>
    public async Task<SurveyDetails> VoteAsync(Guid id, VoteInput input, int userId, CancellationToken ct)
    {
        var optionIds = (input.OptionIds ?? []).Distinct().ToList();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await LockParticipantAsync(id, userId, ct);

        var survey = await LoadVotableAsync(id, userId, ct);
        await participation.EnsureAllowedAsync(userId, ct);
        if (optionIds.Count > survey.MaxVotesPerUser)
            throw AppException.Validation("too_many_votes", $"You can vote for at most {survey.MaxVotesPerUser} option(s).");

        var existing = await db.Options.CountAsync(o => o.SurveyId == id && optionIds.Contains(o.Id), ct);
        if (existing != optionIds.Count)
            throw AppException.Validation("invalid_option", "One of the options does not belong to this survey.");

        var now = clock.GetUtcNow();
        await db.Votes.Where(v => v.SurveyId == id && v.UserId == userId).ExecuteDeleteAsync(ct);
        db.Votes.AddRange(optionIds.Select(o => new Vote { OptionId = o, UserId = userId, SurveyId = id, VotedAt = now }));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return await GetAsync(id, userId, ct);
    }

    public async Task<SurveyDetails> AddOptionAsync(Guid id, AddOptionInput input, int userId, CancellationToken ct)
    {
        var text = (input.Text ?? "").Trim();
        if (text.Length == 0)
            throw AppException.Validation("option_empty", "The option text is required.", "text");
        if (text.Length > Limits.OptionTextMax)
            throw AppException.Validation("option_length", $"An option may be at most {Limits.OptionTextMax} characters.", "text");

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await LockParticipantAsync(id, userId, ct);

        var survey = await LoadVotableAsync(id, userId, ct);
        await participation.EnsureAllowedAsync(userId, ct);
        var isAuthor = survey.AuthorId == userId;
        if (!isAuthor && !survey.AllowParticipantOptions)
            throw AppException.Forbidden("This survey does not accept new options from participants.");

        var options = await db.Options.Where(o => o.SurveyId == id).Select(o => new { o.Text, o.Position, o.AddedById }).ToListAsync(ct);
        if (options.Count >= Limits.OptionsMax)
            throw AppException.Conflict("options_max", $"A survey may have at most {Limits.OptionsMax} options.");
        if (!isAuthor && options.Count(o => o.AddedById == userId) >= survey.MaxOptionsPerParticipant)
            throw AppException.Conflict("option_limit", $"You can add at most {survey.MaxOptionsPerParticipant} option(s).");
        if (options.Any(o => string.Equals(o.Text, text, StringComparison.OrdinalIgnoreCase)))
            throw AppException.Conflict("option_duplicate", "This option already exists.", "text");

        var now = clock.GetUtcNow();
        if (input.Vote)
        {
            var myVotes = await db.Votes.CountAsync(v => v.SurveyId == id && v.UserId == userId, ct);
            if (myVotes >= survey.MaxVotesPerUser)
                throw AppException.Conflict("too_many_votes", $"You can vote for at most {survey.MaxVotesPerUser} option(s).");
        }

        var option = new SurveyOption
        {
            SurveyId = id,
            Text = text,
            Position = options.Count == 0 ? 0 : options.Max(o => o.Position) + 1,
            AddedById = isAuthor ? null : userId,
            CreatedAt = now,
        };
        db.Options.Add(option);
        await db.SaveChangesAsync(ct);

        if (input.Vote)
        {
            db.Votes.Add(new Vote { OptionId = option.Id, UserId = userId, SurveyId = id, VotedAt = now });
            await db.SaveChangesAsync(ct);
        }
        await tx.CommitAsync(ct);

        return await GetAsync(id, userId, ct);
    }

    private static void Apply(Survey survey, SurveyRules.ValidInput valid, DateTimeOffset now)
    {
        survey.Title = valid.Title;
        survey.Description = valid.Description;
        survey.Deadline = valid.Deadline;
        survey.MaxVotesPerUser = valid.MaxVotesPerUser;
        survey.AllowParticipantOptions = valid.AllowParticipantOptions;
        survey.MaxOptionsPerParticipant = valid.MaxOptionsPerParticipant;
        survey.Options.AddRange(valid.Options.Select((text, i) => new SurveyOption { Text = text, Position = i, CreatedAt = now }));
        if (valid.Publish)
            survey.PublishedAt = now;
    }

    private async Task<Survey> LoadOwnAsync(Guid id, int userId, CancellationToken ct, bool includeOptions = false)
    {
        var query = db.Surveys.Where(s => s.Id == id);
        if (includeOptions)
            query = query.Include(s => s.Options);
        var survey = await query.SingleOrDefaultAsync(ct);
        if (survey is null || (survey.PublishedAt is null && survey.AuthorId != userId))
            throw AppException.NotFound("Survey not found.");
        if (survey.AuthorId != userId)
            throw AppException.Forbidden("Only the author can change this survey.");
        return survey;
    }

    private async Task<Survey> LoadVotableAsync(Guid id, int userId, CancellationToken ct)
    {
        var survey = await db.Surveys.AsNoTracking().SingleOrDefaultAsync(s => s.Id == id, ct);
        if (survey is null || (survey.PublishedAt is null && survey.AuthorId != userId))
            throw AppException.NotFound("Survey not found.");
        switch (SurveyRules.StatusOf(survey.PublishedAt, survey.Deadline, clock.GetUtcNow()))
        {
            case SurveyStatus.Draft:
                throw AppException.Conflict("survey_not_published", "The survey is not published yet.");
            case SurveyStatus.Closed:
                throw AppException.Conflict("survey_closed", "The survey is closed.");
        }
        return survey;
    }

    /// <summary>
    /// Serializes concurrent vote / add-option requests of one user in one survey, so two parallel
    /// requests cannot together exceed the per-user limits.
    /// </summary>
    private Task LockParticipantAsync(Guid surveyId, int userId, CancellationToken ct) =>
        db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({userId}, hashtext({surveyId.ToString()}))", ct);
}
