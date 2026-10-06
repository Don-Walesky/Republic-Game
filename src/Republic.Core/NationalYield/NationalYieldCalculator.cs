namespace Republic.Core.NationalYield;

/// <summary>
/// Isolated, deterministic calculation engine for sovereign National Yield.
/// Evaluates <see cref="NationalYieldCalculationInputs"/> across the 6-stage pipeline (ADR-0006).
/// Provides a minimal neutral baseline without hard-coded economic coefficients, arbitrary balancing curves,
/// or premature State Capacity formulas, leaving mathematical calibration for subsequent design steps.
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
        var sortedModifiers = inputs.Modifiers
            .OrderBy(m => m.Id, StringComparer.Ordinal)
            .ToList();

        // =====================================================================
        // STAGE 2: BASE POTENTIAL EVALUATION
        // Direct mappings are used where a straightforward conceptual relationship exists.
        // Categories requiring future domain/economic models use an explicit neutral baseline (0.0).
        // Arbitrary conversion multipliers, GDP formulas, and tax rates are intentionally excluded.
        // =====================================================================
        var industrial = Math.Max(0.0, inputs.EconomicCapacity);
        var infrastructure = Math.Max(0.0, inputs.InfrastructureCondition);
        var humanCapital = Math.Max(0.0, inputs.HumanCapital);
        var science = Math.Max(0.0, inputs.ScienceTechnology);
        var admin = Math.Max(0.0, inputs.GovernmentEffectiveness);
        var security = Math.Max(0.0, inputs.SecurityLevel);

        // Explicit neutral baselines for categories requiring dedicated future domain models
        var energy = 0.0;
        var financial = 0.0;
        var innovation = 0.0;
        var intelligence = 0.0;
        var military = 0.0;
        var diplomatic = 0.0;
        var naturalResource = 0.0;
        var repuRevenue = 0.0;

        // =====================================================================
        // STAGE 3: CROSS-CATEGORY UPSTREAM RELATIONSHIPS
        // Architectural boundary for upstream synergies (Science, Energy, Infrastructure).
        // Numerical multipliers and cross-system coefficients are intentionally deferred.
        // Current baseline passes through base potential values unchanged.
        // =====================================================================

        // =====================================================================
        // STAGE 4: STATE CAPACITY / GOVERNANCE CONVERSION
        // Architectural boundary for institutional conversion efficiency.
        // Mathematical conversion formulas and coefficients are intentionally deferred to future governance calibration.
        // Current baseline passes through potential values unchanged.
        // =====================================================================

        // =====================================================================
        // STAGE 5: NATIONAL & CATEGORY MODIFIERS
        // Evaluate active modifiers targeting specific categories or broad scope.
        // NOTE: Simple additive application is a temporary Step 4D baseline implementation.
        // Advanced modifier stacking, percentages, diminishing returns, and curves remain deferred.
        // =====================================================================
        repuRevenue = ApplyModifiers(repuRevenue, NationalYieldCategory.RepuTreasuryRevenue, sortedModifiers);
        industrial = ApplyModifiers(industrial, NationalYieldCategory.IndustrialCapacity, sortedModifiers);
        energy = ApplyModifiers(energy, NationalYieldCategory.EnergyCapacity, sortedModifiers);
        infrastructure = ApplyModifiers(infrastructure, NationalYieldCategory.InfrastructureCapacity, sortedModifiers);
        financial = ApplyModifiers(financial, NationalYieldCategory.FinancialCapacity, sortedModifiers);

        humanCapital = ApplyModifiers(humanCapital, NationalYieldCategory.HumanCapital, sortedModifiers);
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
        // Allocate a new NationalYield instance containing all 14 canonical categories.
        // =====================================================================
        return new NationalYield
        {
            RepuTreasuryRevenue = Math.Max(0.0, repuRevenue),
            IndustrialCapacity = Math.Max(0.0, industrial),
            EnergyCapacity = Math.Max(0.0, energy),
            InfrastructureCapacity = Math.Max(0.0, infrastructure),
            FinancialCapacity = Math.Max(0.0, financial),

            HumanCapital = Math.Max(0.0, humanCapital),
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
