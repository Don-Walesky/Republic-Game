namespace Republic.Core.Cabinet.Models;

/// <summary>
/// Domain model representing an appointed executive cabinet minister.
/// Encapsulates five normalized scores (0.0 to 1.0): Competence, Loyalty, Integrity, Experience, and Political Connections.
/// </summary>
public sealed class Minister
{
    private double _competence = 0.8;
    private double _loyalty = 0.75;
    private double _integrity = 0.8;
    private double _experience = 0.7;
    private double _politicalConnections = 0.5;
    private double _competenceRating = 80.0;
    private double _loyaltyRating = 75.0;

    /// <summary>
    /// Gets the unique identifier of the minister.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets the full name of the minister.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the assigned cabinet ministerial portfolio.
    /// </summary>
    public CabinetPortfolio Portfolio { get; set; }

    /// <summary>
    /// Gets or sets the minister's administrative and domain competence (0.0 to 1.0).
    /// Clamped between 0.0 and 1.0.
    /// </summary>
    public double Competence
    {
        get => _competence;
        set
        {
            _competence = Math.Clamp(value, 0.0, 1.0);
            _competenceRating = _competence * 100.0;
        }
    }

    /// <summary>
    /// Gets or sets the minister's political loyalty to the government (0.0 to 1.0).
    /// Clamped between 0.0 and 1.0. Recorded for politics slice; does not raise State Capacity.
    /// </summary>
    public double Loyalty
    {
        get => _loyalty;
        set
        {
            _loyalty = Math.Clamp(value, 0.0, 1.0);
            _loyaltyRating = _loyalty * 100.0;
        }
    }

    /// <summary>
    /// Gets or sets the minister's personal integrity and incorruptibility (0.0 to 1.0).
    /// Clamped between 0.0 and 1.0.
    /// </summary>
    public double Integrity
    {
        get => _integrity;
        set => _integrity = Math.Clamp(value, 0.0, 1.0);
    }

    /// <summary>
    /// Gets or sets the minister's professional and civil service experience (0.0 to 1.0).
    /// Clamped between 0.0 and 1.0.
    /// </summary>
    public double Experience
    {
        get => _experience;
        set => _experience = Math.Clamp(value, 0.0, 1.0);
    }

    /// <summary>
    /// Gets or sets the minister's network of parliamentary and political connections (0.0 to 1.0).
    /// Clamped between 0.0 and 1.0. Recorded for politics slice; does not raise State Capacity.
    /// </summary>
    public double PoliticalConnections
    {
        get => _politicalConnections;
        set => _politicalConnections = Math.Clamp(value, 0.0, 1.0);
    }

    /// <summary>
    /// Legacy loyalty rating (0.0 to 100.0). Synchronized with <see cref="Loyalty"/>.
    /// </summary>
    public double LoyaltyRating
    {
        get => _loyaltyRating;
        set
        {
            _loyaltyRating = value;
            _loyalty = Math.Clamp(value > 1.0 ? value / 100.0 : value, 0.0, 1.0);
        }
    }

    /// <summary>
    /// Legacy competence rating (0.0 to 100.0). Synchronized with <see cref="Competence"/>.
    /// </summary>
    public double CompetenceRating
    {
        get => _competenceRating;
        set
        {
            _competenceRating = value;
            _competence = Math.Clamp(value > 1.0 ? value / 100.0 : value, 0.0, 1.0);
        }
    }

    /// <summary>
    /// Gets the faction alignment identifier.
    /// </summary>
    public string FactionAlignmentId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the minister currently holds an active appointment.
    /// </summary>
    public bool IsAppointed { get; set; }

    /// <summary>
    /// Gets or sets the minister's monthly salary.
    /// </summary>
    public double MonthlySalary { get; set; } = 150_000.0;
}
