namespace Republic.Core.NationalYield;

using System.Text.Json.Serialization;

/// <summary>
/// Domain model encapsulating the measurable productive, human, institutional, and resource capacities of a sovereign nation.
/// National Yield represents what a functioning country is capable of producing, generating, or sustaining.
/// Note: Food is intentionally excluded from National Yield as it belongs to the emergent civilian economy.
/// Final calibration and yield calculation formulas belong to subsequent simulation steps.
/// </summary>
public sealed class NationalYield
{
    /// <summary>
    /// Gets or sets the economic and productive capacities of the country.
    /// </summary>
    public EconomicCapacities Economic { get; set; } = new();

    /// <summary>
    /// Gets or sets the human, educational, scientific, and innovation capacities of the country.
    /// </summary>
    public HumanKnowledgeCapacities HumanKnowledge { get; set; } = new();

    /// <summary>
    /// Gets or sets the administrative, security, intelligence, military, and diplomatic capacities of the country.
    /// </summary>
    public StateCapacities State { get; set; } = new();

    /// <summary>
    /// Gets or sets the natural resource output of the country.
    /// </summary>
    public ResourceOutputCapacities ResourceOutput { get; set; } = new();

    // =========================================================================
    // Direct Convenience Properties (14 Categories)
    // =========================================================================

    #region Economic Capacities

    /// <summary>
    /// Gets or sets sovereign monetary revenue in REPU (R).
    /// </summary>
    [JsonIgnore]
    public double RepuTreasuryRevenue
    {
        get => Economic.RepuTreasuryRevenue;
        set => Economic.RepuTreasuryRevenue = value;
    }

    /// <summary>
    /// Gets or sets manufacturing base, heavy industry, construction throughput, and fabrication capability.
    /// </summary>
    [JsonIgnore]
    public double IndustrialCapacity
    {
        get => Economic.IndustrialCapacity;
        set => Economic.IndustrialCapacity = value;
    }

    /// <summary>
    /// Gets or sets power grid output, fuel generation, energy reserves, and electrical distribution stability.
    /// </summary>
    [JsonIgnore]
    public double EnergyCapacity
    {
        get => Economic.EnergyCapacity;
        set => Economic.EnergyCapacity = value;
    }

    /// <summary>
    /// Gets or sets physical transport networks, logistics corridors, deepwater ports, rail, and telecommunications throughput.
    /// </summary>
    [JsonIgnore]
    public double InfrastructureCapacity
    {
        get => Economic.InfrastructureCapacity;
        set => Economic.InfrastructureCapacity = value;
    }

    /// <summary>
    /// Gets or sets sovereign financial sector depth, domestic banking capitalization, credit rating, and capital liquidity.
    /// </summary>
    [JsonIgnore]
    public double FinancialCapacity
    {
        get => Economic.FinancialCapacity;
        set => Economic.FinancialCapacity = value;
    }

    #endregion

    #region Human & Knowledge Capacities

    /// <summary>
    /// Gets or sets national workforce skill level, educational attainment, technical literacy, and public health vitality.
    /// </summary>
    [JsonIgnore]
    public double HumanCapital
    {
        get => HumanKnowledge.HumanCapital;
        set => HumanKnowledge.HumanCapital = value;
    }

    /// <summary>
    /// Gets or sets academic institution strength, laboratory infrastructure, scientific research bandwidth, and patent output.
    /// </summary>
    [JsonIgnore]
    public double ScienceCapacity
    {
        get => HumanKnowledge.ScienceCapacity;
        set => HumanKnowledge.ScienceCapacity = value;
    }

    /// <summary>
    /// Gets or sets commercial technology adoption, entrepreneurial momentum, operational modernism, and efficiency gains.
    /// </summary>
    [JsonIgnore]
    public double InnovationCapacity
    {
        get => HumanKnowledge.InnovationCapacity;
        set => HumanKnowledge.InnovationCapacity = value;
    }

