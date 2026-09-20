using CatFactLogger.Models;

namespace CatFactLogger.Services;

public interface ICatFactClient
{
    /// <summary>
    /// Calls the cat fact API. Returns null (and logs) if the call fails or the
    /// response can't be parsed, so callers don't have to deal with exceptions.
    /// </summary>
    Task<CatFact?> GetFactAsync(CancellationToken cancellationToken = default);
}
