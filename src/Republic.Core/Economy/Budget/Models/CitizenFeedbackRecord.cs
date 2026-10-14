namespace Republic.Core.Economy.Budget.Models;

using System;
using Republic.Core.Time;

/// <summary>
/// Domain value record representing the public approval and citizen welfare consequence
/// resulting from a domestic taxation or governance policy adjustment.
/// </summary>
public sealed record CitizenFeedbackRecord
{
    /// <summary>
    /// Gets the personal income tax rate prior to this policy decision.
    /// </summary>
    public double PreviousIncomeTaxRate { get; init; }

    /// <summary>
    /// Gets the personal income tax rate enacted by this policy decision.
    /// </summary>
    public double NewIncomeTaxRate { get; init; }

    /// <summary>
    /// Gets the corporate tax rate prior to this policy decision.
    /// </summary>
    public double PreviousCorporateTaxRate { get; init; }

    /// <summary>
    /// Gets the corporate tax rate enacted by this policy decision.
    /// </summary>
    public double NewCorporateTaxRate { get; init; }

    /// <summary>
    /// Gets the change in personal income tax rate (New - Previous).
    /// </summary>
    public double IncomeTaxDelta => NewIncomeTaxRate - PreviousIncomeTaxRate;

    /// <summary>
    /// Gets the public approval points gained or lost as a direct result of this policy decision.
    /// </summary>
    public double ApprovalDelta { get; init; }

    /// <summary>
    /// Gets the citizen approval rating immediately prior to this policy decision.
    /// </summary>
    public double PreviousApproval { get; init; }

    /// <summary>
    /// Gets the citizen approval rating immediately after applying this policy decision.
    /// </summary>
    public double CurrentApproval { get; init; }

    /// <summary>
    /// Gets the national baseline stability points shifted as a result of extreme policy settings.
    /// </summary>
    public double StabilityDelta { get; init; }

    /// <summary>
    /// Gets the baseline stability immediately prior to this policy decision.
    /// </summary>
    public double PreviousStability { get; init; }

    /// <summary>
    /// Gets the baseline stability immediately after applying this policy decision.
    /// </summary>
    public double CurrentStability { get; init; }

    /// <summary>
    /// Gets the authoritative Republic simulation timestamp at which this policy decision was enacted.
    /// </summary>
    public RepublicTime EnactedTime { get; init; }

    /// <summary>
    /// Gets the human-readable explanation of the citizen reaction and welfare consequences.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    public override string ToString() => Description;
}
