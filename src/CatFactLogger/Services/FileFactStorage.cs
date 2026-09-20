using System.Text.Json;
using CatFactLogger.Models;
using CatFactLogger.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CatFactLogger.Services;

/// <summary>
/// Appends each fact as one line of JSON to a local .txt file. File.AppendAllTextAsync
/// creates the file automatically the first time it's called, which covers the
/// "create the file locally" requirement without any extra code.
/// </summary>
public sealed class FileFactStorage : IFactStorage
{
    // Guards concurrent writes to the same file from overlapping calls.
    private static readonly SemaphoreSlim WriteLock = new(1, 1);

    private readonly string _filePath;
    private readonly ILogger<FileFactStorage> _logger;

    public FileFactStorage(IOptions<CatFactOptions> options, ILogger<FileFactStorage> logger)
    {
        _filePath = ResolvePath(options.Value.OutputFilePath);
        _logger = logger;
    }

    public async Task AppendAsync(CatFact fact, CancellationToken cancellationToken = default)
    {
        var line = JsonSerializer.Serialize(fact);

        await WriteLock.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(_filePath, line + Environment.NewLine, cancellationToken);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to write the fact to {FilePath}.", _filePath);
            throw;
        }
        finally
        {
            WriteLock.Release();
        }
    }

    public async Task<IReadOnlyList<string>> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return Array.Empty<string>();
        }

        var lines = await File.ReadAllLinesAsync(_filePath, cancellationToken);
        return lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
    }

    private static string ResolvePath(string configuredPath) =>
    Path.IsPathRooted(configuredPath)
        ? configuredPath
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), configuredPath);
}
