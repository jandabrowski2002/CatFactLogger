using CatFactLogger.Models;
using CatFactLogger.Options;
using CatFactLogger.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CatFactLogger.Tests;

public class FileFactStorageTests : IDisposable
{
    private readonly string _tempFile = Path.Combine(Path.GetTempPath(), $"catfacts-{Guid.NewGuid():N}.txt");

    [Fact]
    public async Task AppendAsync_CreatesFile_WhenItDoesNotExistYet()
    {
        var sut = CreateStorage();

        await sut.AppendAsync(new CatFact("Test fact", 9));

        Assert.True(File.Exists(_tempFile));
    }

    [Fact]
    public async Task AppendAsync_AddsOneLinePerCall()
    {
        var sut = CreateStorage();

        await sut.AppendAsync(new CatFact("First fact", 10));
        await sut.AppendAsync(new CatFact("Second fact", 11));

        var lines = await File.ReadAllLinesAsync(_tempFile);
        Assert.Equal(2, lines.Length);
        Assert.Contains("First fact", lines[0]);
        Assert.Contains("Second fact", lines[1]);
    }

    [Fact]
    public async Task ReadAllAsync_ReturnsEmptyList_WhenFileDoesNotExist()
    {
        var sut = CreateStorage();

        var lines = await sut.ReadAllAsync();

        Assert.Empty(lines);
    }

    [Fact]
    public async Task ReadAllAsync_ReturnsSavedLines()
    {
        var sut = CreateStorage();
        await sut.AppendAsync(new CatFact("Only fact", 9));

        var lines = await sut.ReadAllAsync();

        Assert.Single(lines);
        Assert.Contains("Only fact", lines[0]);
    }

    private FileFactStorage CreateStorage()
    {
        var options = Options.Create(new CatFactOptions
        {
            ApiUrl = "https://catfact.ninja/fact",
            OutputFilePath = _tempFile
        });

        return new FileFactStorage(options, NullLogger<FileFactStorage>.Instance);
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
        {
            File.Delete(_tempFile);
        }
    }
}
