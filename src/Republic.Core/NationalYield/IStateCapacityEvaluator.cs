namespace Republic.Core.NationalYield;

using Republic.Core.World.Models;

/// <summary>
/// Interface for pure evaluation of sovereign State Capacity.
/// Determines the conversion rate from national conditions and institutional capacities into realized results.
/// </summary>
public interface IStateCapacityEvaluator
{
    /// <summary>
    /// Purely calculates a <see cref="StateCapacitySnapshot"/> for a sovereign country from its current in-memory state.
    /// Does not call real-time APIs (never calls DateTime.Now).
    /// </summary>
    /// <param name="country">The sovereign country whose State Capacity will be evaluated.</param>
    /// <returns>An immutable <see cref="StateCapacitySnapshot"/>.</returns>
    StateCapacitySnapshot Evaluate(Country country);

    /// <summary>
    /// Purely calculates a <see cref="StateCapacitySnapshot"/> from explicit institutional inputs.
    /// Does not call real-time APIs (never calls DateTime.Now).
    /// </summary>
    /// <param name="inputs">The seven institutional inputs. Null represents missing inputs and evaluates to score 0.0.</param>
    /// <param name="countryId">The optional country identifier.</param>
    /// <returns>An immutable <see cref="StateCapacitySnapshot"/>.</returns>
    StateCapacitySnapshot Evaluate(StateCapacityInputs? inputs, string countryId = "");
}
