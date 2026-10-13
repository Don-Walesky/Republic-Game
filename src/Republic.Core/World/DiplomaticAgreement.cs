namespace Republic.Core.World;

using System;
using System.Text.Json.Serialization;
using Republic.Core.Time;

/// <summary>
/// Classification of bilateral diplomatic agreements between sovereign nations.
/// </summary>
public enum DiplomaticAgreementType
{
    Trade,
    Alliance
}

/// <summary>
/// Lifecycle status of a bilateral diplomatic agreement.
/// </summary>
public enum DiplomaticAgreementStatus
{
    Pending,
    Accepted,
    Rejected,
    Terminated
}

/// <summary>
/// Domain model representing a bilateral diplomatic agreement (such as a trade agreement or alliance)
/// between two sovereign nations. Anchored strictly to Republic simulation clock boundaries.
/// </summary>
public sealed record DiplomaticAgreement
{
    /// <summary>
    /// Gets the unique identifier of the bilateral agreement.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Gets the unique identifier of the sovereign country initiating the proposal.
    /// </summary>
    public string SourceCountryId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the sovereign country receiving the proposal.
    /// </summary>
    public string TargetCountryId { get; init; }

    /// <summary>
    /// Gets the type of agreement (Trade, Alliance).
    /// </summary>
    public DiplomaticAgreementType AgreementType { get; init; }

    /// <summary>
    /// Gets the current lifecycle status (Pending, Accepted, Rejected, Terminated).
    /// </summary>
    public DiplomaticAgreementStatus Status { get; init; }

    /// <summary>
    /// Gets the authoritative Republic simulation clock boundary when the agreement was proposed.
    /// </summary>
    public RepublicTime ProposedBoundary { get; init; }

    /// <summary>
    /// Gets the authoritative Republic simulation clock boundary when the agreement was decided (accepted or rejected),
    /// or null if still pending.
    /// </summary>
    public RepublicTime? DecidedBoundary { get; init; }

    /// <summary>
    /// Indicates whether this agreement is currently pending a diplomatic decision.
    /// </summary>
    [JsonIgnore]
    public bool IsPending => Status == DiplomaticAgreementStatus.Pending;

    /// <summary>
    /// Indicates whether this agreement has been accepted and is active.
    /// </summary>
    [JsonIgnore]
    public bool IsAccepted => Status == DiplomaticAgreementStatus.Accepted;

    /// <summary>
    /// Indicates whether this agreement was rejected.
    /// </summary>
    [JsonIgnore]
    public bool IsRejected => Status == DiplomaticAgreementStatus.Rejected;

    /// <summary>
    /// Indicates whether this agreement is currently active (i.e. accepted).
    /// </summary>
    [JsonIgnore]
    public bool IsActive => Status == DiplomaticAgreementStatus.Accepted;

    /// <summary>
    /// Indicates whether this agreement is a bilateral trade agreement.
    /// </summary>
    [JsonIgnore]
    public bool IsTrade => AgreementType == DiplomaticAgreementType.Trade;

    /// <summary>
    /// Indicates whether this agreement is a bilateral alliance.
    /// </summary>
    [JsonIgnore]
    public bool IsAlliance => AgreementType == DiplomaticAgreementType.Alliance;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiplomaticAgreement"/> record.
    /// </summary>
    public DiplomaticAgreement(
        string sourceCountryId,
        string targetCountryId,
        DiplomaticAgreementType agreementType,
        DiplomaticAgreementStatus status,
        RepublicTime proposedBoundary,
        RepublicTime? decidedBoundary = null,
        string? id = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCountryId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetCountryId);

        Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id.Trim();
        SourceCountryId = sourceCountryId.Trim();
        TargetCountryId = targetCountryId.Trim();
        AgreementType = agreementType;
        Status = status;
        ProposedBoundary = proposedBoundary;
        DecidedBoundary = decidedBoundary;
    }

    /// <summary>
    /// Checks whether the specified country identifier is one of the parties to this bilateral agreement.
    /// </summary>
    public bool Involves(string countryId)
    {
        if (string.IsNullOrWhiteSpace(countryId))
        {
            return false;
        }

        return string.Equals(SourceCountryId, countryId.Trim(), StringComparison.OrdinalIgnoreCase) ||
               string.Equals(TargetCountryId, countryId.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks whether this bilateral agreement connects the specified two countries (in either direction).
    /// </summary>
    public bool Involves(string countryAId, string countryBId)
    {
        if (string.IsNullOrWhiteSpace(countryAId) || string.IsNullOrWhiteSpace(countryBId))
        {
            return false;
        }

        var a = countryAId.Trim();
        var b = countryBId.Trim();

        return (string.Equals(SourceCountryId, a, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(TargetCountryId, b, StringComparison.OrdinalIgnoreCase)) ||
               (string.Equals(SourceCountryId, b, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(TargetCountryId, a, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Deconstructs the diplomatic agreement into its primary components.
    /// </summary>
    public void Deconstruct(
        out string sourceCountryId,
        out string targetCountryId,
        out DiplomaticAgreementType agreementType,
        out DiplomaticAgreementStatus status,
        out RepublicTime proposedBoundary,
        out RepublicTime? decidedBoundary)
    {
        sourceCountryId = SourceCountryId;
        targetCountryId = TargetCountryId;
        agreementType = AgreementType;
        status = Status;
        proposedBoundary = ProposedBoundary;
        decidedBoundary = DecidedBoundary;
    }

    /// <summary>
    /// Formats the bilateral agreement for inspection and logging.
    /// </summary>
    public override string ToString()
    {
        var decidedText = DecidedBoundary.HasValue ? $", decided at {DecidedBoundary.Value}" : "";
        return $"{AgreementType} Agreement: {SourceCountryId} <-> {TargetCountryId} [{Status}] (proposed at {ProposedBoundary}{decidedText})";
    }
}
