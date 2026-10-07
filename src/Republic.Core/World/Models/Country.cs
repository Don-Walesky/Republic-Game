namespace Republic.Core.World.Models;

using System.Text.Json.Serialization;
using Republic.Core.Economy.Treasury;
using Republic.Core.NationalYield;
using Republic.Core.Time;

/// <summary>
/// Domain model representing a sovereign nation entity.
/// Holds the authoritative national identity, constitution, and historical founding moment.
/// </summary>
public sealed class Country
{
    private RepublicTime _foundingTime;
    private bool _isFoundingSealed;

    /// <summary>
    /// Gets the unique identifier of the sovereign country.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets the official name of the sovereign nation.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the capital city.
    /// </summary>
    public string CapitalCity { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the constitutional system of governance.
    /// </summary>
    public string GovernmentType { get; set; } = "Democratic Republic";

    /// <summary>
    /// Gets or sets the sovereign land area in square kilometers.
    /// </summary>
    public double TerritorySizeSqKm { get; init; } = 500000;

    /// <summary>
    /// Gets the national traits and cultural characteristics.
    /// </summary>
    public List<string> NationalTraits { get; init; } = new();

    /// <summary>
    /// Gets or sets the baseline political and societal stability (0.0 to 100.0).
    /// </summary>
    public double BaselineStability { get; set; } = 75.0;

    /// <summary>
    /// Gets the authoritative National Yield state representing the country's productive, human, institutional, and resource capacities.
    /// Each sovereign country owns an independent, isolated instance.
    /// </summary>
    public NationalYield Yield { get; init; } = new();

    /// <summary>
    /// Gets the authoritative sovereign treasury holding accumulated liquid REPU balance.
    /// Each sovereign country owns an independent, isolated instance.
    /// </summary>
    public RepuTreasury Treasury { get; init; } = new();

    /// <summary>
    /// Gets the authoritative Republic simulation time at which this nation was founded.
    /// Init-only to prevent casual post-creation mutation.
    /// </summary>
    public RepublicTime FoundingTime
    {
        get => _foundingTime;
        init
        {
            _foundingTime = value;
            _isFoundingSealed = value.WatTimestamp != default;
        }
    }

    /// <summary>
    /// Gets or sets the foundational lifecycle status of the country.
    /// </summary>
    public CountryFoundingStatus FoundingStatus { get; set; } = CountryFoundingStatus.NewlyFounded;

    /// <summary>
    /// Gets a value indicating whether the founding moment has been authoritatively established.
    /// </summary>
    [JsonIgnore]
    public bool IsFounded => _isFoundingSealed || _foundingTime.WatTimestamp != default;

    /// <summary>
    /// Gets the Gregorian calendar date of national independence in West Africa Time (WAT).
    /// </summary>
    [JsonIgnore]
    public DateOnly IndependenceDay => FoundingTime.Date;

    /// <summary>
    /// Gets the time of day of national independence in West Africa Time (WAT).
    /// </summary>
    [JsonIgnore]
    public TimeOnly IndependenceTime => FoundingTime.Time;

    /// <summary>
    /// Gets the Republic simulation day number on which national independence occurred (e.g. Day 0, Day 42).
    /// </summary>
    [JsonIgnore]
    public long FoundingRepublicDay => FoundingTime.DayNumber;

    /// <summary>
    /// Gets a value indicating whether the country has transitioned from newly founded to established statehood.
    /// </summary>
    [JsonIgnore]
    public bool IsEstablished => FoundingStatus == CountryFoundingStatus.Established;

    /// <summary>
    /// Establishes the authoritative founding moment for an unsealed country.
    /// Throws <see cref="InvalidOperationException"/> if already established.
    /// </summary>
    public void EstablishFounding(RepublicTime foundingTime)
    {
        if (IsFounded)
        {
            throw new InvalidOperationException(
                $"The founding moment for country '{Name}' ({Id}) has already been authoritatively established and cannot be overwritten.");
        }

        if (foundingTime.WatTimestamp == default)
        {
            throw new ArgumentException("Cannot establish founding with an uninitialized Republic time.", nameof(foundingTime));
        }

        _foundingTime = foundingTime;
        _isFoundingSealed = true;
    }

    /// <summary>
    /// Marks the country as having achieved established statehood.
    /// </summary>
    public void MarkEstablished()
    {
        FoundingStatus = CountryFoundingStatus.Established;
    }

    /// <summary>
    /// Calculates the country's national age relative to the specified Republic simulation time.
    /// Returns <see cref="TimeSpan.Zero"/> if the simulation time is at or before founding, or if not founded yet.
    /// </summary>
    public TimeSpan GetAge(RepublicTime currentRepublicTime)
    {
        if (!IsFounded)
        {
            return TimeSpan.Zero;
        }

        var age = currentRepublicTime - FoundingTime;
        return age < TimeSpan.Zero ? TimeSpan.Zero : age;
    }

    /// <summary>
    /// Calculates the country's national age relative to the authoritative Republic clock.
    /// </summary>
    public TimeSpan GetAge(IRepublicClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return GetAge(clock.CurrentTime);
    }

    /// <summary>
    /// Factory method to create and found a new sovereign country with an authoritative founding moment.
    /// </summary>
    public static Country Found(
        string name,
        RepublicTime foundingTime,
        string id = "",
        string capitalCity = "",
        string governmentType = "Democratic Republic",
        double territorySizeSqKm = 500000,
        double baselineStability = 75.0,
        NationalYield? yield = null,
        RepuTreasury? treasury = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var countryId = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
        return new Country
        {
            Id = countryId,
            Name = name,
            FoundingTime = foundingTime,
            CapitalCity = capitalCity,
            GovernmentType = governmentType,
            TerritorySizeSqKm = territorySizeSqKm,
            BaselineStability = baselineStability,
            FoundingStatus = CountryFoundingStatus.NewlyFounded,
            Yield = yield?.Clone() ?? new NationalYield(),
            Treasury = treasury?.Clone() ?? new RepuTreasury(0.0, countryId)
        };
    }
}
