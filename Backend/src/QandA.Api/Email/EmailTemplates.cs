using System.Net;

namespace QandA.Api.Email;

/// <summary>Plain-text and HTML bodies of the emails, in the recipient's language (uk, en, ru).</summary>
public static class EmailTemplates
{
    private record Texts(string Subject, string Greeting, string[] Lines, string Button, string Footer);

    public static EmailMessage Confirmation(string to, string name, string link, string locale) => Build(to, name, link, locale switch
    {
        "en" => new Texts("Confirm your email for QandA", "Hi, {0}!",
            ["Thanks for signing up for QandA.", "Confirm your email address to vote and create surveys."],
            "Confirm email", "The link is valid for 3 days. If you did not sign up, just ignore this email."),
        "ru" => new Texts("Подтвердите email в QandA", "Привет, {0}!",
            ["Спасибо за регистрацию в QandA.", "Подтвердите адрес, чтобы голосовать и создавать опросы."],
            "Подтвердить email", "Ссылка действует 3 дня. Если вы не регистрировались, просто проигнорируйте письмо."),
        _ => new Texts("Підтвердіть email у QandA", "Привіт, {0}!",
            ["Дякуємо за реєстрацію в QandA.", "Підтвердіть адресу, щоб голосувати та створювати опитування."],
            "Підтвердити email", "Посилання дійсне 3 дні. Якщо ви не реєструвалися, просто проігноруйте цей лист."),
    });

    public static EmailMessage PasswordReset(string to, string name, string link, string locale) => Build(to, name, link, locale switch
    {
        "en" => new Texts("Reset your QandA password", "Hi, {0}!",
            ["Someone asked to reset the password of your QandA account.", "If it was you, choose a new password:"],
            "Choose a new password", "The link is valid for 1 hour and works once. If it was not you, ignore this email: your password stays the same."),
        "ru" => new Texts("Сброс пароля в QandA", "Привет, {0}!",
            ["Кто-то запросил сброс пароля для вашего аккаунта QandA.", "Если это были вы, задайте новый пароль:"],
            "Задать новый пароль", "Ссылка действует 1 час и срабатывает один раз. Если это были не вы, проигнорируйте письмо: пароль не изменится."),
        _ => new Texts("Скидання пароля в QandA", "Привіт, {0}!",
            ["Хтось попросив скинути пароль вашого акаунта QandA.", "Якщо це були ви, задайте новий пароль:"],
            "Задати новий пароль", "Посилання дійсне 1 годину і спрацьовує один раз. Якщо це були не ви, проігноруйте лист: пароль не зміниться."),
    });

    public static EmailMessage PasswordChanged(string to, string name, string resetLink, string locale) => Build(to, name, resetLink, locale switch
    {
        "en" => new Texts("Your QandA password was changed", "Hi, {0}!",
            ["The password of your QandA account was just changed and other sessions were signed out.", "If it was not you, reset the password right away:"],
            "Reset password", "If it was you, there is nothing to do."),
        "ru" => new Texts("Пароль в QandA изменён", "Привет, {0}!",
            ["Пароль вашего аккаунта QandA только что изменён, другие сессии завершены.", "Если это были не вы, сразу сбросьте пароль:"],
            "Сбросить пароль", "Если это были вы, ничего делать не нужно."),
        _ => new Texts("Пароль у QandA змінено", "Привіт, {0}!",
            ["Пароль вашого акаунта QandA щойно змінено, інші сесії завершено.", "Якщо це були не ви, одразу скиньте пароль:"],
            "Скинути пароль", "Якщо це були ви, нічого робити не потрібно."),
    });

    private static EmailMessage Build(string to, string name, string link, Texts texts)
    {
        var greeting = string.Format(texts.Greeting, name);
        var text = string.Join("\n\n", [greeting, .. texts.Lines, link, texts.Footer]);

        Func<string, string> e = s => WebUtility.HtmlEncode(s);
        var paragraphs = string.Concat(texts.Lines.Select(line => $"<p>{e(line)}</p>"));
        var html = $"""
            <div style="font-family:Segoe UI,Arial,sans-serif;font-size:15px;line-height:1.5;color:#171a23;max-width:520px">
              <p><strong>{e(greeting)}</strong></p>
              {paragraphs}
              <p><a href="{e(link)}" style="display:inline-block;padding:10px 18px;border-radius:8px;background:#4f46e5;color:#fff;text-decoration:none;font-weight:600">{e(texts.Button)}</a></p>
              <p style="color:#5d6475;font-size:13px">{e(texts.Footer)}<br><a href="{e(link)}" style="color:#5d6475">{e(link)}</a></p>
            </div>
            """;
        return new EmailMessage(to, texts.Subject, text, html);
    }
}
