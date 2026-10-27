namespace Republic.Core.Media.Models;

/// <summary>
/// Domain model representing a news or broadcast organisation in the country's living media ecosystem.
/// </summary>
public sealed class MediaOutlet
{
    /// <summary>
    /// Gets the unique identifier of the media outlet.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the publication or broadcast name of the outlet.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the core editorial stance and perspective of the outlet.
    /// </summary>
    public MediaEditorialLeaning Leaning { get; init; }

    /// <summary>
    /// Gets or sets the audience reach of the outlet as a percentage of the sovereign population (0.0% to 100.0%).
    /// </summary>
    public double AudienceReach { get; set; } = 50.0;

    /// <summary>
    /// Gets or sets the journalistic credibility rating of the outlet (0.0% to 100.0%).
    /// Directly bounds the public impact of reports published by this outlet.
    /// </summary>
    public double Credibility { get; set; } = 70.0;

    /// <summary>
    /// Gets or sets the outlet's current editorial attitude toward the presidential administration.
    /// Ranges from -100.0 (Hostile) to +100.0 (Fervently Supportive).
    /// </summary>
    public double GovernmentAttitude { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets a value indicating whether the outlet is currently subject to executive gag orders or press restrictions.
    /// </summary>
    public bool IsCensored { get; set; }

    /// <summary>
    /// Gets or sets the count of executive press interventions or warnings issued to this outlet.
    /// </summary>
    public int ConsecutiveStrikes { get; set; }
}
