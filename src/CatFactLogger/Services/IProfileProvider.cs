using CatFactLogger.Models;

namespace CatFactLogger.Services;

public interface IProfileProvider
{
    Task<ProfileModel> GetProfileAsync(CancellationToken cancellationToken = default);
}
