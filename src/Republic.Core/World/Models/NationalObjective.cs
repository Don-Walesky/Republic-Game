namespace Republic.Core.World.Models;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Domain model representing an authoritative sovereign national objective
/// generated from the administration's national development plans and strategic agenda.
/// </summary>
public sealed class NationalObjective
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = "NationalDevelopment";

    [JsonPropertyName("target_value")]
    public double TargetValue { get; set; } = 100.0;

    [JsonPropertyName("current_value")]
    public double CurrentValue { get; set; }

    [JsonPropertyName("progress_percentage")]
    public double ProgressPercentage { get; set; }

    [JsonPropertyName("is_completed")]
    public bool IsCompleted { get; set; }

    [JsonPropertyName("reward_summary")]
    public string RewardSummary { get; set; } = string.Empty;
}

/// <summary>
/// Domain model representing a strategic presidential opportunity
/// emerging from the player's chosen leadership identity and executive doctrine.
/// </summary>
public sealed class StrategicOpportunity
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("advantage_summary")]
    public string AdvantageSummary { get; set; } = string.Empty;

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;
}
