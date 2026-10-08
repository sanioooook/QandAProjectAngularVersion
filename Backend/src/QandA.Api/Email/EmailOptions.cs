namespace QandA.Api.Email;

/// <summary>
/// Email is optional: without <c>Email:Smtp:Host</c> nothing is sent, registration does not need a
/// confirmation and the password-reset flow is unavailable. Everything else works the same.
/// </summary>
public class EmailOptions
{
    public const string Section = "Email";

    public SmtpOptions Smtp { get; set; } = new();

    public string From { get; set; } = "QandA <noreply@qanda.local>";

    /// <summary>When email is enabled, users must confirm their address before voting or creating surveys.</summary>
    public bool RequireConfirmation { get; set; } = true;

    public bool Enabled => !string.IsNullOrWhiteSpace(Smtp.Host);

    public bool ConfirmationRequired => Enabled && RequireConfirmation;
}

public class SmtpOptions
{
    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public string? Username { get; set; }
    public string? Password { get; set; }
    /// <summary>None, StartTls, SslOnConnect or Auto (MailKit SecureSocketOptions).</summary>
    public string Security { get; set; } = "Auto";
}

public class AppOptions
{
    public const string Section = "App";

    /// <summary>Address of the web app as users open it; used to build links in emails.</summary>
    public string PublicUrl { get; set; } = "http://localhost:8080";
}
