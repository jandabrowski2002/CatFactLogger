[Wersja polska](README.pl.md)

# Cat Fact Logger

A .NET 10 console app built for a recruitment task for NETWISE. It calls the [Cat Facts API](https://catfact.ninja/fact),
appends every response to a local `.txt` file as a new line, and includes an
extra "My profile" screen showing my CV.

![Cat Fact Logger screenshot 1](docs/screenshot1.png)
*Main menu*

![Cat Fact Logger screenshot 2](docs/screenshot2.png)
*Fetching and saving a cat fact*

![Cat Fact Logger screenshot 3](docs/screenshot3.png)
*The "My profile" screen*

## What it does

- **Fetch a new cat fact** — calls `GET https://catfact.ninja/fact`, prints the
  result, and appends it as one JSON line to `cat-facts.txt` **on the current
  user's Desktop**. The file is created automatically on the first write, so
  it works the same way for anyone who runs the app, on any machine.
- **View saved facts** — reads `cat-facts.txt` back and displays it as a table.
- **My profile** — renders my CV (from `profile.json`) as panels, a skills
  table and an experience tree, using [Spectre.Console](https://spectreconsole.net/).

## Requirements

- .NET 10 SDK

## Running it

### Visual Studio
Open `CatFactLogger.sln`, set `CatFactLogger` (under `src`) as the startup
project, and run (F5 / Ctrl+F5).

### Command line
```
dotnet run --project src/CatFactLogger
```

### Running the tests
```
dotnet test
```

## Project structure

```
src/CatFactLogger/
  Models/            CatFact, ProfileModel and related DTOs
  Options/           Strongly-typed configuration (CatFactOptions, ProfileOptions)
  Services/          ICatFactClient / CatFactClient, IFactStorage / FileFactStorage,
                      IProfileProvider / JsonProfileProvider, IProfileRenderer / SpectreProfileRenderer
  ConsoleMenu.cs     Drives the interactive menu; depends only on interfaces
  Program.cs         Composition root: builds the host, configures DI
  appsettings.json   API URL, output file path, profile file path
  profile.json       My CV content, rendered by the profile screen

tests/CatFactLogger.Tests/
  CatFactClientTests.cs      HTTP client tested against a fake HttpMessageHandler (no real network calls)
  FileFactStorageTests.cs    File storage tested against temp files
```

## Why these choices

- **Dependency Injection** via the .NET Generic Host (`Host.CreateApplicationBuilder`).
  Every service is behind an interface (`ICatFactClient`, `IFactStorage`,
  `IProfileProvider`, `IProfileRenderer`), registered in `Program.cs`.
- **`HttpClient` via `AddHttpClient`**, not `new HttpClient()`, so connections
  are pooled correctly and a timeout is configured centrally.
- **Configuration over hardcoding**: the API URL, output file path and profile
  file path all live in `appsettings.json` and are bound to typed options
  classes with the options pattern, rather than being hardcoded. A relative
  `OutputFilePath` (the default) is resolved against the current user's
  Desktop at runtime (`Environment.SpecialFolder.Desktop`), so the same
  config works on any machine; an absolute path in the config overrides that.
- **Error handling**: network failures, non-success HTTP status codes, and
  malformed JSON are all caught and logged; the app reports a friendly message
  and keeps running instead of crashing.
- **Separation of concerns**: fetching data, saving data, loading profile data
  and rendering the profile are separate, independently testable services.
  `ConsoleMenu` only orchestrates them.

## About me

See the "My profile" option in the app, or `src/CatFactLogger/profile.json` directly.
