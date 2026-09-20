using CatFactLogger.Models;

namespace CatFactLogger.Services;

public interface IFactStorage
{
    /// <summary>
    /// Appends one fact as a new line in the output file, creating the file
    /// if it doesn't exist yet.
    /// </summary>
    Task AppendAsync(CatFact fact, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads back every line saved so far (empty list if the file doesn't exist).
    /// </summary>
    Task<IReadOnlyList<string>> ReadAllAsync(CancellationToken cancellationToken = default);
}
