namespace Republic.Core.Networking;

using System;
using System.Text.Json.Serialization;
using Republic.Core.World;

/// <summary>
/// Lightweight network snapshot of diplomatic recognition status between two sovereign countries.
/// Either player can update recognition status and sync across the network.
/// </summary>
public sealed record DiplomaticSnapshot
{
    /// <summary>
    /// Gets the unique identifier of the source country.
    /// </summary>
    [JsonPropertyName("from_country_id")]
    public string FromCountryId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the unique identifier of the target country.
    /// </summary>
    [JsonPropertyName("to_country_id")]
    public string ToCountryId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current diplomatic status ("Unrecognized", "Recognized").
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = "Unrecognized";

    /// <summary>
    /// Gets a value indicating whether this relation has attained formal diplomatic recognition.
    /// </summary>
    [JsonPropertyName("is_recognized")]
    public bool IsRecognized => string.Equals(Status, "Recognized", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="DiplomaticSnapshot"/> class.
    /// </summary>
    public DiplomaticSnapshot() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiplomaticSnapshot"/> class with country IDs and status.
    /// </summary>
    public DiplomaticSnapshot(string fromCountryId, string toCountryId, string status = "Unrecognized")
    {
        FromCountryId = fromCountryId ?? string.Empty;
        ToCountryId = toCountryId ?? string.Empty;
        Status = status ?? "Unrecognized";
    }

    /// <summary>
    /// Creates a new <see cref="DiplomaticSnapshot"/> from an existing domain <see cref="DiplomaticRelation"/>.
    /// </summary>
    public static DiplomaticSnapshot FromRelation(DiplomaticRelation relation)
    {
        ArgumentNullException.ThrowIfNull(relation);
        return new DiplomaticSnapshot(relation.FromCountryId, relation.ToCountryId, relation.Status.ToString());
    }
}
