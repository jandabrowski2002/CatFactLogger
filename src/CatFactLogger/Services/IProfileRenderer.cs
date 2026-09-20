using CatFactLogger.Models;

namespace CatFactLogger.Services;

public interface IProfileRenderer
{
    void Render(ProfileModel profile);
}
