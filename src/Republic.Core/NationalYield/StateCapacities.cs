namespace Republic.Core.NationalYield;

/// <summary>
/// Domain model encapsulating sovereign governance, administrative, security, intelligence, defense, and diplomatic capacities.
/// </summary>
public sealed class StateCapacities
{
    /// <summary>
    /// Gets or sets civil service competence, bureaucratic bandwidth, institutional integrity, and policy implementation throughput.
    /// </summary>
    public double AdministrativeCapacity { get; set; }

    /// <summary>
    /// Gets or sets internal law enforcement effectiveness, border security integrity, crime deterrence, and rule of law strength.
    /// </summary>
    public double SecurityCapacity { get; set; }

    /// <summary>
    /// Gets or sets intelligence gathering bandwidth, surveillance analysis, counter-espionage, and national threat detection capability.
    /// </summary>
    public double IntelligenceCapacity { get; set; }

    /// <summary>
    /// Gets or sets armed forces training, military hardware operational readiness, doctrinal competence, and mobilization speed.
    /// </summary>
    public double MilitaryReadiness { get; set; }

    /// <summary>
    /// Gets or sets diplomatic corps capability, bilateral treaty negotiation bandwidth, alliance management, and international soft power.
    /// </summary>
    public double DiplomaticCapacity { get; set; }

    /// <summary>
    /// Creates a deep clone of the current state capacities.
    /// </summary>
    public StateCapacities Clone() => new()
    {
        AdministrativeCapacity = AdministrativeCapacity,
        SecurityCapacity = SecurityCapacity,
        IntelligenceCapacity = IntelligenceCapacity,
        MilitaryReadiness = MilitaryReadiness,
        DiplomaticCapacity = DiplomaticCapacity
    };
}
