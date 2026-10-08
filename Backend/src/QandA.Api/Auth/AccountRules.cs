using System.Text.RegularExpressions;
using QandA.Api.Common;
using QandA.Api.Domain;

namespace QandA.Api.Auth;

/// <summary>Validation of account fields. Pure functions: they throw <see cref="AppException"/> with a stable code.</summary>
public static partial class AccountRules
{
    public static readonly string[] Locales = ["uk", "en", "ru"];

    // Deliberately loose: one @, something on both sides and a dot in the domain. Real validation is the confirmation email.
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s.]+$")]
    private static partial Regex EmailPattern();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    /// <summary>Trimmed email and its normalized (upper-case) form used for lookups.</summary>
    public static (string Email, string Normalized) Email(string? raw, string field = "email")
    {
        var email = (raw ?? "").Trim();
        if (email.Length == 0 || email.Length > Limits.EmailMax || !EmailPattern().IsMatch(email))
            throw AppException.Validation("email_invalid", "Enter a valid email address.", field);
        return (email, Normalize(email));
    }

    public static string Normalize(string? email) => (email ?? "").Trim().ToUpperInvariant();

    public static string DisplayName(string? raw)
    {
        var name = Whitespace().Replace((raw ?? "").Trim(), " ");
        if (name.Length < Limits.DisplayNameMin || name.Length > Limits.DisplayNameMax)
            throw AppException.Validation("name_length", $"Name must be {Limits.DisplayNameMin}-{Limits.DisplayNameMax} characters.", "displayName");
        return name;
    }

    public static void Password(string? password, string email, string field = "password")
    {
        password ??= "";
        if (password.Length < Limits.PasswordMin || password.Length > Limits.PasswordMax)
            throw AppException.Validation("password_length", $"Password must be {Limits.PasswordMin}-{Limits.PasswordMax} characters.", field);
        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            throw AppException.Validation("password_weak", "Password must contain at least one letter and one digit.", field);
        var localPart = email.Split('@')[0];
        if (string.Equals(password, email, StringComparison.OrdinalIgnoreCase)
            || string.Equals(password, localPart, StringComparison.OrdinalIgnoreCase))
            throw AppException.Validation("password_equals_email", "Password must differ from the email.", field);
    }

    public static string Locale(string? raw) =>
        Locales.FirstOrDefault(l => string.Equals(l, raw?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? "uk";
}
