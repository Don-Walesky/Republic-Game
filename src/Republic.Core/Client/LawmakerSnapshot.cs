namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable data contract representing a parliamentarian / lawmaker in the National Assembly.
/// </summary>
public sealed record LawmakerSnapshot
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("constituency")]
    public string Constituency { get; init; } = string.Empty;

    [JsonPropertyName("original_party")]
    public string OriginalParty { get; init; } = "Opposition";

    [JsonPropertyName("current_party")]
    public string CurrentParty { get; init; } = "Opposition";

    [JsonPropertyName("seat_weight")]
    public int SeatWeight { get; init; } = 1;

    [JsonPropertyName("stance")]
    public string Stance { get; init; } = string.Empty;

    [JsonPropertyName("loyalty")]
    public double Loyalty { get; init; } = 60.0;

    [JsonPropertyName("has_defected")]
    public bool HasDefected { get; init; }

    [JsonPropertyName("defection_incentive")]
    public string DefectionIncentive { get; init; } = string.Empty;
}