    #endregion

    #region State Capacities

    /// <summary>
    /// Gets or sets civil service competence, bureaucratic bandwidth, institutional integrity, and policy implementation throughput.
    /// </summary>
    [JsonIgnore]
    public double AdministrativeCapacity
    {
        get => State.AdministrativeCapacity;
        set => State.AdministrativeCapacity = value;
    }

    /// <summary>
    /// Gets or sets internal law enforcement effectiveness, border security integrity, crime deterrence, and rule of law strength.
    /// </summary>
    [JsonIgnore]
    public double SecurityCapacity
    {
        get => State.SecurityCapacity;
        set => State.SecurityCapacity = value;
    }

    /// <summary>
    /// Gets or sets intelligence gathering bandwidth, surveillance analysis, counter-espionage, and national threat detection capability.
    /// </summary>
    [JsonIgnore]
    public double IntelligenceCapacity
    {
        get => State.IntelligenceCapacity;
        set => State.IntelligenceCapacity = value;
    }

    /// <summary>
    /// Gets or sets armed forces training, military hardware operational readiness, doctrinal competence, and mobilization speed.
    /// </summary>
    [JsonIgnore]
    public double MilitaryReadiness
    {
        get => State.MilitaryReadiness;
        set => State.MilitaryReadiness = value;
    }

    /// <summary>
    /// Gets or sets diplomatic corps capability, bilateral treaty negotiation bandwidth, alliance management, and international soft power.
    /// </summary>
    [JsonIgnore]
    public double DiplomaticCapacity
    {
        get => State.DiplomaticCapacity;
        set => State.DiplomaticCapacity = value;
    }

    #endregion

    #region Resource Output

    /// <summary>
    /// Gets or sets sovereign raw natural resource output (hydrocarbons, industrial ores, timber, rare earths) where geographically applicable.
    /// </summary>
    [JsonIgnore]
    public double NaturalResourceOutput
    {
        get => ResourceOutput.NaturalResourceOutput;
        set => ResourceOutput.NaturalResourceOutput = value;
    }

    #endregion

    // =========================================================================
    // Category-Based Accessors & Utility
    // =========================================================================

    /// <summary>
    /// Retrieves the capacity value for a specific <see cref="NationalYieldCategory"/>.
    /// </summary>
    public double GetCapacity(NationalYieldCategory category) => category switch
    {
        NationalYieldCategory.RepuTreasuryRevenue => Economic.RepuTreasuryRevenue,
        NationalYieldCategory.IndustrialCapacity => Economic.IndustrialCapacity,
        NationalYieldCategory.EnergyCapacity => Economic.EnergyCapacity,
        NationalYieldCategory.InfrastructureCapacity => Economic.InfrastructureCapacity,
        NationalYieldCategory.FinancialCapacity => Economic.FinancialCapacity,
        NationalYieldCategory.HumanCapital => HumanKnowledge.HumanCapital,
        NationalYieldCategory.ScienceCapacity => HumanKnowledge.ScienceCapacity,
        NationalYieldCategory.InnovationCapacity => HumanKnowledge.InnovationCapacity,
        NationalYieldCategory.AdministrativeCapacity => State.AdministrativeCapacity,
        NationalYieldCategory.SecurityCapacity => State.SecurityCapacity,
        NationalYieldCategory.IntelligenceCapacity => State.IntelligenceCapacity,
        NationalYieldCategory.MilitaryReadiness => State.MilitaryReadiness,
        NationalYieldCategory.DiplomaticCapacity => State.DiplomaticCapacity,
        NationalYieldCategory.NaturalResourceOutput => ResourceOutput.NaturalResourceOutput,
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown National Yield category.")
    };

