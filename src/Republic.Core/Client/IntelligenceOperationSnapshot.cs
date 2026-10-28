namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable data contract representing a planned, active, or completed covert intelligence operation.
/// Exposes operational parameters, status, prerequisites, risks, and outcome to the Godot client.
/// </summary>
public sealed record IntelligenceOperationSnapshot
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("initiator_country_id")]
    public string InitiatorCountryId { get; init; } = string.Empty;

    [JsonPropertyName("target_country_id")]
    public string TargetCountryId { get; init; } = string.Empty;

    [JsonPropertyName("target_country_name")]
    public string TargetCountryName { get; init; } = string.Empty;

    [JsonPropertyName("operation_type")]
    public string OperationType { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("financial_cost")]
    public double FinancialCost { get; init; }

    [JsonPropertyName("financial_cost_formatted")]
    public string FinancialCostFormatted { get; init; } = string.Empty;

    [JsonPropertyName("required_military_level")]
    public int RequiredMilitaryLevel { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("risk_rating")]
    public double RiskRating { get; init; }

    [JsonPropertyName("success_probability")]
    public double SuccessProbability { get; init; }

    [JsonPropertyName("is_detected")]
    public bool IsDetected { get; init; }

    [JsonPropertyName("outcome_message")]
    public string OutcomeMessage { get; init; } = string.Empty;
}
