using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Npgsql;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using QandA.Api.Email;
using QandA.Api.Data;

namespace QandA.Api.Tests.Infrastructure;

/// <summary>
/// Runs the real API against a throw-away PostgreSQL database (one per test class).
/// The server is taken from TEST_DB_CONNECTION, which docker compose sets for the test container.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Starts at the real "now": the cookie handler also uses this clock for the cookie expiry,
    // and the HttpClient cookie container drops cookies that look expired in real time.
    public static readonly DateTimeOffset Start = DateTimeOffset.UtcNow;

    private readonly string _connectionString;

    public ApiFactory()
    {
        var server = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Username=qanda;Password=qanda";
        _connectionString = new NpgsqlConnectionStringBuilder(server) { Database = $"qanda_test_{Guid.NewGuid():N}" }.ConnectionString;
    }

    public FakeTimeProvider Clock { get; } = new(Start);

    protected virtual int AuthPermitLimit => 10_000;

    /// <summary>With email enabled, outgoing messages land in <see cref="Outbox"/> instead of an SMTP server.</summary>
    protected virtual bool EmailEnabled => false;

    public CapturingOutbox Outbox { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.UseSetting("ConnectionStrings:Default", _connectionString);
        builder.UseSetting("RateLimiting:AuthPermitLimit", AuthPermitLimit.ToString());
        builder.UseSetting("App:PublicUrl", "http://qanda.test");
        if (EmailEnabled)
            builder.UseSetting("Email:Smtp:Host", "smtp.invalid");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
            if (EmailEnabled)
            {
                services.RemoveAll<IEmailOutbox>();
                services.AddSingleton<IEmailOutbox>(Outbox);
            }
        });
    }

    public ValueTask InitializeAsync()
    {
        // Touching Services starts the host, which applies the migrations.
        _ = Services;
        return ValueTask.CompletedTask;
    }

    public async Task<T> WithDbAsync<T>(Func<AppDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    public override async ValueTask DisposeAsync()
    {
        await WithDbAsync(db => db.Database.EnsureDeletedAsync());
        await base.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}

/// <summary>API with email delivery configured; messages are captured by <see cref="ApiFactory.Outbox"/>.</summary>
public class EmailApiFactory : ApiFactory
{
    protected override bool EmailEnabled => true;
}

public class CapturingOutbox : IEmailOutbox
{
    private readonly ConcurrentQueue<EmailMessage> _messages = new();

    public IReadOnlyList<EmailMessage> Messages => [.. _messages];

    public void Enqueue(EmailMessage message) => _messages.Enqueue(message);

    public EmailMessage[] To(string email) =>
        _messages.Where(m => string.Equals(m.To, email, StringComparison.OrdinalIgnoreCase)).ToArray();

    /// <summary>Token from the link in the last email of the given kind sent to <paramref name="email"/>.</summary>
    public string LastToken(string email, string path)
    {
        var message = To(email).Last(m => m.Text.Contains($"/{path}?token="));
        var match = Regex.Match(message.Text, $@"/{path}\?token=([^\s]+)");
        return Uri.UnescapeDataString(match.Groups[1].Value);
    }
}

/// <summary>Same API with a tiny login rate limit, for the throttling test.</summary>
public class ThrottledApiFactory : ApiFactory
{
    public const int Limit = 3;
    protected override int AuthPermitLimit => Limit;
}
