using System.Text.Json.Serialization;

namespace CatFactLogger.Models;

/// <summary>
/// Maps the JSON response from https://catfact.ninja/fact.
/// </summary>
public sealed record CatFact(
    [property: JsonPropertyName("fact")] string Fact,
    [property: JsonPropertyName("length")] int Length);
