namespace QandA.Api.Common;

public static class RateLimits
{
    /// <summary>Login / registration: limited per client IP to slow down password guessing.</summary>
    public const string Auth = "auth";
}
