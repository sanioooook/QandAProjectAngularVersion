using Microsoft.EntityFrameworkCore;
using QandA.Api.Auth;
using QandA.Api.Common;
using QandA.Api.Data;
using QandA.Api.Domain;

namespace QandA.Api.Account;

public record UpdateProfileRequest(string? DisplayName, string? Locale);

public record AvatarFile(string ContentType, byte[] Data);

public class AccountService(AppDbContext db, AuthService auth, TimeProvider clock)
{
    /// <summary>Only a missing field is left unchanged, so the client can update the name or the language alone.</summary>
    public async Task<AccountDto> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken ct)
    {
        var user = await db.Users.SingleAsync(u => u.Id == userId, ct);
        if (request.DisplayName is not null)
            user.DisplayName = AccountRules.DisplayName(request.DisplayName);
        if (request.Locale is not null)
            user.Locale = AccountRules.Locale(request.Locale);
        await db.SaveChangesAsync(ct);
        return (await auth.FindAsync(userId, ct))!;
    }

    public async Task<AccountDto> SetAvatarAsync(int userId, Stream content, long length, CancellationToken ct)
    {
        if (length == 0)
            throw AppException.Validation("avatar_invalid", "Choose a PNG, JPEG or WebP image.", "file");
        if (length > Limits.AvatarMaxBytes)
            throw AppException.Validation("avatar_too_large", $"The image may be at most {Limits.AvatarMaxBytes / 1024} KB.", "file");

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);
        var data = buffer.ToArray();
        // The declared content type and file name are ignored: the bytes decide what is stored and served.
        var contentType = Avatars.DetectImageType(data)
            ?? throw AppException.Validation("avatar_invalid", "Choose a PNG, JPEG or WebP image.", "file");

        var user = await db.Users.SingleAsync(u => u.Id == userId, ct);
        var avatar = await db.UserAvatars.SingleOrDefaultAsync(a => a.UserId == userId, ct);
        if (avatar is null)
            db.UserAvatars.Add(new UserAvatar { UserId = userId, ContentType = contentType, Data = data });
        else
        {
            avatar.ContentType = contentType;
            avatar.Data = data;
        }
        user.AvatarUpdatedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(ct);
        return (await auth.FindAsync(userId, ct))!;
    }

    public async Task<AccountDto> RemoveAvatarAsync(int userId, CancellationToken ct)
    {
        await db.UserAvatars.Where(a => a.UserId == userId).ExecuteDeleteAsync(ct);
        await db.Users.Where(u => u.Id == userId).ExecuteUpdateAsync(u => u.SetProperty(x => x.AvatarUpdatedAt, (DateTimeOffset?)null), ct);
        return (await auth.FindAsync(userId, ct))!;
    }

    public Task<AvatarFile?> GetAvatarAsync(int userId, CancellationToken ct) =>
        db.UserAvatars.AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => new AvatarFile(a.ContentType, a.Data))
            .SingleOrDefaultAsync(ct);
}

public static class Avatars
{
    /// <summary>Versioned URL: a new avatar gets a new URL, so the old one can be cached forever.</summary>
    public static string? Url(int userId, DateTimeOffset? updatedAt) =>
        updatedAt is null ? null : $"/api/users/{userId}/avatar?v={updatedAt.Value.ToUnixTimeMilliseconds()}";

    /// <summary>Content type from the file signature, or null when it is not a PNG, JPEG or WebP image.</summary>
    public static string? DetectImageType(ReadOnlySpan<byte> data)
    {
        if (data.StartsWith((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
            return "image/png";
        if (data.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xD8, 0xFF]))
            return "image/jpeg";
        if (data.Length >= 12 && data[..4].SequenceEqual("RIFF"u8) && data[8..12].SequenceEqual("WEBP"u8))
            return "image/webp";
        return null;
    }
}
