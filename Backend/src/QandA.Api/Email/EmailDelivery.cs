using System.Threading.Channels;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace QandA.Api.Email;

public record EmailMessage(string To, string Subject, string Text, string Html);

/// <summary>Accepts messages without waiting for delivery: a slow or broken mail server never fails a request.</summary>
public interface IEmailOutbox
{
    void Enqueue(EmailMessage message);
}

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct);
}

public sealed class ChannelEmailOutbox : IEmailOutbox
{
    private readonly Channel<EmailMessage> _channel = Channel.CreateUnbounded<EmailMessage>();

    public ChannelReader<EmailMessage> Reader => _channel.Reader;

    public void Enqueue(EmailMessage message) => _channel.Writer.TryWrite(message);
}

/// <summary>Sends queued emails in the background, retrying a few times before giving up.</summary>
public sealed class EmailDispatcher(ChannelEmailOutbox outbox, IEmailSender sender, ILogger<EmailDispatcher> logger)
    : BackgroundService
{
    private static readonly TimeSpan[] RetryDelays = [TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30)];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in outbox.Reader.ReadAllAsync(stoppingToken))
        {
            for (var attempt = 0; ; attempt++)
            {
                try
                {
                    await sender.SendAsync(message, stoppingToken);
                    break;
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    if (attempt >= RetryDelays.Length)
                    {
                        logger.LogError(e, "Giving up sending \"{Subject}\" to {To}", message.Subject, message.To);
                        break;
                    }
                    logger.LogWarning(e, "Sending \"{Subject}\" failed, retrying", message.Subject);
                    await Task.Delay(RetryDelays[attempt], stoppingToken);
                }
            }
        }
    }
}

public sealed class SmtpEmailSender(IOptions<EmailOptions> options) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        var config = options.Value;
        var mime = new MimeMessage();
        mime.From.Add(MailboxAddress.Parse(config.From));
        mime.To.Add(MailboxAddress.Parse(message.To));
        mime.Subject = message.Subject;
        mime.Body = new BodyBuilder { TextBody = message.Text, HtmlBody = message.Html }.ToMessageBody();

        using var client = new SmtpClient();
        var security = Enum.TryParse<SecureSocketOptions>(config.Smtp.Security, ignoreCase: true, out var parsed)
            ? parsed
            : SecureSocketOptions.Auto;
        await client.ConnectAsync(config.Smtp.Host!, config.Smtp.Port, security, ct);
        if (!string.IsNullOrEmpty(config.Smtp.Username))
            await client.AuthenticateAsync(config.Smtp.Username, config.Smtp.Password ?? "", ct);
        await client.SendAsync(mime, ct);
        await client.DisconnectAsync(true, ct);
    }
}

/// <summary>Used when email is disabled: nothing leaves the server.</summary>
public sealed class DisabledEmailOutbox(ILogger<DisabledEmailOutbox> logger) : IEmailOutbox
{
    public void Enqueue(EmailMessage message) =>
        logger.LogDebug("Email is disabled; not sending \"{Subject}\"", message.Subject);
}
