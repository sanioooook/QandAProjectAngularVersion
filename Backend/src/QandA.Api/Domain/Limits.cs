namespace QandA.Api.Domain;

/// <summary>Validation limits shared by the database schema and request validation.</summary>
public static class Limits
{
    public const int EmailMax = 254;
    public const int DisplayNameMin = 2;
    public const int DisplayNameMax = 50;
    /// <summary>The client crops and compresses avatars to 256x256 before upload, so this is generous.</summary>
    public const int AvatarMaxBytes = 512 * 1024;
    public const int PasswordMin = 8;
    public const int PasswordMax = 128;

    public const int TitleMax = 200;
    public const int DescriptionMax = 1000;
    public const int OptionTextMax = 200;
    public const int OptionsMin = 2;
    public const int OptionsMax = 30;
    public const int MaxVotesPerUserCap = 30;
    public const int MaxOptionsPerParticipantCap = 10;

    public const int PageSizeDefault = 20;
    public const int PageSizeMax = 50;
}
