global using BillStatus = Republic.Core.Politics.CountryBillStatus;

namespace Republic.Core.Politics;

using System;
using System.Text.Json.Serialization;
using Republic.Core.Time;

/// <summary>
/// Specifies the legislative status of a country bill.
/// </summary>
public enum CountryBillStatus
{
    Pending,
    Passed,
    Failed
}

/// <summary>
/// Domain model representing a legislative bill proposed in a sovereign country.
/// Holds the bill title, owning country identifier, status, and the authoritative clock boundary when voted.
/// </summary>
public sealed record CountryBill
{
    private readonly string _title = string.Empty;

    /// <summary>
    /// Gets the official title of the legislative bill (trimmed, not empty).
    /// </summary>
    public string Title
    {
        get => _title;
        init => _title = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Gets the identifier of the sovereign country where the bill was proposed.
    /// </summary>
    public string CountryId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current legislative status of the bill (Pending, Passed, or Failed).
    /// </summary>
    public CountryBillStatus Status { get; init; } = CountryBillStatus.Pending;

    /// <summary>
    /// Gets the Republic simulation clock boundary at which the bill was voted on, or null if pending.
    /// </summary>
    public RepublicTime? VotedBoundary { get; init; }

    /// <summary>
    /// Gets a value indicating whether the bill is currently pending a legislative vote.
    /// </summary>
    [JsonIgnore]
    public bool IsPending => Status == CountryBillStatus.Pending;

    /// <summary>
    /// Gets a value indicating whether the bill has successfully passed.
    /// </summary>
    [JsonIgnore]
    public bool IsPassed => Status == CountryBillStatus.Passed;

    /// <summary>
    /// Gets a value indicating whether the bill has failed.
    /// </summary>
    [JsonIgnore]
    public bool IsFailed => Status == CountryBillStatus.Failed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CountryBill"/> record with default values.
    /// </summary>
    public CountryBill() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CountryBill"/> record with validated parameters.
    /// </summary>
    /// <param name="title">The official bill title (trimmed, not empty).</param>
    /// <param name="countryId">The owning country identifier.</param>
    /// <param name="status">The initial or updated status of the bill.</param>
    /// <param name="votedBoundary">The clock boundary when the bill was voted on, or null.</param>
    public CountryBill(
        string title,
        string countryId,
        CountryBillStatus status = CountryBillStatus.Pending,
        RepublicTime? votedBoundary = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Title = title.Trim();
        CountryId = countryId ?? string.Empty;
        Status = status;
        VotedBoundary = votedBoundary;
    }

    /// <summary>
    /// Deconstructs the bill into its constituent components.
    /// </summary>
    public void Deconstruct(out string title, out string countryId, out CountryBillStatus status, out RepublicTime? votedBoundary)
    {
        title = Title;
        countryId = CountryId;
        status = Status;
        votedBoundary = VotedBoundary;
    }

    /// <summary>
    /// Deconstructs the bill into its 3-part representation without country id.
    /// </summary>
    public void Deconstruct(out string title, out CountryBillStatus status, out RepublicTime? votedBoundary)
    {
        title = Title;
        status = Status;
        votedBoundary = VotedBoundary;
    }

    /// <summary>
    /// Returns a formatted representation of the bill title and status.
    /// </summary>
    public override string ToString() => VotedBoundary.HasValue
        ? $"{Title} ({Status} at {VotedBoundary.Value})"
        : $"{Title} ({Status})";

    /// <summary>
    /// Implicitly converts a <see cref="CountryBill"/> to a boolean indicating whether the bill was passed.
    /// </summary>
    public static implicit operator bool(CountryBill? bill) => bill != null && bill.Status == CountryBillStatus.Passed;
}
