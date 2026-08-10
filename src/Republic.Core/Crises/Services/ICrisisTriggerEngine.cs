namespace Republic.Core.Crises.Services;

/// <summary>
/// Service interface evaluating simulation thresholds to generate emergent crises and executive prompts.
/// </summary>
public interface ICrisisTriggerEngine
{
    int EvaluateSimulationMetrics(ulong currentTick);
}
