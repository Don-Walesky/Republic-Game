namespace Republic.Core.World;

using System;
using System.Text.Json.Serialization;
using Republic.Core.Time;

/// <summary>
/// Status of an open bilateral diplomatic relation between two countries.
/// </summary>
public enum DiplomaticRelationStatus
{
    Unrecognized,
    Recognized
}

/// <summary>
/// Domain model representing a diplomatic relation opened from a source country to a target country.
/// Anchored to the authoritative Republic simulation clock boundary when opened.
/// </summary>
public sealed record DiplomaticRelation
{
    /// <summary>
    /// Gets the unique identifier of the source country opening the relation.
    /// </summary>
    public string FromCountryId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the target country.
    /// </summary>
    public string ToCountryId { get; init; }

    /// <summary>
    /// Gets the current diplomatic status (Unrecognized, Recognized).
    /// </summary>
    public DiplomaticRelationStatus Status { get; init; }

    /// <summary>
    /// Gets the authoritative Republic simulation clock boundary when the relation was opened.
    /// </summary>
    public RepublicTime OpenedBoundary { get; init; }

    /// <summary>
    /// Gets the authoritative Republic simulation clock boundary when the relation was formally recognized, or null if unrecognized.
    /// </summary>
    public RepublicTime? RecognizedBoundary { get; init; }

    /// <summary>
    /// Alias for <see cref="OpenedBoundary"/>.
    /// </summary>
    [JsonIgnore]
    public RepublicTime Boundary => OpenedBoundary;

    /// <summary>
    /// Alias for <see cref="FromCountryId"/>.
    /// </summary>
    [JsonIgnore]
    public string SourceCountryId => FromCountryId;

    /// <summary>
    /// Alias for <see cref="ToCountryId"/>.
    /// </summary>
    [JsonIgnore]
    public string TargetCountryId => ToCountryId;

    /// <summary>
    /// Indicates whether this diplomatic relation has attained formal diplomatic recognition.
    /// </summary>
    [JsonIgnore]
    public bool IsRecognized => Status == DiplomaticRelationStatus.Recognized;

    /// <summary>
    /// Indicates whether this diplomatic relation is in unrecognized status.
    /// </summary>
    [JsonIgnore]
    public bool IsUnrecognized => Status == DiplomaticRelationStatus.Unrecognized;

    /// <summary>
    /// Initializes a new diplomatic relation record with full status and recognition specification.
    /// </summary>
    [JsonConstructor]
    public DiplomaticRelation(
        string fromCountryId,
        string toCountryId,
        DiplomaticRelationStatus status,
        RepublicTime openedBoundary,
        RepublicTime? recognizedBoundary = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fromCountryId);
        ArgumentException.ThrowIfNullOrWhiteSpace(toCountryId);

        FromCountryId = fromCountryId.Trim();
        ToCountryId = toCountryId.Trim();
        Status = status;
        OpenedBoundary = openedBoundary;
        RecognizedBoundary = recognizedBoundary;
    }

    /// <summary>
    /// Initializes a new diplomatic relation record defaulting to <see cref="DiplomaticRelationStatus.Unrecognized"/>.
    /// </summary>
    public DiplomaticRelation(
        string fromCountryId,
        string toCountryId,
        RepublicTime openedBoundary)
        : this(fromCountryId, toCountryId, DiplomaticRelationStatus.Unrecognized, openedBoundary, null)
    {
    }

    /// <summary>
    /// Deconstructs the diplomatic relation into its primary constituents.
    /// </summary>
    public void Deconstruct(
        out string fromCountryId,
        out string toCountryId,
        out DiplomaticRelationStatus status,
        out RepublicTime openedBoundary)
    {
        fromCountryId = FromCountryId;
        toCountryId = ToCountryId;
        status = Status;
        openedBoundary = OpenedBoundary;
    }

    /// <summary>
    /// Deconstructs the diplomatic relation including its recognition boundary.
    /// </summary>
    public void Deconstruct(
        out string fromCountryId,
        out string toCountryId,
        out DiplomaticRelationStatus status,
        out RepublicTime openedBoundary,
        out RepublicTime? recognizedBoundary)
    {
        fromCountryId = FromCountryId;
        toCountryId = ToCountryId;
        status = Status;
        openedBoundary = OpenedBoundary;
        recognizedBoundary = RecognizedBoundary;
    }

    /// <summary>
    /// Implicit boolean operator evaluating whether a valid relation reference exists.
    /// </summary>
    public static implicit operator bool(DiplomaticRelation? relation) => relation is not null;

    /// <summary>
    /// Returns a new diplomatic relation record with the specified recognition status.
    /// Either party can update recognition status in bilateral diplomacy.
    /// </summary>
    public DiplomaticRelation WithStatus(DiplomaticRelationStatus status, RepublicTime? boundary = null)
    {
        return this with
        {
            Status = status,
            RecognizedBoundary = status == DiplomaticRelationStatus.Recognized
                ? (boundary ?? RecognizedBoundary ?? OpenedBoundary)
                : null
        };
    }

    /// <summary>
    /// Recognizes this bilateral diplomatic relation.
    /// </summary>
    public DiplomaticRelation Recognize(RepublicTime? boundary = null) =>
        WithStatus(DiplomaticRelationStatus.Recognized, boundary);

    /// <summary>
    /// Formats the diplomatic relation for dashboard and diagnostic inspection.
    /// </summary>
    public override string ToString()
    {
        var recInfo = (Status == DiplomaticRelationStatus.Recognized && RecognizedBoundary.HasValue)
            ? $" (recognized at {RecognizedBoundary.Value})"
            : "";
        return $"{FromCountryId} -> {ToCountryId} ({Status}{recInfo}) opened at {OpenedBoundary}";
    }
}
