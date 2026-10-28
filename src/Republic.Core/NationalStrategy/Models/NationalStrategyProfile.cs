namespace Republic.Core.NationalStrategy.Models;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Authoritative state record representing a country's active national strategic direction,
/// historical transition friction, adoption timeline, and alignment metrics.
/// </summary>
public sealed class NationalStrategyProfile
{
    /// <summary>
    /// Gets or sets the active national strategic direction.
    /// </summary>
    [JsonPropertyName("direction")]
    public NationalStrategicDirection Direction { get; set; } = NationalStrategicDirection.BalancedGrowth;

    /// <summary>
    /// Gets or sets the simulation day on which this strategic direction was adopted.
    /// </summary>
    [JsonPropertyName("adopted_day")]
    public long AdoptedDay { get; set; } = 1;

    /// <summary>
    /// Gets or sets the previous strategic direction prior to the latest change, if any.
    /// </summary>
    [JsonPropertyName("previous_direction")]
    public NationalStrategicDirection? PreviousDirection { get; set; }

    /// <summary>
    /// Gets or sets the number of days remaining in the transition friction period.
    /// During transition, administrative realignment introduces temporary frictions.
    /// </summary>
    [JsonPropertyName("transition_days_remaining")]
    public int TransitionDaysRemaining { get; set; }

    /// <summary>
    /// Gets or sets whether the country is currently in an active strategic transition period.
    /// </summary>
    [JsonPropertyName("is_transitioning")]
    public bool IsTransitioning => TransitionDaysRemaining > 0;

    /// <summary>
    /// Gets or sets the cumulative progress index towards the strategic direction's goals (0.0 to 100.0).
    /// </summary>
    [JsonPropertyName("progress_index")]
    public double ProgressIndex { get; set; } = 10.0;

    /// <summary>
    /// Gets or sets the total treasury funds expended in transition/retooling costs.
    /// </summary>
    [JsonPropertyName("transition_cost_incurred")]
    public double TransitionCostIncurred { get; set; }

    /// <summary>
    /// Gets or sets an executive summary statement of the national strategic direction.
    /// </summary>
    [JsonPropertyName("focus_statement")]
    public string FocusStatement { get; set; } = string.Empty;

    /// <summary>
    /// Gets the list of priority economic sectors aligned with the active direction.
    /// </summary>
    [JsonPropertyName("priority_sectors")]
    public List<string> PrioritySectors { get; set; } = new();

    /// <summary>
    /// Formats a concise summary string for UI dashboards and executive briefings.
    /// </summary>
    public string FormatSummary()
    {
        var dirName = Direction.GetDisplayName();
        if (IsTransitioning)
        {
            return $"{dirName} (Transitioning: {TransitionDaysRemaining}d remaining, Progress: {ProgressIndex:0.#}%)";
        }

        return $"{dirName} (Progress: {ProgressIndex:0.#}%)";
    }
}
