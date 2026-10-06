namespace Republic.Core.NationalYield;

using Republic.Core.World.Models;

/// <summary>
/// Domain model representing the input contract for National Yield evaluation.
/// Encapsulates the measurable institutional, physical, human, and political factors that inform national productive output.
/// Does NOT implement mathematical yield calculations, arbitrary multipliers, or REPU revenue generation.
/// </summary>
public sealed class NationalYieldCalculationInputs
{
    /// <summary>
    /// Gets or sets the sovereign country identifier to which these calculation inputs apply.
    /// </summary>
    public string CountryId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the baseline economic productive capacity factor.
    /// </summary>
    public double EconomicCapacity { get; set; }

    /// <summary>
    /// Gets or sets the administrative competence and institutional throughput factor.
    /// </summary>
    public double GovernmentEffectiveness { get; set; }

    /// <summary>
    /// Gets or sets the physical infrastructure condition, network density, and logistics capacity factor.
    /// </summary>
    public double InfrastructureCondition { get; set; }

    /// <summary>
    /// Gets or sets the national human capital, workforce skills, and public vitality factor.
    /// </summary>
    public double HumanCapital { get; set; }

    /// <summary>
    /// Gets or sets the science, academic research, and technological capability factor.
    /// </summary>
    public double ScienceTechnology { get; set; }

    /// <summary>
    /// Gets or sets the political stability and societal order factor.
    /// Composes with or references sovereign baseline stability.
    /// </summary>
    public double PoliticalStability { get; set; }

    /// <summary>
    /// Gets or sets the internal security, rule of law, and territorial defense factor.
    /// </summary>
    public double SecurityLevel { get; set; }

    /// <summary>
    /// Gets or sets the executive cabinet leadership, ministerial competence, and portfolio alignment factor.
    /// </summary>
    public double CabinetEffectiveness { get; set; }

    /// <summary>
    /// Gets or sets the collection of explicitly defined national modifiers applicable to yield calculations.
    /// </summary>
    public List<NationalYieldModifier> Modifiers { get; set; } = new();

    /// <summary>
    /// Adds a defined national modifier to the inputs.
    /// </summary>
    public void AddModifier(NationalYieldModifier modifier)
    {
        ArgumentNullException.ThrowIfNull(modifier);
        Modifiers.Add(modifier);
    }

    /// <summary>
    /// Retrieves modifiers specifically targeting the given <see cref="NationalYieldCategory"/> or broadly scoped (null target).
    /// </summary>
    public IEnumerable<NationalYieldModifier> GetModifiersForCategory(NationalYieldCategory category) =>
        Modifiers.Where(m => m.TargetCategory == null || m.TargetCategory == category);

    /// <summary>
    /// Creates a calculation-input snapshot composing from an existing <see cref="Country"/> domain model.
    /// Avoids duplicating competing sources of truth by sourcing stability and capacities from authoritative country state.
    /// </summary>
    public static NationalYieldCalculationInputs FromCountry(Country country, double cabinetEffectiveness = 0.0)
    {
        ArgumentNullException.ThrowIfNull(country);

        var inputs = new NationalYieldCalculationInputs
        {
            CountryId = country.Id,
            PoliticalStability = country.BaselineStability,
            EconomicCapacity = country.Yield.IndustrialCapacity,
            GovernmentEffectiveness = country.Yield.AdministrativeCapacity,
            InfrastructureCondition = country.Yield.InfrastructureCapacity,
            HumanCapital = country.Yield.HumanCapital,
            ScienceTechnology = country.Yield.ScienceCapacity,
            SecurityLevel = country.Yield.SecurityCapacity,
            CabinetEffectiveness = cabinetEffectiveness
        };

        foreach (var trait in country.NationalTraits)
        {
            inputs.AddModifier(new NationalYieldModifier
            {
                Id = $"trait_{trait.ToLowerInvariant().Replace(' ', '_')}",
                Name = trait,
                Source = "NationalTrait",
                Description = $"Inherent national trait of {country.Name}"
            });
        }

        return inputs;
    }

    /// <summary>
    /// Creates an independent deep clone of the calculation inputs.
    /// </summary>
    public NationalYieldCalculationInputs Clone() => new()
    {
        CountryId = CountryId,
        EconomicCapacity = EconomicCapacity,
        GovernmentEffectiveness = GovernmentEffectiveness,
        InfrastructureCondition = InfrastructureCondition,
        HumanCapital = HumanCapital,
        ScienceTechnology = ScienceTechnology,
        PoliticalStability = PoliticalStability,
        SecurityLevel = SecurityLevel,
        CabinetEffectiveness = CabinetEffectiveness,
        Modifiers = Modifiers.Select(m => m.Clone()).ToList()
    };
}
