using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QandA.Api.Common;

namespace QandA.Api.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService auth) : ControllerBase
{
    public const string StampClaim = "qanda:stamp";

    [HttpGet("config")]
    [AllowAnonymous]
    public AuthConfigDto Config() => auth.Config();

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimits.Auth)]
    public async Task<ActionResult<AccountDto>> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await auth.RegisterAsync(request, ct);
        await SignInAsync(result);
        return CreatedAtAction(nameof(Me), result.Account);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimits.Auth)]
    public async Task<ActionResult<AccountDto>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await auth.LoginAsync(request, ct);
        await SignInAsync(result);
        return result.Account;
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    [HttpGet("me")]
    public async Task<ActionResult<AccountDto>> Me(CancellationToken ct) =>
        await auth.FindAsync(User.GetUserId(), ct) is { } account ? account : Unauthorized();

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimits.Auth)]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequest request, CancellationToken ct)
    {
        await auth.ConfirmEmailAsync(request, ct);
        return NoContent();
    }

    [HttpPost("resend-confirmation")]
    [EnableRateLimiting(RateLimits.Auth)]
    public async Task<IActionResult> ResendConfirmation(CancellationToken ct)
    {
        await auth.ResendConfirmationAsync(User.GetUserId(), ct);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimits.Auth)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
    {
        await auth.ForgotPasswordAsync(request, ct);
        return NoContent();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimits.Auth)]
    public async Task<ActionResult<AccountDto>> ResetPassword(ResetPasswordRequest request, CancellationToken ct)
    {
        var result = await auth.ResetPasswordAsync(request, ct);
        await SignInAsync(result);
        return result.Account;
    }

    /// <summary>Other sessions are signed out by the new security stamp; this one gets a fresh cookie.</summary>
    [HttpPost("change-password")]
    [EnableRateLimiting(RateLimits.Auth)]
    public async Task<ActionResult<AccountDto>> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        var result = await auth.ChangePasswordAsync(User.GetUserId(), request, ct);
        await SignInAsync(result);
        return result.Account;
    }

    private Task SignInAsync(SignIn result)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, result.Account.Id.ToString()), new Claim(StampClaim, result.SecurityStamp)],
            CookieAuthenticationDefaults.AuthenticationScheme);
        return HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
    }
}
