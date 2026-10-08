using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using QandA.Api.Account;
using QandA.Api.Common;
using QandA.Api.Data;
using QandA.Api.Domain;
using QandA.Api.Email;

namespace QandA.Api.Auth;

public record RegisterRequest(string? Email, string? DisplayName, string? Password, string? Locale);
public record LoginRequest(string? Email, string? Password);
public record ConfirmEmailRequest(string? Token);
public record ForgotPasswordRequest(string? Email, string? Locale);
public record ResetPasswordRequest(string? Token, string? Password);
public record ChangePasswordRequest(string? CurrentPassword, string? NewPassword);

/// <summary>The signed-in user's own account. The email never appears in public DTOs.</summary>
public record AccountDto(int Id, string Email, string DisplayName, bool EmailConfirmed, string Locale, string? AvatarUrl);

/// <summary>How other users see someone (survey author, voter).</summary>
public record PublicUserDto(int Id, string Name, string? AvatarUrl);

public record AuthConfigDto(bool EmailEnabled, bool ConfirmationRequired);

/// <summary>Result of an action that (re)issues the auth cookie.</summary>
public record SignIn(AccountDto Account, string SecurityStamp);

public class AuthService(
    AppDbContext db,
    IPasswordHasher<User> hasher,
    TokenService tokens,
    IEmailOutbox outbox,
    IOptions<EmailOptions> emailOptions,
    IOptions<AppOptions> appOptions,
    TimeProvider clock)
{
    // Hash of a random password, verified against when the email is unknown so that
    // "unknown email" and "wrong password" take the same time.
    private static readonly Lazy<string> DummyHash =
        new(() => new PasswordHasher<User>().HashPassword(null!, Guid.NewGuid().ToString()));

    private EmailOptions Email => emailOptions.Value;

    public AuthConfigDto Config() => new(Email.Enabled, Email.ConfirmationRequired);

    public async Task<SignIn> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var (email, normalized) = AccountRules.Email(request.Email);
        var name = AccountRules.DisplayName(request.DisplayName);
        AccountRules.Password(request.Password, email);

        if (await db.Users.AnyAsync(u => u.NormalizedEmail == normalized, ct))
            throw EmailTaken();

        var user = new User
        {
            Email = email,
            NormalizedEmail = normalized,
            DisplayName = name,
            Locale = AccountRules.Locale(request.Locale),
            CreatedAt = clock.GetUtcNow(),
        };
        user.PasswordHash = hasher.HashPassword(user, request.Password!);
        db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Two registrations with the same email raced past the check above.
            throw EmailTaken();
        }

        if (Email.Enabled)
            await SendConfirmationAsync(user, ct);

        return ToSignIn(user);
    }

    public async Task<SignIn> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var normalized = AccountRules.Normalize(request.Email);
        var password = request.Password ?? "";
        if (normalized.Length == 0 || password.Length == 0)
            throw AppException.Validation("credentials_required", "Email and password are required.");

        var user = await db.Users.SingleOrDefaultAsync(u => u.NormalizedEmail == normalized, ct);
        if (user is null)
        {
            hasher.VerifyHashedPassword(null!, DummyHash.Value, password);
            throw InvalidCredentials();
        }

        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
            throw InvalidCredentials();
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, password);
            await db.SaveChangesAsync(ct);
        }

        return ToSignIn(user);
    }

    public async Task<AccountDto?> FindAsync(int id, CancellationToken ct) =>
        await db.Users.Where(u => u.Id == id)
            .Select(u => new { u.Id, u.Email, u.DisplayName, Confirmed = u.EmailConfirmedAt != null, u.Locale, u.AvatarUpdatedAt })
            .SingleOrDefaultAsync(ct) is { } u
            ? new AccountDto(u.Id, u.Email, u.DisplayName, u.Confirmed, u.Locale, Avatars.Url(u.Id, u.AvatarUpdatedAt))
            : null;

    /// <summary>Current security stamp of a user, or null when the user no longer exists.</summary>
    public Task<string?> StampAsync(int id, CancellationToken ct) =>
        db.Users.Where(u => u.Id == id).Select(u => u.SecurityStamp).SingleOrDefaultAsync(ct);

    public async Task ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken ct)
    {
        var token = await tokens.FindValidAsync(request.Token, TokenPurpose.EmailConfirmation, ct);
        token.UsedAt = clock.GetUtcNow();
        token.User.EmailConfirmedAt ??= clock.GetUtcNow();
        await db.SaveChangesAsync(ct);
    }

    public async Task ResendConfirmationAsync(int userId, CancellationToken ct)
    {
        RequireEmail();
        var user = await db.Users.SingleAsync(u => u.Id == userId, ct);
        if (user.EmailConfirmedAt is not null)
            throw AppException.Conflict("already_confirmed", "The email is already confirmed.");
        await SendConfirmationAsync(user, ct);
    }

    /// <summary>Always succeeds for a well-formed request, so it cannot be used to find out which emails are registered.</summary>
    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct)
    {
        RequireEmail();
        var (_, normalized) = AccountRules.Email(request.Email);
        var user = await db.Users.SingleOrDefaultAsync(u => u.NormalizedEmail == normalized, ct);
        if (user is null)
            return;

        var token = await tokens.IssueAsync(user.Id, TokenPurpose.PasswordReset, ct);
        var locale = request.Locale is null ? user.Locale : AccountRules.Locale(request.Locale);
        outbox.Enqueue(EmailTemplates.PasswordReset(user.Email, user.DisplayName, Link("reset-password", token), locale));
    }

    public async Task<SignIn> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct)
    {
        var token = await tokens.FindValidAsync(request.Token, TokenPurpose.PasswordReset, ct);
        var user = token.User;
        AccountRules.Password(request.Password, user.Email);

        token.UsedAt = clock.GetUtcNow();
        // Following the emailed link proves the address belongs to the user.
        user.EmailConfirmedAt ??= clock.GetUtcNow();
        await SetPasswordAsync(user, request.Password!, ct);
        return ToSignIn(user);
    }

    public async Task<SignIn> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct)
    {
        var user = await db.Users.SingleAsync(u => u.Id == userId, ct);
        if (hasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword ?? "") == PasswordVerificationResult.Failed)
            throw AppException.Validation("invalid_current_password", "The current password is wrong.", "currentPassword");
        AccountRules.Password(request.NewPassword, user.Email, "newPassword");

        await SetPasswordAsync(user, request.NewPassword!, ct);
        return ToSignIn(user);
    }

    /// <summary>New hash and security stamp (signs out every other session), notification email.</summary>
    private async Task SetPasswordAsync(User user, string password, CancellationToken ct)
    {
        user.PasswordHash = hasher.HashPassword(user, password);
        user.SecurityStamp = User.NewStamp();
        await db.SaveChangesAsync(ct);
        await tokens.RevokeAsync(user.Id, TokenPurpose.PasswordReset, ct);

        if (Email.Enabled)
            outbox.Enqueue(EmailTemplates.PasswordChanged(user.Email, user.DisplayName, Link("forgot-password"), user.Locale));
    }

    private async Task SendConfirmationAsync(User user, CancellationToken ct)
    {
        var token = await tokens.IssueAsync(user.Id, TokenPurpose.EmailConfirmation, ct);
        outbox.Enqueue(EmailTemplates.Confirmation(user.Email, user.DisplayName, Link("confirm-email", token), user.Locale));
    }

    private string Link(string path, string? token = null) =>
        $"{appOptions.Value.PublicUrl.TrimEnd('/')}/{path}" + (token is null ? "" : $"?token={Uri.EscapeDataString(token)}");

    private void RequireEmail()
    {
        if (!Email.Enabled)
            throw AppException.Conflict("email_disabled", "Email is not configured on this server.");
    }

    private static SignIn ToSignIn(User user) =>
        new(new AccountDto(user.Id, user.Email, user.DisplayName, user.EmailConfirmedAt is not null, user.Locale,
            Avatars.Url(user.Id, user.AvatarUpdatedAt)), user.SecurityStamp);

    private static AppException EmailTaken() =>
        AppException.Conflict("email_taken", "An account with this email already exists.", "email");

    private static AppException InvalidCredentials() =>
        AppException.Unauthorized("invalid_credentials", "Wrong email or password.");
}
