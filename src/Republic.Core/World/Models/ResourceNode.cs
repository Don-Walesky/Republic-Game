namespace Republic.Core.World.Models;

using System;

/// <summary>
/// Domain model representing a natural or strategic resource node associated with a sovereign country or region.
/// Supplies raw physical commodities into a country's physical goods stockpile upon six-hour cycle production.
/// </summary>
public sealed class ResourceNode
{
    /// <summary>
    /// Explicit, documented base production rate per six-hour cycle boundary (50.0 units).
    /// </summary>
    public const double DefaultProductionRate = 50.0;

    private double _abundance = 10000.0;
    private double _extractionRate = DefaultProductionRate;

    /// <summary>
    /// Gets the unique identifier of the resource node.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets the display name of this resource node (e.g., "Arcadia Iron Basin").
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the commodity or resource type produced (e.g., "Iron", "Petroleum", "Timber").
    /// </summary>
    public string ResourceType { get; init; } = "Iron";

    /// <summary>
    /// Gets or sets the sovereign country ID owning or explicitly associated with this node.
    /// Empty string indicates an unassigned or neutral node.
    /// </summary>
    public string CountryId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this resource node is currently active and extracting goods.
    /// Defaults to true. Inactive nodes produce 0 goods.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the remaining physical resource reserve abundance.
    /// Depleted when non-renewable extraction occurs.
    /// </summary>
    public double Abundance
    {
        get => _abundance;
        set
        {
            if (!double.IsFinite(value) || value < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Resource abundance must be a finite non-negative number.");
            }
            _abundance = value;
        }
    }

    /// <summary>
    /// Gets or sets the physical units extracted and produced per six-hour cycle boundary.
    /// Defaults to <see cref="DefaultProductionRate"/> (50.0).
    /// </summary>
    public double ExtractionRate
    {
        get => _extractionRate;
        set
        {
            if (!double.IsFinite(value) || value <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Extraction rate must be a finite number strictly greater than zero.");
            }
            _extractionRate = value;
        }
    }

    /// <summary>
    /// Gets a value indicating whether this node has renewable reserves that do not deplete over time.
    /// </summary>
    public bool IsRenewable { get; init; }

    /// <summary>
    /// Gets the regional identifier where this node is geographically located.
    /// </summary>
    public string LocationRegionId { get; init; } = string.Empty;

    /// <summary>
    /// Determines whether this node is associated with the specified country.
    /// </summary>
    public bool IsAssociatedWith(string countryId)
    {
        if (string.IsNullOrWhiteSpace(countryId))
        {
            return false;
        }
        return string.Equals(CountryId?.Trim(), countryId.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"{ResourceType} Node '{(string.IsNullOrWhiteSpace(Name) ? Id : Name)}' [{(IsActive ? "Active" : "Inactive")}] (Abundance: {Abundance:N0}, Rate: {ExtractionRate:N0}/cycle, Country: {(string.IsNullOrWhiteSpace(CountryId) ? "None" : CountryId)})";
}
