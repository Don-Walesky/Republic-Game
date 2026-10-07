namespace Republic.Core.NationalYield;

/// <summary>
/// Isolated, deterministic calculation engine for sovereign National Yield.
/// Evaluates <see cref="NationalYieldCalculationInputs"/> across the 6-stage pipeline (ADR-0006).
/// Implements a simplified, clean calibration model: direct relationships map directly,
/// and enabling factors act as transparent bottleneck constraints on dependent capabilities.
/// </summary>
public sealed class NationalYieldCalculator : INationalYieldCalculator
{
    /// <inheritdoc />
    public NationalYield Calculate(NationalYieldCalculationInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        // =====================================================================
        // STAGE 1: INPUT SNAPSHOTTING & RESOLUTION
        // Read input factors safely; guarantee non-negativity across all inputs.
        // Inputs remain completely immutable.
        // =====================================================================
        var economicCapacity = Math.Max(0.0, inputs.EconomicCapacity);
        var govtEffectiveness = Math.Max(0.0, inputs.GovernmentEffectiveness);
        var infraCondition = Math.Max(0.0, inputs.InfrastructureCondition);
        var humanCapital = Math.Max(0.0, inputs.HumanCapital);
        var scienceTech = Math.Max(0.0, inputs.ScienceTechnology);
        var stability = Math.Max(0.0, inputs.PoliticalStability);
        var securityLevel = Math.Max(0.0, inputs.SecurityLevel);
        var cabinetEffectiveness = Math.Max(0.0, inputs.CabinetEffectiveness);

        var sortedModifiers = inputs.Modifiers
            .OrderBy(m => m.Id, StringComparer.Ordinal)
            .ToList();

        // =====================================================================
        // STAGE 2: BASE POTENTIAL EVALUATION
        // Direct baseline mappings where clear conceptual relationships exist.
        // =====================================================================
        var human = humanCapital;
        var infrastructure = infraCondition;
        var admin = govtEffectiveness;
        var security = securityLevel;

        // Energy capability requires physical infrastructure to generate and distribute power
        var energy = infrastructure;

        // Intelligence capability is rooted in the domestic security apparatus
        var intelligence = security;

        // =====================================================================
        // STAGE 3: CROSS-CATEGORY RELATIONSHIPS & ENABLING CONSTRAINTS
        // Direct relationships map simply; enabling factors constrain dependent capabilities.
        // =====================================================================

        // Scientific research capacity is supported and constrained by educated human capital
        var science = Math.Min(scienceTech, human);

        // Strong science directly powers commercial innovation
        var innovation = science;

        // Weak infrastructure constrains industrial fabrication throughput
        var industrial = Math.Min(economicCapacity, infrastructure);

        // Natural resource extraction is constrained by physical transport infrastructure
        var naturalResource = Math.Min(economicCapacity, infrastructure);

        // Commercial banking depth requires political stability to prevent capital flight
        var financial = Math.Min(economicCapacity, stability);

        // Military defense readiness depends on security, constrained by industrial equipment
        var military = Math.Min(security, industrial);

        // Diplomatic capacity reflects institutional governance and political stability
        var diplomatic = Math.Min(admin, stability);

        // =====================================================================
        // STAGE 4: STATE CAPACITY / GOVERNANCE CONVERSION & REPU REVENUE FLOW
        // Economic revenue emerges from the productive base (industry or natural resources),
        // but realization is constrained by administrative collection capacity against leakage.
        // =====================================================================
        var revenueBase = Math.Max(industrial, naturalResource);
        var repuRevenue = Math.Min(revenueBase, admin);

        // =====================================================================
        // STAGE 5: NATIONAL & CATEGORY MODIFIERS
        // Apply active modifiers deterministically with non-negativity clamping.
        // =====================================================================
        repuRevenue = ApplyModifiers(repuRevenue, NationalYieldCategory.RepuTreasuryRevenue, sortedModifiers);
        industrial = ApplyModifiers(industrial, NationalYieldCategory.IndustrialCapacity, sortedModifiers);
        energy = ApplyModifiers(energy, NationalYieldCategory.EnergyCapacity, sortedModifiers);
        infrastructure = ApplyModifiers(infrastructure, NationalYieldCategory.InfrastructureCapacity, sortedModifiers);
        financial = ApplyModifiers(financial, NationalYieldCategory.FinancialCapacity, sortedModifiers);

        human = ApplyModifiers(human, NationalYieldCategory.HumanCapital, sortedModifiers);
        science = ApplyModifiers(science, NationalYieldCategory.ScienceCapacity, sortedModifiers);
        innovation = ApplyModifiers(innovation, NationalYieldCategory.InnovationCapacity, sortedModifiers);

        admin = ApplyModifiers(admin, NationalYieldCategory.AdministrativeCapacity, sortedModifiers);
        security = ApplyModifiers(security, NationalYieldCategory.SecurityCapacity, sortedModifiers);
        intelligence = ApplyModifiers(intelligence, NationalYieldCategory.IntelligenceCapacity, sortedModifiers);
        military = ApplyModifiers(military, NationalYieldCategory.MilitaryReadiness, sortedModifiers);
        diplomatic = ApplyModifiers(diplomatic, NationalYieldCategory.DiplomaticCapacity, sortedModifiers);

        naturalResource = ApplyModifiers(naturalResource, NationalYieldCategory.NaturalResourceOutput, sortedModifiers);

        // =====================================================================
        // STAGE 6: FINAL CATEGORY OUTPUT & FLOW RESOLUTION
        // Allocate a new, independent NationalYield instance containing all 14 canonical categories.
        // =====================================================================
        return new NationalYield
        {
            RepuTreasuryRevenue = Math.Max(0.0, repuRevenue),
            IndustrialCapacity = Math.Max(0.0, industrial),
            EnergyCapacity = Math.Max(0.0, energy),
            InfrastructureCapacity = Math.Max(0.0, infrastructure),
            FinancialCapacity = Math.Max(0.0, financial),

            HumanCapital = Math.Max(0.0, human),
            ScienceCapacity = Math.Max(0.0, science),
            InnovationCapacity = Math.Max(0.0, innovation),

            AdministrativeCapacity = Math.Max(0.0, admin),
            SecurityCapacity = Math.Max(0.0, security),
            IntelligenceCapacity = Math.Max(0.0, intelligence),
            MilitaryReadiness = Math.Max(0.0, military),
            DiplomaticCapacity = Math.Max(0.0, diplomatic),

            NaturalResourceOutput = Math.Max(0.0, naturalResource)
        };
    }

    private static double ApplyModifiers(
        double baseValue,
        NationalYieldCategory category,
        IReadOnlyList<NationalYieldModifier> modifiers)
    {
        var result = baseValue;
        for (var i = 0; i < modifiers.Count; i++)
        {
            var modifier = modifiers[i];
            if (modifier.TargetCategory == null || modifier.TargetCategory == category)
            {
                result += modifier.Value;
            }
        }

        return Math.Max(0.0, result);
    }
}
