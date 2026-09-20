using System.Net.Http.Json;
using System.Text.Json;
using CatFactLogger.Models;
using Microsoft.Extensions.Logging;

namespace CatFactLogger.Services;

/// <summary>
/// Talks to https://catfact.ninja/fact. Registered with AddHttpClient so the
/// HttpClient itself is pooled and managed by the framework rather than us.
/// </summary>
public sealed class CatFactClient : ICatFactClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CatFactClient> _logger;

    public CatFactClient(HttpClient httpClient, ILogger<CatFactClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CatFact?> GetFactAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // BaseAddress is set to the full API URL, so an empty relative URI
            // is enough to hit it.
            using var response = await _httpClient.GetAsync(string.Empty, cancellationToken);
            response.EnsureSuccessStatusCode();

            var fact = await response.Content.ReadFromJsonAsync<CatFact>(cancellationToken: cancellationToken);
            if (fact is null)
            {
                _logger.LogWarning("Cat fact response body could not be deserialized.");
            }

            return fact;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach the cat fact API.");
            return null;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "The request to the cat fact API timed out.");
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "The cat fact API returned a response that could not be parsed.");
            return null;
        }
    }
}