    /// <summary>
    /// Sets the capacity value for a specific <see cref="NationalYieldCategory"/>.
    /// </summary>
    public void SetCapacity(NationalYieldCategory category, double value)
    {
        switch (category)
        {
            case NationalYieldCategory.RepuTreasuryRevenue:
                Economic.RepuTreasuryRevenue = value;
                break;
            case NationalYieldCategory.IndustrialCapacity:
                Economic.IndustrialCapacity = value;
                break;
            case NationalYieldCategory.EnergyCapacity:
                Economic.EnergyCapacity = value;
                break;
            case NationalYieldCategory.InfrastructureCapacity:
                Economic.InfrastructureCapacity = value;
                break;
            case NationalYieldCategory.FinancialCapacity:
                Economic.FinancialCapacity = value;
                break;
            case NationalYieldCategory.HumanCapital:
                HumanKnowledge.HumanCapital = value;
                break;
            case NationalYieldCategory.ScienceCapacity:
                HumanKnowledge.ScienceCapacity = value;
                break;
            case NationalYieldCategory.InnovationCapacity:
                HumanKnowledge.InnovationCapacity = value;
                break;
            case NationalYieldCategory.AdministrativeCapacity:
                State.AdministrativeCapacity = value;
                break;
            case NationalYieldCategory.SecurityCapacity:
                State.SecurityCapacity = value;
                break;
            case NationalYieldCategory.IntelligenceCapacity:
                State.IntelligenceCapacity = value;
                break;
            case NationalYieldCategory.MilitaryReadiness:
                State.MilitaryReadiness = value;
                break;
            case NationalYieldCategory.DiplomaticCapacity:
                State.DiplomaticCapacity = value;
                break;
            case NationalYieldCategory.NaturalResourceOutput:
                ResourceOutput.NaturalResourceOutput = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown National Yield category.");
        }
    }

    /// <summary>
    /// Creates an independent deep clone of the National Yield state.
    /// </summary>
    public NationalYield Clone() => new()
    {
        Economic = Economic.Clone(),
        HumanKnowledge = HumanKnowledge.Clone(),
        State = State.Clone(),
        ResourceOutput = ResourceOutput.Clone()
    };

    /// <summary>
    /// Creates an independent copy of the National Yield with all 14 canonical categories scaled by the specified multiplier.
    /// </summary>
    /// <param name="multiplier">The non-negative multiplier to apply.</param>
    /// <returns>A newly allocated <see cref="NationalYield"/> with scaled capacities.</returns>
    public NationalYield Multiply(double multiplier)
    {
        if (multiplier < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), "Yield multiplier cannot be negative.");
        }

        return new NationalYield
        {
            RepuTreasuryRevenue = RepuTreasuryRevenue * multiplier,
            IndustrialCapacity = IndustrialCapacity * multiplier,
            EnergyCapacity = EnergyCapacity * multiplier,
            InfrastructureCapacity = InfrastructureCapacity * multiplier,
            FinancialCapacity = FinancialCapacity * multiplier,
            HumanCapital = HumanCapital * multiplier,
            ScienceCapacity = ScienceCapacity * multiplier,
            InnovationCapacity = InnovationCapacity * multiplier,
            AdministrativeCapacity = AdministrativeCapacity * multiplier,
            SecurityCapacity = SecurityCapacity * multiplier,
            IntelligenceCapacity = IntelligenceCapacity * multiplier,
            MilitaryReadiness = MilitaryReadiness * multiplier,
            DiplomaticCapacity = DiplomaticCapacity * multiplier,
            NaturalResourceOutput = NaturalResourceOutput * multiplier
        };
    }

    /// <summary>
    /// Multiplies all 14 canonical categories of the National Yield by the specified scalar.
    /// </summary>
    public static NationalYield operator *(NationalYield yield, double multiplier)
    {
        ArgumentNullException.ThrowIfNull(yield);
        return yield.Multiply(multiplier);
    }

    /// <summary>
    /// Multiplies all 14 canonical categories of the National Yield by the specified scalar.
    /// </summary>
    public static NationalYield operator *(double multiplier, NationalYield yield) => yield * multiplier;
}

