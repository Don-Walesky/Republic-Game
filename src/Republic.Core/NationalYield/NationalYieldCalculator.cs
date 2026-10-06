namespace Republic.Core.NationalYield;

/// <summary>
/// Isolated, deterministic calculation engine for sovereign National Yield.
/// Evaluates <see cref="NationalYieldCalculationInputs"/> across a 6-stage pipeline (ADR-0006)
/// to produce a calculated <see cref="NationalYield"/> result without mutating external or input state.
/// </summary>
public sealed class NationalYieldCalculator : INationalYieldCalculator
{
    /// <inheritdoc />
    public NationalYield Calculate(NationalYieldCalculationInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        // =====================================================================
        // STAGE 1: INPUT SNAPSHOTTING & RESOLUTION
        // Read input factors safely; inputs remain completely immutable.
        // =====================================================================
        var economicBase = Math.Max(0.0, inputs.EconomicCapacity);
        var govtEffectiveness = Math.Max(0.0, inputs.GovernmentEffectiveness);
        var infraCondition = Math.Max(0.0, inputs.InfrastructureCondition);
        var humanCapital = Math.Max(0.0, inputs.HumanCapital);
        var scienceTech = Math.Max(0.0, inputs.ScienceTechnology);
        var stability = Math.Max(0.0, inputs.PoliticalStability);
        var security = Math.Max(0.0, inputs.SecurityLevel);
        var cabinet = Math.Max(0.0, inputs.CabinetEffectiveness);

        // Deterministic modifier ordering: sort by Id ordinal to eliminate collection iteration variance
        var sortedModifiers = inputs.Modifiers
            .OrderBy(m => m.Id, StringComparer.Ordinal)
            .ToList();

        // =====================================================================
        // STAGE 2: BASE POTENTIAL EVALUATION
        // Compute base structural potential directly from available factors.
        // Categories requiring future economic/domain inputs use explicit baselines.
        // =====================================================================
        var industrial = economicBase;
        var infrastructure = infraCondition;
        var energy = (infraCondition + economicBase) * 0.5;
        var financial = (economicBase + stability) * 0.5;

        var humanCap = humanCapital;
        var science = scienceTech;
        var innovation = (scienceTech + humanCapital) * 0.5;

        var admin = govtEffectiveness;
        var securityCap = security;
        var intelligence = (security + govtEffectiveness) * 0.5;
        var military = (security + cabinet) * 0.5;
        var diplomatic = (govtEffectiveness + stability) * 0.5;

        var naturalResource = economicBase * 0.5;

        // Flow baseline for REPU Treasury Revenue
        var repuRevenue = economicBase * 1000.0;

        // =====================================================================
        // STAGE 3: CROSS-CATEGORY UPSTREAM RELATIONSHIPS
        // Propagate structural upstream synergies (Science, Energy, Infrastructure).
        // Uses simple, transparent relationships without arbitrary game-balancing curves.
        // =====================================================================
        if (science > 0.0)
        {
            innovation += science * 0.1;
        }

        if (energy > 0.0 && infrastructure > 0.0)
        {
            industrial += (energy + infrastructure) * 0.05;
        }

        // =====================================================================
        // STAGE 4: STATE CAPACITY / GOVERNANCE CONVERSION
        // Evaluate the country's institutional ability to convert potential into output.
        // Blends government effectiveness, political stability, and cabinet leadership.
        // =====================================================================
        var governanceIndex = (govtEffectiveness + stability + cabinet) / 3.0;
        var conversionFactor = governanceIndex > 0.0
            ? 0.5 + (governanceIndex / 200.0) // Neutral midpoint ~1.0 when index is 100
            : 1.0;

        industrial *= conversionFactor;
        repuRevenue *= conversionFactor;
        admin *= conversionFactor;

        // =====================================================================
        // STAGE 5: NATIONAL & CATEGORY MODIFIERS
        // Evaluate active modifiers targeting specific categories or broad scope.
        // Simple additive application for this baseline step; non-negativity clamped.
        // =====================================================================
        repuRevenue = ApplyModifiers(repuRevenue, NationalYieldCategory.RepuTreasuryRevenue, sortedModifiers);
        industrial = ApplyModifiers(industrial, NationalYieldCategory.IndustrialCapacity, sortedModifiers);
        energy = ApplyModifiers(energy, NationalYieldCategory.EnergyCapacity, sortedModifiers);
        infrastructure = ApplyModifiers(infrastructure, NationalYieldCategory.InfrastructureCapacity, sortedModifiers);
        financial = ApplyModifiers(financial, NationalYieldCategory.FinancialCapacity, sortedModifiers);

        humanCap = ApplyModifiers(humanCap, NationalYieldCategory.HumanCapital, sortedModifiers);
        science = ApplyModifiers(science, NationalYieldCategory.ScienceCapacity, sortedModifiers);
        innovation = ApplyModifiers(innovation, NationalYieldCategory.InnovationCapacity, sortedModifiers);

        admin = ApplyModifiers(admin, NationalYieldCategory.AdministrativeCapacity, sortedModifiers);
        securityCap = ApplyModifiers(securityCap, NationalYieldCategory.SecurityCapacity, sortedModifiers);
        intelligence = ApplyModifiers(intelligence, NationalYieldCategory.IntelligenceCapacity, sortedModifiers);
        military = ApplyModifiers(military, NationalYieldCategory.MilitaryReadiness, sortedModifiers);
        diplomatic = ApplyModifiers(diplomatic, NationalYieldCategory.DiplomaticCapacity, sortedModifiers);

        naturalResource = ApplyModifiers(naturalResource, NationalYieldCategory.NaturalResourceOutput, sortedModifiers);

        // =====================================================================
        // STAGE 6: FINAL CATEGORY OUTPUT & FLOW RESOLUTION
        // Allocate a new NationalYield instance containing all 14 canonical categories.
        // =====================================================================
        return new NationalYield
        {
            RepuTreasuryRevenue = Math.Max(0.0, repuRevenue),
            IndustrialCapacity = Math.Max(0.0, industrial),
            EnergyCapacity = Math.Max(0.0, energy),
            InfrastructureCapacity = Math.Max(0.0, infrastructure),
            FinancialCapacity = Math.Max(0.0, financial),

            HumanCapital = Math.Max(0.0, humanCap),
            ScienceCapacity = Math.Max(0.0, science),
            InnovationCapacity = Math.Max(0.0, innovation),

            AdministrativeCapacity = Math.Max(0.0, admin),
            SecurityCapacity = Math.Max(0.0, securityCap),
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
