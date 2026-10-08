using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QandA.Api.Common;
using QandA.Api.Data;
using QandA.Api.Domain;

namespace QandA.Api.Auth;

/// <summary>Single-use, expiring tokens sent by email. The database only holds their SHA-256 hashes.</summary>
public class TokenService(AppDbContext db, TimeProvider clock)
{
    public static TimeSpan Lifetime(TokenPurpose purpose) =>
        purpose == TokenPurpose.PasswordReset ? TimeSpan.FromHours(1) : TimeSpan.FromDays(3);

    public static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    /// <summary>Creates a new token; earlier unused tokens of the same purpose stop working.</summary>
    public async Task<string> IssueAsync(int userId, TokenPurpose purpose, CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        await RevokeAsync(userId, purpose, ct);
        var token = Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        db.UserTokens.Add(new UserToken
        {
            UserId = userId,
            Purpose = purpose,
            TokenHash = Hash(token),
            CreatedAt = now,
            ExpiresAt = now + Lifetime(purpose),
        });
        await db.SaveChangesAsync(ct);
        return token;
    }

    /// <summary>The valid (unused, unexpired) token with its user, tracked so the caller can mark it used.</summary>
    public async Task<UserToken> FindValidAsync(string? token, TokenPurpose purpose, CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        var hash = Hash(token ?? "");
        return await db.UserTokens.Include(t => t.User)
                .SingleOrDefaultAsync(t => t.TokenHash == hash && t.Purpose == purpose && t.UsedAt == null && t.ExpiresAt > now, ct)
            ?? throw AppException.Validation("token_invalid", "The link is invalid or has expired.");
    }

    public Task RevokeAsync(int userId, TokenPurpose purpose, CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        return db.UserTokens
            .Where(t => t.UserId == userId && t.Purpose == purpose && t.UsedAt == null)
            .ExecuteUpdateAsync(u => u.SetProperty(t => t.UsedAt, now), ct);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
