namespace Republic.Core.NationalYield;

using Republic.Core.World.Models;

/// <summary>
/// Pure, deterministic evaluator for sovereign State Capacity.
/// Implements the authoritative domain rules:
/// - One score from 0.0 to 1.0 determined strictly as the minimum of the seven institutional inputs (bottleneck principle).
/// - The weakest institution is the bottleneck. Does not average them.
/// - Missing input evaluates to 0.0, not 1.0.
/// - Isolated per sovereign country: Country B does not use Country A's score.
/// - Evaluates purely from country in-memory state; never calls DateTime.Now.
/// </summary>
public sealed class StateCapacityEvaluator : IStateCapacityEvaluator
{
    /// <inheritdoc />
    public StateCapacitySnapshot Evaluate(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);
        return Evaluate(country.StateCapacity, country.Id);
    }

    /// <inheritdoc />
    public StateCapacitySnapshot Evaluate(StateCapacityInputs? inputs, string countryId = "")
    {
        if (inputs == null)
        {
            return new StateCapacitySnapshot(
                score: 0.0,
                governmentEffectiveness: 0.0,
                administrativeCompetence: 0.0,
                institutionQuality: 0.0,
                corruptionControl: 0.0,
                stability: 0.0,
                infrastructureCondition: 0.0,
                civilServiceQuality: 0.0,
                bottleneckFactor: "Missing inputs",
                countryId: countryId);
        }

        // Missing inputs evaluate to 0.0, not 1.0. Valid inputs are clamped to [0.0, 1.0].
        var gov = inputs.GovernmentEffectiveness.HasValue ? Math.Clamp(inputs.GovernmentEffectiveness.Value, 0.0, 1.0) : 0.0;
        var admin = inputs.AdministrativeCompetence.HasValue ? Math.Clamp(inputs.AdministrativeCompetence.Value, 0.0, 1.0) : 0.0;
        var inst = inputs.InstitutionQuality.HasValue ? Math.Clamp(inputs.InstitutionQuality.Value, 0.0, 1.0) : 0.0;
        var corr = inputs.CorruptionControl.HasValue ? Math.Clamp(inputs.CorruptionControl.Value, 0.0, 1.0) : 0.0;
        var stab = inputs.Stability.HasValue ? Math.Clamp(inputs.Stability.Value, 0.0, 1.0) : 0.0;
        var infra = inputs.InfrastructureCondition.HasValue ? Math.Clamp(inputs.InfrastructureCondition.Value, 0.0, 1.0) : 0.0;
        var civil = inputs.CivilServiceQuality.HasValue ? Math.Clamp(inputs.CivilServiceQuality.Value, 0.0, 1.0) : 0.0;

        // The score is the minimum of the seven inputs (bottleneck principle, not average).
        var score = Math.Min(gov, Math.Min(admin, Math.Min(inst, Math.Min(corr, Math.Min(stab, Math.Min(infra, civil))))));
        score = Math.Clamp(score, 0.0, 1.0);

        var bottleneck = DetermineBottleneckFactor(gov, admin, inst, corr, stab, infra, civil, score, inputs);

        return new StateCapacitySnapshot(
            score: score,
            governmentEffectiveness: gov,
            administrativeCompetence: admin,
            institutionQuality: inst,
            corruptionControl: corr,
            stability: stab,
            infrastructureCondition: infra,
            civilServiceQuality: civil,
            bottleneckFactor: bottleneck,
            countryId: countryId);
    }

    private static string DetermineBottleneckFactor(
        double gov,
        double admin,
        double inst,
        double corr,
        double stab,
        double infra,
        double civil,
        double score,
        StateCapacityInputs inputs)
    {
        // Prioritize flagging any explicitly missing input if score is 0.0
        if (!inputs.GovernmentEffectiveness.HasValue) return "Government effectiveness (missing)";
        if (!inputs.AdministrativeCompetence.HasValue) return "Administrative competence (missing)";
        if (!inputs.InstitutionQuality.HasValue) return "Institution quality (missing)";
        if (!inputs.CorruptionControl.HasValue) return "Corruption control (missing)";
        if (!inputs.Stability.HasValue) return "Stability (missing)";
        if (!inputs.InfrastructureCondition.HasValue) return "Infrastructure condition (missing)";
        if (!inputs.CivilServiceQuality.HasValue) return "Civil service quality (missing)";

        if (gov <= score) return "Government effectiveness";
        if (admin <= score) return "Administrative competence";
        if (inst <= score) return "Institution quality";
        if (corr <= score) return "Corruption control";
        if (stab <= score) return "Stability";
        if (infra <= score) return "Infrastructure condition";
        return "Civil service quality";
    }
}
