namespace Republic.Core.NationalYield;

using Republic.Core.Time;

/// <summary>
/// Immutable value representation of temporary founding buffs active after a sovereign nation's independence.
/// Captures seven temporary national capacities (0.0 to 1.0) on the Republic simulation clock,
/// decaying from full strength (1.0) over days 0-7 down to zero at day 28.
/// </summary>
public sealed class FoundingBuffSnapshot
{
    /// <summary>
    /// Duration during which founding buffs remain at full strength (7 days).
    /// </summary>
    public const double FullStrengthDays = 7.0;

    /// <summary>
    /// Total duration until founding buffs fully taper to zero (28 days).
    /// </summary>
    public const double TotalDurationDays = 28.0;

    /// <summary>
    /// Duration of the linear decay window (21 days).
    /// </summary>
    public const double DecayWindowDays = TotalDurationDays - FullStrengthDays;

    /// <summary>
    /// Gets the temporary administrative efficiency buff factor (0.0 to 1.0).
    /// </summary>
    public double AdministrativeEfficiency { get; init; }

    /// <summary>
    /// Gets the temporary innovation drive buff factor (0.0 to 1.0).
    /// </summary>
    public double InnovationDrive { get; init; }

    /// <summary>
    /// Gets the temporary development initiative buff factor (0.0 to 1.0).
    /// </summary>
    public double DevelopmentInitiative { get; init; }

    /// <summary>
    /// Gets the temporary investor confidence buff factor (0.0 to 1.0).
    /// </summary>
    public double InvestorConfidence { get; init; }

    /// <summary>
    /// Gets the temporary diplomatic recognition momentum buff factor (0.0 to 1.0).
    /// </summary>
    public double DiplomaticRecognitionMomentum { get; init; }

    /// <summary>
    /// Gets the temporary institution-building buff factor (0.0 to 1.0).
    /// </summary>
    public double InstitutionBuilding { get; init; }

    /// <summary>
    /// Gets the temporary national unity buff factor (0.0 to 1.0).
    /// </summary>
    public double TemporaryNationalUnity { get; init; }

    /// <summary>
    /// Gets the simulation timestamp as of which this snapshot was evaluated.
    /// </summary>
    public RepublicTime AsOfTime { get; init; }

    /// <summary>
    /// Gets the administrative efficiency buff formatted as percentage.
    /// </summary>
    public string FormattedAdministrativeEfficiency => $"{AdministrativeEfficiency:P0}";

    /// <summary>
    /// Gets the innovation drive buff formatted as percentage.
    /// </summary>
    public string FormattedInnovationDrive => $"{InnovationDrive:P0}";

    /// <summary>
    /// Gets the development initiative buff formatted as percentage.
    /// </summary>
    public string FormattedDevelopmentInitiative => $"{DevelopmentInitiative:P0}";

    /// <summary>
    /// Gets the investor confidence buff formatted as percentage.
    /// </summary>
    public string FormattedInvestorConfidence => $"{InvestorConfidence:P0}";

    /// <summary>
    /// Gets the diplomatic recognition momentum buff formatted as percentage.
    /// </summary>
    public string FormattedDiplomaticRecognitionMomentum => $"{DiplomaticRecognitionMomentum:P0}";

    /// <summary>
    /// Gets the institution-building buff formatted as percentage.
    /// </summary>
    public string FormattedInstitutionBuilding => $"{InstitutionBuilding:P0}";

    /// <summary>
    /// Gets the temporary national unity buff formatted as percentage.
    /// </summary>
    public string FormattedTemporaryNationalUnity => $"{TemporaryNationalUnity:P0}";

