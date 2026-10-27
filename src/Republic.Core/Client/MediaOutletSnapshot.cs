namespace Republic.Core.Client;

using System.Text.Json.Serialization;

/// <summary>
/// Serializable data contract representing an active media outlet in the republic.
/// </summary>
public sealed record MediaOutletSnapshot
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("leaning")]
    public string Leaning { get; init; } = "StateAligned";

    [JsonPropertyName("audience_reach")]
    public double AudienceReach { get; init; }

    [JsonPropertyName("credibility")]
    public double Credibility { get; init; }

    [JsonPropertyName("government_attitude")]
    public double GovernmentAttitude { get; init; }

    [JsonPropertyName("is_censored")]
    public bool IsCensored { get; init; }

    [JsonPropertyName("consecutive_strikes")]
    public int ConsecutiveStrikes { get; init; }
}
