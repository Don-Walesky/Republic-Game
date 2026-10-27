namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable data contract representing a private sector investor or corporate consortium.
/// </summary>
public sealed record PrivateInvestorSnapshot
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("sector")]
    public string Sector { get; init; } = "Manufacturing";

    [JsonPropertyName("investor_type")]
    public string InvestorType { get; init; } = "DomesticConglomerate";

    [JsonPropertyName("capital_capacity")]
    public double CapitalCapacity { get; init; }

    [JsonPropertyName("risk_tolerance")]
    public string RiskTolerance { get; init; } = "Moderate";

    [JsonPropertyName("government_relationship")]
    public double GovernmentRelationship { get; init; }
}
