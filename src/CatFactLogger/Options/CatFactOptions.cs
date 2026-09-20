namespace CatFactLogger.Options;

/// <summary>
/// Bound from the "CatFact" section of appsettings.json.
/// </summary>
public sealed class CatFactOptions
{
    public const string SectionName = "CatFact";

    public required string ApiUrl { get; set; }

    public required string OutputFilePath { get; set; }
}
