namespace CatFactLogger.Models;

/// <summary>
/// CV/profile data shown by the "My profile" screen. Loaded from profile.json,
/// so it can be edited without touching any code.
/// </summary>
public sealed record ProfileModel(
    string FullName,
    string Headline,
    string About,
    ContactInfo Contact,
    List<SkillEntry> Skills,
    List<ExperienceEntry> Experience,
    List<string> Education);

public sealed record ContactInfo(
    string Email,
    string? GitHub,
    string? LinkedIn,
    string? Phone);

public sealed record SkillEntry(
    string Name,
    string Level,
    string? YearsUsed);

public sealed record ExperienceEntry(
    string Role,
    string Company,
    string Period,
    List<string> Highlights);
