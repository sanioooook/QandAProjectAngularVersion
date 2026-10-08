namespace QandA.Api.Common;

/// <summary>
/// Expected business error. <see cref="Code"/> is a stable machine-readable key the
/// frontend translates; the message is an English fallback.
/// </summary>
public class AppException(int status, string code, string message, string? field = null) : Exception(message)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
    public string? Field { get; } = field;

    public static AppException Validation(string code, string message, string? field = null) => new(400, code, message, field);
    public static AppException Unauthorized(string code, string message) => new(401, code, message);
    public static AppException Forbidden(string message = "You are not allowed to do this.") => new(403, "forbidden", message);
    public static AppException NotFound(string message = "Not found.") => new(404, "not_found", message);
    public static AppException Conflict(string code, string message, string? field = null) => new(409, code, message, field);
}
