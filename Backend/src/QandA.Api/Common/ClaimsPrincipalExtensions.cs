using System.Security.Claims;

namespace QandA.Api.Common;

public static class ClaimsPrincipalExtensions
{
    /// <summary>The signed-in user's id, or null for guests.</summary>
    public static int? FindUserId(this ClaimsPrincipal principal) =>
        int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public static int GetUserId(this ClaimsPrincipal principal) =>
        int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw AppException.Unauthorized("unauthorized", "Not signed in.");
}
