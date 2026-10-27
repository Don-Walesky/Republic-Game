namespace Republic.Core.Media.Models;

/// <summary>
/// Domain model encapsulating the authoritative outcome of a presidential media-management action.
/// </summary>
public sealed class MediaResponseResult
{
    /// <summary>
    /// Gets a value indicating whether the media response action succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the executive action taken (e.g. statement, hush_money, censorship).
    /// </summary>
    public string ActionTaken { get; init; } = string.Empty;

    /// <summary>
    /// Gets the detailed outcome description reported back to the President.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the liquid REPU cost deducted from the national treasury.
    /// </summary>
    public double CostDeducted { get; init; }

    /// <summary>
    /// Gets the net change in presidential approval resulting from this action.
    /// </summary>
    public double ApprovalDelta { get; init; }

    /// <summary>
    /// Gets the net change in national stability resulting from this action.
    /// </summary>
    public double StabilityDelta { get; init; }

    /// <summary>
    /// Gets a value indicating whether a covert action was leaked or exposed to the public.
    /// </summary>
    public bool WasExposed { get; init; }
}
