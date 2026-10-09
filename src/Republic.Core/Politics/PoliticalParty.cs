namespace Republic.Core.Politics;

using System;
using System.Text.Json.Serialization;
using Republic.Core.Time;

/// <summary>
/// Domain model representing a sovereign political party founded in a country.
/// Holds the party name, country ownership, founding clock boundary, and public support rating.
/// </summary>
public sealed record PoliticalParty
{
    private readonly string _name = string.Empty;
    private readonly double _support = 10.0;

    /// <summary>
    /// Initial baseline public support rating for a newly founded political party.
    /// </summary>
    public const double InitialSupport = 10.0;

    /// <summary>
    /// Gets the official name of the political party (trimmed, not empty).
    /// </summary>
    public string Name
    {
        get => _name;
        init => _name = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Gets the identifier of the sovereign country where the political party was founded.
    /// </summary>
    public string CountryId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the Republic simulation clock boundary at which the political party was founded.
    /// </summary>
    public RepublicTime FoundedBoundary { get; init; }

    /// <summary>
    /// Alias for <see cref="FoundedBoundary"/>.
    /// </summary>
    [JsonIgnore]
    public RepublicTime Boundary => FoundedBoundary;

    /// <summary>
    /// Gets the public political support rating (0.0 to 100.0), starting at 10.
    /// </summary>
    public double Support
    {
        get => _support;
        init => _support = Math.Clamp(value, 0.0, 100.0);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PoliticalParty"/> record with default values.
    /// </summary>
    public PoliticalParty() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PoliticalParty"/> record with validated parameters.
    /// </summary>
    /// <param name="name">The official party name (trimmed, not empty).</param>
    /// <param name="countryId">The owning country identifier.</param>
    /// <param name="foundedBoundary">The Republic clock boundary when founded.</param>
    /// <param name="support">Public support rating between 0 and 100 (defaults to 10.0).</param>
    public PoliticalParty(string name, string countryId, RepublicTime foundedBoundary, double support = InitialSupport)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (support < 0.0 || support > 100.0)
        {
            throw new ArgumentOutOfRangeException(nameof(support), "Support must be between 0.0 and 100.0.");
        }

        Name = name.Trim();
        CountryId = countryId ?? string.Empty;
        FoundedBoundary = foundedBoundary;
        Support = support;
    }

    /// <summary>
    /// Deconstructs the political party into its constituent components.
    /// </summary>
    public void Deconstruct(out string name, out string countryId, out RepublicTime foundedBoundary, out double support)
    {
        name = Name;
        countryId = CountryId;
        foundedBoundary = FoundedBoundary;
        support = Support;
    }

    /// <summary>
    /// Returns a formatted representation of the political party name and support rating.
    /// </summary>
    public override string ToString() => $"{Name} (Support: {Support:0.##}%)";
}
