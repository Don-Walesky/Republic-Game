namespace Republic.Core.World.Models;

using System;
using System.Text.Json.Serialization;
using Republic.Core.Time;
using Republic.Core.World.Rules;

/// <summary>
/// Geographical macro-regions for sovereign states in Republic.
/// </summary>
public enum CountryRegion
{
    West,
    East,
    Central,
    North,
    South
}

/// <summary>
/// Sovereign constitutional government forms.
/// </summary>
public enum GovernmentForm
{
    Republic,
    Parliamentary,
    Federal,
    Military
}

/// <summary>
/// Immutable value object representing a sovereign nation's descriptive profile and identity.
/// Contains official nomenclature, geography, demographics, constitutional classification,
/// dominant language, natural resource base, and political culture.
/// </summary>
public sealed record CountryProfile
{
    /// <summary>
    /// Default fallback string for unspecified text properties.
    /// </summary>
    public const string DefaultUnspecified = "Unspecified";

    private readonly string _officialName = DefaultUnspecified;
    private readonly string _capital = DefaultUnspecified;
    private readonly string _dominantLanguage = DefaultUnspecified;
    private readonly string _constitutionName = DefaultUnspecified;
    private readonly string _primaryResource = DefaultUnspecified;
    private readonly string _politicalCulture = DefaultUnspecified;
    private readonly long _population;

    /// <summary>
    /// Gets the official ceremonial and legal title of the sovereign state.
    /// Must pass the CountryNameRule (must contain the whole word 'Republic') unless unspecified.
    /// </summary>
    public string OfficialName
    {
        get => _officialName;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _officialName = DefaultUnspecified;
            }
            else
            {
                var trimmed = value.Trim();
                if (trimmed.Equals(DefaultUnspecified, StringComparison.OrdinalIgnoreCase))
                {
                    _officialName = DefaultUnspecified;
                }
                else
                {
                    _officialName = CountryNameRule.Default.Validate(trimmed);
                }
            }
        }
    }

    /// <summary>
    /// Gets the seat of government and capital city.
    /// </summary>
    public string Capital
    {
        get => _capital;
        init => _capital = string.IsNullOrWhiteSpace(value) ? DefaultUnspecified : value.Trim();
    }

    /// <summary>
    /// Gets the sovereign macro-region.
    /// </summary>
    public CountryRegion Region { get; init; } = CountryRegion.Central;

    /// <summary>
    /// Gets the national population count. Cannot be negative.
    /// </summary>
    public long Population
    {
        get => _population;
        init
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Population cannot be negative.");
            }

            _population = value;
        }
    }

    /// <summary>
    /// Gets the dominant national language.
    /// </summary>
    public string DominantLanguage
    {
        get => _dominantLanguage;
        init => _dominantLanguage = string.IsNullOrWhiteSpace(value) ? DefaultUnspecified : value.Trim();
    }

    /// <summary>
    /// Gets the structural constitutional government form.
    /// </summary>
    public GovernmentForm GovernmentForm { get; init; } = GovernmentForm.Republic;

    /// <summary>
    /// Gets the formal title of the national constitution or basic law.
    /// </summary>
    public string ConstitutionName
    {
        get => _constitutionName;
        init => _constitutionName = string.IsNullOrWhiteSpace(value) ? DefaultUnspecified : value.Trim();
    }

    /// <summary>
    /// Gets the nation's primary economic resource base.
    /// </summary>
    public string PrimaryResource
    {
        get => _primaryResource;
        init => _primaryResource = string.IsNullOrWhiteSpace(value) ? DefaultUnspecified : value.Trim();
    }

    /// <summary>
    /// Gets a single-sentence overview of national political culture and civic philosophy.
    /// </summary>
    public string PoliticalCulture
    {
        get => _politicalCulture;
        init => _politicalCulture = string.IsNullOrWhiteSpace(value) ? DefaultUnspecified : value.Trim();
    }

    /// <summary>
    /// Gets the authoritative Republic simulation timestamp at which national statehood was founded.
    /// Reuses the existing founding timestamp of the country; does not create a second founding date.
    /// </summary>
    public RepublicTime FoundingTime { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CountryProfile"/> record with default unspecified values.
    /// Parameterless constructor required for JSON serialization.
    /// </summary>
    public CountryProfile()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CountryProfile"/> record with validated fields.
    /// </summary>
    [JsonConstructor]
    public CountryProfile(
        string? officialName = DefaultUnspecified,
        string? capital = DefaultUnspecified,
        CountryRegion region = CountryRegion.Central,
        long population = 0,
        string? dominantLanguage = DefaultUnspecified,
        GovernmentForm governmentForm = GovernmentForm.Republic,
        string? constitutionName = DefaultUnspecified,
        string? primaryResource = DefaultUnspecified,
        string? politicalCulture = DefaultUnspecified,
        RepublicTime foundingTime = default)
    {
        if (population < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(population), "Population cannot be negative.");
        }

        if (!string.IsNullOrWhiteSpace(officialName) && !officialName.Trim().Equals(DefaultUnspecified, StringComparison.OrdinalIgnoreCase))
        {
            OfficialName = CountryNameRule.Default.Validate(officialName);
        }
        else
        {
            OfficialName = DefaultUnspecified;
        }

        Capital = capital ?? DefaultUnspecified;
        Region = region;
        Population = population;
        DominantLanguage = dominantLanguage ?? DefaultUnspecified;
        GovernmentForm = governmentForm;
        ConstitutionName = constitutionName ?? DefaultUnspecified;
        PrimaryResource = primaryResource ?? DefaultUnspecified;
        PoliticalCulture = politicalCulture ?? DefaultUnspecified;
        FoundingTime = foundingTime;
    }

    /// <summary>
    /// Creates a fixed, stable profile representing the Republic of Arcadia with canonical defaults.
    /// </summary>
    public static CountryProfile CreateArcadia(
        RepublicTime foundingTime = default,
        string officialName = "Republic of Arcadia",
        string capital = "Grand Arcadia") => new(
            officialName: officialName,
            capital: capital,
            region: CountryRegion.Central,
            population: 10_000_000,
            dominantLanguage: "English",
            governmentForm: GovernmentForm.Federal,
            constitutionName: "Constitution of the Republic of Arcadia",
            primaryResource: "Iron",
            politicalCulture: "A constitutional federal democracy anchored by civic republicanism and regional self-governance.",
            foundingTime: foundingTime);
}
