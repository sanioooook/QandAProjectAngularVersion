using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QandA.Api.Common;
using QandA.Api.Data;
using QandA.Api.Email;

namespace QandA.Api.Auth;

/// <summary>
/// Who may vote, add options and create surveys: any signed-in user, and when email confirmation is
/// required (email enabled), only users who confirmed their address. Reading needs neither.
/// </summary>
public class ParticipationPolicy(AppDbContext db, IOptions<EmailOptions> email)
{
    public async Task<bool> IsAllowedAsync(int? userId, CancellationToken ct)
    {
        if (userId is null) return false;
        if (!email.Value.ConfirmationRequired) return true;
        return await db.Users.AnyAsync(u => u.Id == userId && u.EmailConfirmedAt != null, ct);
    }

    public async Task EnsureAllowedAsync(int userId, CancellationToken ct)
    {
        if (!await IsAllowedAsync(userId, ct))
            throw new AppException(StatusCodes.Status403Forbidden, "email_not_confirmed", "Confirm your email address first.");
    }
}
