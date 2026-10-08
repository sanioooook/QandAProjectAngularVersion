using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QandA.Api.Auth;
using QandA.Api.Common;
using QandA.Api.Domain;

namespace QandA.Api.Account;

[ApiController]
[Route("api/account")]
public class AccountController(AccountService account) : ControllerBase
{
    [HttpPut("profile")]
    public Task<AccountDto> UpdateProfile(UpdateProfileRequest request, CancellationToken ct) =>
        account.UpdateProfileAsync(User.GetUserId(), request, ct);

    /// <summary>multipart/form-data with a single "file" field.</summary>
    [HttpPut("avatar")]
    [RequestSizeLimit(Limits.AvatarMaxBytes + 16 * 1024)]
    public async Task<AccountDto> SetAvatar(IFormFile? file, CancellationToken ct)
    {
        await using var stream = file?.OpenReadStream() ?? Stream.Null;
        return await account.SetAvatarAsync(User.GetUserId(), stream, file?.Length ?? 0, ct);
    }

    [HttpDelete("avatar")]
    public Task<AccountDto> RemoveAvatar(CancellationToken ct) => account.RemoveAvatarAsync(User.GetUserId(), ct);
}

[ApiController]
[Route("api/users")]
public class UsersController(AccountService account) : ControllerBase
{
    /// <summary>Public, like the name next to it. The URL carries a version, so it is cached for a year.</summary>
    [HttpGet("{id:int}/avatar")]
    [AllowAnonymous]
    public async Task<IActionResult> Avatar(int id, CancellationToken ct)
    {
        var avatar = await account.GetAvatarAsync(id, ct);
        if (avatar is null)
            return NotFound();
        Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(avatar.Data, avatar.ContentType);
    }
}