    /// <summary>
    /// Initializes a new instance of the <see cref="FoundingBuffSnapshot"/> class with default zeros.
    /// </summary>
    public FoundingBuffSnapshot()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FoundingBuffSnapshot"/> class with explicit values.
    /// </summary>
    public FoundingBuffSnapshot(
        double administrativeEfficiency,
        double innovationDrive,
        double developmentInitiative,
        double investorConfidence,
        double diplomaticRecognitionMomentum,
        double institutionBuilding,
        double temporaryNationalUnity,
        RepublicTime asOfTime)
    {
        AdministrativeEfficiency = Math.Clamp(administrativeEfficiency, 0.0, 1.0);
        InnovationDrive = Math.Clamp(innovationDrive, 0.0, 1.0);
        DevelopmentInitiative = Math.Clamp(developmentInitiative, 0.0, 1.0);
        InvestorConfidence = Math.Clamp(investorConfidence, 0.0, 1.0);
        DiplomaticRecognitionMomentum = Math.Clamp(diplomaticRecognitionMomentum, 0.0, 1.0);
        InstitutionBuilding = Math.Clamp(institutionBuilding, 0.0, 1.0);
        TemporaryNationalUnity = Math.Clamp(temporaryNationalUnity, 0.0, 1.0);
        AsOfTime = asOfTime;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FoundingBuffSnapshot"/> class with uniform buff strength across all seven factors.
    /// </summary>
    public FoundingBuffSnapshot(double uniformStrength, RepublicTime asOfTime)
        : this(uniformStrength, uniformStrength, uniformStrength, uniformStrength, uniformStrength, uniformStrength, uniformStrength, asOfTime)
    {
    }

    /// <summary>
    /// Returns the seven founding buffs mapped by their canonical names.
    /// </summary>
    public IReadOnlyDictionary<string, double> ToDictionary() => new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
    {
        ["Administrative efficiency"] = AdministrativeEfficiency,
        ["Innovation drive"] = InnovationDrive,
        ["Development initiative"] = DevelopmentInitiative,
        ["Investor confidence"] = InvestorConfidence,
        ["Diplomatic recognition momentum"] = DiplomaticRecognitionMomentum,
        ["Institution-building"] = InstitutionBuilding,
        ["Temporary national unity"] = TemporaryNationalUnity
    };

    /// <summary>
    /// Converts the seven founding buffs into explicit <see cref="NationalYieldModifier"/> domain instances.
    /// </summary>
    public IReadOnlyList<NationalYieldModifier> ToModifiers(string countryId = "") => new List<NationalYieldModifier>
    {
        new() { Name = "Administrative efficiency", Value = AdministrativeEfficiency, Source = "FoundingBuff", Description = "Post-independence administrative focus." },
        new() { Name = "Innovation drive", Value = InnovationDrive, Source = "FoundingBuff", Description = "Founding era innovation momentum." },
        new() { Name = "Development initiative", Value = DevelopmentInitiative, Source = "FoundingBuff", Description = "Early sovereign development push." },
        new() { Name = "Investor confidence", Value = InvestorConfidence, Source = "FoundingBuff", Description = "Early post-founding sovereign investor confidence." },
        new() { Name = "Diplomatic recognition momentum", Value = DiplomaticRecognitionMomentum, Source = "FoundingBuff", Description = "International goodwill following independence." },
        new() { Name = "Institution-building", Value = InstitutionBuilding, Source = "FoundingBuff", Description = "Founding institutional establishment drive." },
        new() { Name = "Temporary national unity", Value = TemporaryNationalUnity, Source = "FoundingBuff", Description = "Post-independence patriotic national cohesion." }
    };

    /// <inheritdoc />
    public override string ToString() =>
        $"AsOf: {AsOfTime} | Admin: {FormattedAdministrativeEfficiency} | Innovation: {FormattedInnovationDrive} | Dev: {FormattedDevelopmentInitiative} | Investor: {FormattedInvestorConfidence} | Diplo: {FormattedDiplomaticRecognitionMomentum} | Institutions: {FormattedInstitutionBuilding} | Unity: {FormattedTemporaryNationalUnity}";
}
