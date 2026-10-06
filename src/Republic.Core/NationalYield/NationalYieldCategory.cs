namespace Republic.Core.NationalYield;

/// <summary>
/// Strongly typed enumeration identifying the 14 canonical categories of National Yield.
/// National Yield represents measurable productive output and accumulating state capacity, not arbitrary free resource deposits.
/// Note: Food is intentionally excluded as it belongs to the emergent civilian economy.
/// </summary>
public enum NationalYieldCategory
{
    // --- Economic / Productive Capacity ---

    /// <summary>
    /// Sovereign monetary revenue (tax collections, state enterprise returns, sovereign wealth flows) denominated in REPU (R).
    /// </summary>
    RepuTreasuryRevenue = 0,

    /// <summary>
    /// Manufacturing base, heavy industry, construction throughput, and material fabrication capability.
    /// </summary>
    IndustrialCapacity = 1,

    /// <summary>
    /// National power grid output, fuel generation, energy reserves, and electrical distribution stability.
    /// </summary>
    EnergyCapacity = 2,

    /// <summary>
    /// Physical transport networks, logistics corridors, deepwater ports, rail networks, and telecommunications throughput.
    /// </summary>
    InfrastructureCapacity = 3,

    /// <summary>
    /// Sovereign financial sector depth, domestic banking capitalization, credit rating, and capital liquidity.
    /// </summary>
    FinancialCapacity = 4,

    // --- Human / Knowledge Capacity ---

    /// <summary>
    /// Workforce skill level, educational attainment, technical specialization, and public health vitality.
    /// </summary>
    HumanCapital = 5,

    /// <summary>
    /// Academic institutions, scientific laboratories, theoretical research bandwidth, and patent output.
    /// </summary>
    ScienceCapacity = 6,

    /// <summary>
    /// Commercial technology adoption, entrepreneurial activity, business model modernism, and operational efficiency gains.
    /// </summary>
    InnovationCapacity = 7,

    // --- State Capacity ---

    /// <summary>
    /// Civil service competence, regulatory execution bandwidth, institutional integrity, and policy implementation throughput.
    /// </summary>
    AdministrativeCapacity = 8,

    /// <summary>
    /// Internal policing, judicial enforcement, border security, crime deterrence, and institutional rule of law.
    /// </summary>
    SecurityCapacity = 9,

    /// <summary>
    /// Foreign and domestic intelligence analysis bandwidth, counter-surveillance, threat detection, and national espionage capability.
    /// </summary>
    IntelligenceCapacity = 10,

    /// <summary>
    /// Armed forces readiness, military equipment maintenance, tactical doctrine execution, and combat mobilization speed.
    /// </summary>
    MilitaryReadiness = 11,

    /// <summary>
    /// Diplomatic corps bandwidth, international treaty negotiation leverage, alliance maintenance, and global soft power.
    /// </summary>
    DiplomaticCapacity = 12,

    // --- Resource Output ---

    /// <summary>
    /// Raw strategic resource extraction capability (oil, gas, minerals, timber, strategic metals) where geographically applicable.
    /// </summary>
    NaturalResourceOutput = 13
}
