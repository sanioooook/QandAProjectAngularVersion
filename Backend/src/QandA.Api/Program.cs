using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QandA.Api.Account;
using QandA.Api.Auth;
using QandA.Api.Common;
using QandA.Api.Data;
using QandA.Api.Domain;
using QandA.Api.Email;
using QandA.Api.Surveys;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(config.GetConnectionString("Default")));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ParticipationPolicy>();
builder.Services.AddScoped<SurveyService>();
builder.Services.AddScoped<AccountService>();

builder.Services.Configure<AppOptions>(config.GetSection(AppOptions.Section));
builder.Services.Configure<EmailOptions>(config.GetSection(EmailOptions.Section));
if (config.GetSection(EmailOptions.Section).Get<EmailOptions>() is { Enabled: true })
{
    builder.Services.AddSingleton<ChannelEmailOutbox>();
    builder.Services.AddSingleton<IEmailOutbox>(sp => sp.GetRequiredService<ChannelEmailOutbox>());
    builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
    builder.Services.AddHostedService<EmailDispatcher>();
}
else
{
    builder.Services.AddSingleton<IEmailOutbox, DisabledEmailOutbox>();
}

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)))
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = ValidationResponse.Create);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<AppExceptionHandler>();

if (config["DataProtection:KeysPath"] is { Length: > 0 } keysPath)
    builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keysPath));

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = "qanda.auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        o.ExpireTimeSpan = TimeSpan.FromDays(14);
        o.SlidingExpiration = true;
        // This is an API: answer with status codes instead of redirecting to a login page.
        o.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
        o.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
        // Reject cookies of deleted users and cookies issued before the last password change.
        o.Events.OnValidatePrincipal = async ctx =>
        {
            var auth = ctx.HttpContext.RequestServices.GetRequiredService<AuthService>();
            var id = ctx.Principal?.FindUserId();
            var stamp = id is null ? null : await auth.StampAsync(id.Value, ctx.HttpContext.RequestAborted);
            if (stamp is null || stamp != ctx.Principal!.FindFirstValue(AuthController.StampClaim))
            {
                ctx.RejectPrincipal();
                await ctx.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });
builder.Services.AddAuthorization(o =>
    o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

var authPermitLimit = config.GetValue("RateLimiting:AuthPermitLimit", 10);
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy(RateLimits.Auth, ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = authPermitLimit, Window = TimeSpan.FromMinutes(1) }));
});

// The API runs behind the frontend's nginx inside the compose network.
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
});

builder.Services.AddHealthChecks();

var app = builder.Build();

if (config.GetValue("Database:MigrateOnStartup", true))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

public partial class Program;
