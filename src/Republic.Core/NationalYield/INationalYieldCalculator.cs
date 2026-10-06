namespace Republic.Core.NationalYield;

/// <summary>
/// Service interface for the pure, deterministic National Yield calculation engine.
/// Evaluates sovereign calculation inputs and produces an evaluated <see cref="NationalYield"/> result.
/// </summary>
public interface INationalYieldCalculator
{
    /// <summary>
    /// Deterministically calculates National Yield from the supplied input contract.
    /// Pure calculation operation: does not mutate the input object, country, or world state.
    /// </summary>
    /// <param name="inputs">The calculation inputs containing rated national factors and active modifiers.</param>
    /// <returns>A newly allocated <see cref="NationalYield"/> containing all 14 canonical categories.</returns>
    NationalYield Calculate(NationalYieldCalculationInputs inputs);
}
