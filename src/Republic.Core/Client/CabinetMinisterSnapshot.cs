namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable data contract representing an individual cabinet ministerial portfolio
/// and its appointed minister or vacancy status within the presidential desk state.
/// </summary>
public sealed record CabinetMinisterSnapshot
{
    /// <summary>
    /// Gets the programmatic name of the cabinet ministerial portfolio (e.g. "Finance", "Interior").
    /// </summary>
    [JsonPropertyName("portfolio")]
    public string Portfolio { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether this portfolio currently has an active appointed minister.
    /// </summary>
    [JsonPropertyName("is_appointed")]
    public bool IsAppointed { get; init; }

    /// <summary>
    /// Gets the human-readable appointment status ("Appointed" or "Vacant").
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = "Vacant";

    /// <summary>
    /// Gets the unique identifier of the appointed minister, or empty string if vacant.
    /// </summary>
    [JsonPropertyName("minister_id")]
    public string MinisterId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the full name of the appointed minister, or "Vacant" if none is assigned.
    /// </summary>
    [JsonPropertyName("minister_name")]
    public string MinisterName { get; init; } = "Vacant";

    /// <summary>
    /// Gets the minister's administrative and domain competence score (0.0 to 1.0), or 0.0 if vacant.
    /// </summary>
    [JsonPropertyName("competence")]
    public double Competence { get; init; }

    /// <summary>
    /// Gets the minister's political loyalty rating to the government (0.0 to 1.0), or 0.0 if vacant.
    /// </summary>
    [JsonPropertyName("loyalty")]
    public double Loyalty { get; init; }

    /// <summary>
    /// Gets the minister's personal integrity rating (0.0 to 1.0), or 0.0 if vacant.
    /// </summary>
    [JsonPropertyName("integrity")]
    public double Integrity { get; init; }

    /// <summary>
    /// Gets the minister's professional experience rating (0.0 to 1.0), or 0.0 if vacant.
    /// </summary>
    [JsonPropertyName("experience")]
    public double Experience { get; init; }

    /// <summary>
    /// Gets the minister's parliamentary and factional connections rating (0.0 to 1.0), or 0.0 if vacant.
    /// </summary>
    [JsonPropertyName("political_connections")]
    public double PoliticalConnections { get; init; }

    /// <summary>
    /// Gets the minister's monthly salary in REPU, or 0.0 if vacant.
    /// </summary>
    [JsonPropertyName("monthly_salary")]
    public double MonthlySalary { get; init; }

    /// <summary>
    /// Gets the political faction alignment identifier of the minister, or empty string if unaligned/vacant.
    /// </summary>
    [JsonPropertyName("faction_alignment_id")]
    public string FactionAlignmentId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name of the state capacity metric directly influenced by this portfolio, or null if not applicable.
    /// </summary>
    [JsonPropertyName("state_capacity_metric")]
    public string? StateCapacityMetric { get; init; }

    /// <summary>
    /// Gets the current state capacity value (0.0 to 1.0) governed by this portfolio, or null if not applicable.
    /// </summary>
    [JsonPropertyName("state_capacity_value")]
    public double? StateCapacityValue { get; init; }
}
