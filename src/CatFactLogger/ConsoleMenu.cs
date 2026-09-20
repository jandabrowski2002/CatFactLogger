using System.Text.Json;
using CatFactLogger.Models;
using CatFactLogger.Services;
using Spectre.Console;

namespace CatFactLogger;

/// <summary>
/// Drives the interactive menu. Depends only on interfaces, so it can be
/// unit-tested with fakes if needed, and knows nothing about HTTP or file I/O
/// details.
/// </summary>
public sealed class ConsoleMenu
{
    private const string FetchFact = "Fetch a new cat fact";
    private const string ViewSaved = "View saved facts";
    private const string ViewProfile = "My profile";
    private const string Exit = "Exit";

    private readonly ICatFactClient _catFactClient;
    private readonly IFactStorage _factStorage;
    private readonly IProfileProvider _profileProvider;
    private readonly IProfileRenderer _profileRenderer;

    public ConsoleMenu(
        ICatFactClient catFactClient,
        IFactStorage factStorage,
        IProfileProvider profileProvider,
        IProfileRenderer profileRenderer)
    {
        _catFactClient = catFactClient;
        _factStorage = factStorage;
        _profileProvider = profileProvider;
        _profileRenderer = profileRenderer;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        AnsiConsole.Write(new FigletText("Cat Facts").Color(Color.Orange1));

        while (!cancellationToken.IsCancellationRequested)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What would you like to do?")
                    .AddChoices(FetchFact, ViewSaved, ViewProfile, Exit));

            AnsiConsole.WriteLine();

            switch (choice)
            {
                case FetchFact:
                    await FetchAndSaveFactAsync(cancellationToken);
                    break;
                case ViewSaved:
                    await ShowSavedFactsAsync(cancellationToken);
                    break;
                case ViewProfile:
                    await ShowProfileAsync(cancellationToken);
                    break;
                case Exit:
                    return;
            }

            AnsiConsole.WriteLine();
        }
    }

    private async Task FetchAndSaveFactAsync(CancellationToken cancellationToken)
    {
        var fact = await AnsiConsole.Status()
            .StartAsync("Fetching a cat fact...", _ => _catFactClient.GetFactAsync(cancellationToken));

        if (fact is null)
        {
            AnsiConsole.MarkupLine("[red]Could not fetch a cat fact. Check your connection and try again.[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[green]Fact:[/] {Markup.Escape(fact.Fact)} [grey]({fact.Length} chars)[/]");

        try
        {
            await _factStorage.AppendAsync(fact, cancellationToken);
            AnsiConsole.MarkupLine("[grey]Saved to file.[/]");
        }
        catch (IOException)
        {
            AnsiConsole.MarkupLine("[red]Fetched the fact, but could not save it to the file.[/]");
        }
    }

    private async Task ShowSavedFactsAsync(CancellationToken cancellationToken)
    {
        var lines = await _factStorage.ReadAllAsync(cancellationToken);

        if (lines.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No facts saved yet.[/]");
            return;
        }

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("#");
        table.AddColumn("Fact");
        table.AddColumn("Length");

        for (var i = 0; i < lines.Count; i++)
        {
            try
            {
                var fact = JsonSerializer.Deserialize<CatFact>(lines[i]);
                table.AddRow(
                    (i + 1).ToString(),
                    Markup.Escape(fact?.Fact ?? lines[i]),
                    fact?.Length.ToString() ?? "-");
            }
            catch (JsonException)
            {
                table.AddRow((i + 1).ToString(), Markup.Escape(lines[i]), "-");
            }
        }

        AnsiConsole.Write(table);
    }

    private async Task ShowProfileAsync(CancellationToken cancellationToken)
    {
        try
        {
            var profile = await _profileProvider.GetProfileAsync(cancellationToken);
            _profileRenderer.Render(profile);
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            AnsiConsole.MarkupLine("[red]Could not load profile.json.[/]");
        }
    }
}
