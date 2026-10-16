namespace Republic.Core.Networking;

using System.Text.Json.Serialization;

/// <summary>
/// Lightweight snapshot of a sovereign country's basic governance state for multiplayer synchronization.
/// </summary>
public sealed record CountrySnapshot
{
    /// <summary>
    /// Gets the unique identifier of the country or network peer.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the official name of the sovereign nation.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the formatted sovereign liquid treasury balance in REPU (e.g. "R 15,000,000,000").
    /// </summary>
    [JsonPropertyName("treasury")]
    public string Treasury { get; init; } = string.Empty;

    /// <summary>
    /// Gets the formatted citizen approval rating percentage (e.g. "70.0%").
    /// </summary>
    [JsonPropertyName("approval")]
    public string Approval { get; init; } = string.Empty;

    /// <summary>
    /// Gets the formatted personal income tax rate percentage (e.g. "25.0%").
    /// </summary>
    [JsonPropertyName("tax_rate")]
    public string TaxRate { get; init; } = string.Empty;

    /// <summary>
    /// Gets the note or summary of the last AI governance decision.
    /// </summary>
    [JsonPropertyName("last_ai_decision")]
    public string LastAiDecision { get; init; } = string.Empty;
}

