namespace Republic.Core.World.Services;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Service interface for sovereign country founding, registry, and statehood management.
/// </summary>
public interface ICountryService
{
    /// <summary>
    /// Gets the authoritative Republic simulation clock used to establish founding moments.
    /// </summary>
    IRepublicClock Clock { get; }

    /// <summary>
    /// Founds and registers a new sovereign country, capturing the authoritative founding moment from the clock.
    /// </summary>
    Country FoundCountry(string name, string capitalCity = "", string governmentType = "Democratic Republic", string id = "");

    /// <summary>
    /// Registers a country entity, establishing its founding moment if not already sealed.
    /// </summary>
    Country RegisterCountry(Country country);

    /// <summary>
    /// Retrieves a registered country by its unique identifier.
    /// </summary>
    Country? GetCountry(string id);

    /// <summary>
    /// Retrieves all registered sovereign countries.
    /// </summary>
    IReadOnlyList<Country> GetAllCountries();

    /// <summary>
    /// Updates the baseline stability of a country.
    /// </summary>
    bool UpdateStability(string countryId, double delta);
}
