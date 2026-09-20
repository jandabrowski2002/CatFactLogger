using System.Text.Json;
using CatFactLogger.Models;
using CatFactLogger.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CatFactLogger.Services;

/// <summary>
/// Loads CV/profile data from profile.json, so the content can be edited
/// without touching any C# code.
/// </summary>
public sealed class JsonProfileProvider : IProfileProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _filePath;
    private readonly ILogger<JsonProfileProvider> _logger;

    public JsonProfileProvider(IOptions<ProfileOptions> options, ILogger<JsonProfileProvider> logger)
    {
        _filePath = ResolvePath(options.Value.FilePath);
        _logger = logger;
    }

    public async Task<ProfileModel> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var stream = File.OpenRead(_filePath);
            var profile = await JsonSerializer.DeserializeAsync<ProfileModel>(stream, SerializerOptions, cancellationToken);
            return profile ?? throw new InvalidOperationException($"{_filePath} is empty or invalid.");
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            _logger.LogError(ex, "Failed to load profile data from {FilePath}.", _filePath);
            throw;
        }
    }

    private static string ResolvePath(string configuredPath) =>
        Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(AppContext.BaseDirectory, configuredPath);
}
