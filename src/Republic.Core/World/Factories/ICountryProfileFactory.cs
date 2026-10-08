namespace Republic.Core.World.Factories;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Factory contract producing deterministic sovereign country profiles and country entities.
/// </summary>
public interface ICountryProfileFactory
{
    /// <summary>
    /// Generates a deterministic sovereign country profile using a pseudorandom seed.
    /// The same seed must always produce an identical profile.
    /// </summary>
    CountryProfile CreateProfile(int seed, RepublicTime foundingTime = default);

    /// <summary>
    /// Generates a fully initialized sovereign country with an attached deterministic profile.
    /// </summary>
    Country CreateCountry(int seed, RepublicTime foundingTime = default, string? id = null, double startingTreasury = 1_000_000.0);
}
