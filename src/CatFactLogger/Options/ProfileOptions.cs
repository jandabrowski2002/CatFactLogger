namespace CatFactLogger.Options;

/// <summary>
/// Bound from the "Profile" section of appsettings.json.
/// </summary>
public sealed class ProfileOptions
{
    public const string SectionName = "Profile";

    public required string FilePath { get; set; }
}
