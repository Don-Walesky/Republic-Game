namespace Republic.Core.NationalYield;

/// <summary>
/// Isolated, deterministic calculation engine for sovereign National Yield.
/// Evaluates <see cref="NationalYieldCalculationInputs"/> across the 6-stage pipeline (ADR-0006),
/// establishing the first working calibrated model derived from the approved relationships in ADR-0008
/// and the numerical calibration principles in ADR-0009.
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
        // Guarantee non-negativity across all incoming sovereign factors.
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
        // Establish foundational structural potential directly from relevant inputs.
        // Capacities reflect their core physical, human, and institutional substrates.
        // =====================================================================
        var human = humanCapital;
        var science = scienceTech;
        var infrastructure = infraCondition;
        var admin = govtEffectiveness;
        var security = securityLevel;
        var industrial = economicCapacity;

        // Energy capacity requires physical infrastructure to generate and distribute power
        var energy = infraCondition;

        // Financial capacity reflects commercial banking depth and credit liquidity
        var financial = economicCapacity;

        // Innovation capacity reflects commercial technology adoption, rooted in science & tech
        var innovation = scienceTech;

        // Intelligence capacity reflects strategic early warning and security apparatus
        var intelligence = securityLevel;

        // Military readiness reflects defense capability and armed forces posture
        var military = securityLevel;

        // Diplomatic capacity reflects international standing and bilateral representation
        var diplomatic = govtEffectiveness;

        // Natural resource extraction requires physical transport corridors to access and move deposits
        var naturalResource = Math.Min(economicCapacity, infraCondition);

        // =====================================================================
        // STAGE 3: CROSS-CATEGORY UPSTREAM RELATIONSHIPS
        // Propagate causal synergies and enabling throughput across categories (ADR-0008).
        // Uses bounded, monotonic combinations without arbitrary multiplier inflation.
        // =====================================================================

        // 1. Human Capital helps Science: educated talent fuels research labs
        science = (science * 2.0 + human) / 3.0;

        // 2. Science helps Innovation: research breakthroughs feed commercial technology adoption
        innovation = (science * 2.0 + human) / 3.0;

        // 3. Infrastructure & Energy support Industry: factories require logistics corridors and base-load power
        var energySupport = (energy + infrastructure) / 2.0;
        industrial = (industrial * 2.0 + energySupport) / 3.0;

        // 4. Administration supports Infrastructure: public works oversight ensures maintenance
        infrastructure = (infrastructure * 2.0 + admin) / 3.0;

        // 5. Infrastructure & Resources support Energy: grid transmission and domestic fuel feed power generation
        var energyInputs = (infrastructure + naturalResource) / 2.0;
        energy = (energy * 2.0 + energyInputs) / 3.0;

        // 6. Administration supports Human Capital: public health and education services sustain vitality
        human = (human * 2.0 + admin) / 3.0;

        // 7. Administration supports Intelligence: institutional oversight and vetting
        var adminStability = (admin + stability) / 2.0;
        intelligence = (intelligence * 2.0 + adminStability) / 3.0;

        // 8. Intelligence supports Security: threat early warning aids law enforcement
        security = (security * 2.0 + intelligence) / 3.0;

        // 9. Stability & Security support Financial Capacity: depositor confidence and rule of law
        var financialSecurity = (stability + security) / 2.0;
        financial = (financial * 2.0 + financialSecurity) / 3.0;

        // 10. Industry & Human Capital support Military Readiness: equipment fabrication and trained recruits
        var militarySupport = (industrial + human) / 2.0;
        military = (military * 2.0 + militarySupport) / 3.0;

        // 11. Institutional & Strategic strength support Diplomatic Capacity: governance legitimacy and defense deterrence
        var institutionalStrength = (admin + stability) / 2.0;
        diplomatic = (institutionalStrength * 2.0 + military) / 3.0;

        // =====================================================================
        // STAGE 4: STATE CAPACITY / GOVERNANCE CONVERSION & REPU REVENUE FLOW
        // Evaluate the country's institutional ability to convert capabilities into output.
        // Industrial strength, financial volume, and resources generate the taxable economic base,
        // while administrative effectiveness determines tax compliance and anti-leakage realization.
        // =====================================================================
        var taxableBase = (industrial + financial + naturalResource) / 3.0;
        var repuRevenue = (taxableBase + admin) / 2.0;

        // =====================================================================
        // STAGE 5: NATIONAL & CATEGORY MODIFIERS
        // Apply active modifiers targeting specific categories or broad scope additively.
        // Stably sorted by Id to eliminate iteration order variance; non-negativity clamped.
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
