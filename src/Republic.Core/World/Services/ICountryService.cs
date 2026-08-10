namespace Republic.Core.World.Services;

using Republic.Core.World.Models;

/// <summary>
/// Service interface for nation entity registry and stability management.
/// </summary>
public interface ICountryService
{
    Country RegisterCountry(Country country);
    Country? GetCountry(string id);
    IReadOnlyList<Country> GetAllCountries();
    bool UpdateStability(string countryId, double delta);
}
