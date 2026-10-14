namespace Republic.Core.World;

using System;
using Republic.Core.Time;

/// <summary>
/// Domain value record representing the outcome of a domestic resource production cycle.
/// </summary>
public sealed record ResourceProductionResult
{
    /// <summary>
    /// Gets a value indicating whether physical goods were successfully extracted and added to inventory.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the unique identifier of the sovereign country receiving production.
    /// </summary>
    public string CountryId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the unique identifier of the resource node extracted from.
    /// </summary>
    public string NodeId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the commodity type produced (e.g., "Iron", "Timber", "Petroleum").
    /// </summary>
    public string ResourceType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the quantity of goods produced and deposited into inventory during this cycle.
    /// </summary>
    public double QuantityProduced { get; init; }

    /// <summary>
    /// Gets the authoritative Republic simulation clock six-hour cycle boundary for this production run.
    /// </summary>
    public RepublicTime Boundary { get; init; }

    /// <summary>
    /// Gets a descriptive message explaining the outcome or reason for non-production.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether production for this boundary had already been executed previously.
    /// </summary>
    public bool AlreadyProduced { get; init; }

    /// <summary>
    /// Creates a successful production outcome.
    /// </summary>
    public static ResourceProductionResult Succeeded(
        string countryId,
        string nodeId,
        string resourceType,
        double quantity,
        RepublicTime boundary,
        string message) =>
        new()
        {
            Success = true,
            CountryId = countryId,
            NodeId = nodeId,
            ResourceType = resourceType,
            QuantityProduced = quantity,
            Boundary = boundary,
            Message = message,
            AlreadyProduced = false
        };

    /// <summary>
    /// Creates an unsuccessful or skipped production outcome.
    /// </summary>
    public static ResourceProductionResult Failure(
        string countryId,
        string nodeId,
        string resourceType,
        RepublicTime boundary,
        string message,
        bool alreadyProduced = false) =>
        new()
        {
            Success = false,
            CountryId = countryId,
            NodeId = nodeId,
            ResourceType = resourceType,
            QuantityProduced = 0.0,
            Boundary = boundary,
            Message = message,
            AlreadyProduced = alreadyProduced
        };

    /// <inheritdoc />
    public override string ToString() =>
        Success
            ? $"Produced {QuantityProduced:N0} units of {ResourceType} at {Boundary} [{CountryId}]"
            : $"Production Failed at {Boundary}: {Message} [{CountryId}]";
}
