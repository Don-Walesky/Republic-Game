namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable data contract representing a historical or recent strategic military directive outcome.
/// </summary>
public sealed record MilitaryActionSnapshot
{
    [JsonPropertyName("attacker_country")]
    public string AttackerCountry { get; init; } = string.Empty;

    [JsonPropertyName("target_country")]
    public string TargetCountry { get; init; } = string.Empty;

    [JsonPropertyName("operation_type")]
    public string OperationType { get; init; } = string.Empty;

    [JsonPropertyName("troops_committed")]
    public int TroopsCommitted { get; init; }

    [JsonPropertyName("succeeded")]
    public bool Succeeded { get; init; }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; init; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}

