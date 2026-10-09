namespace Republic.Core.Economy.Treasury;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Service interface for evaluating and applying systemic consequences resulting from sovereign treasury state at cycle boundaries.
/// </summary>
public interface ITreasuryConsequenceService
{
    /// <summary>
    /// Evaluates and applies the low-treasury approval consequence to the sovereign country at a cycle boundary.
    /// If the treasury balance is below R100,000,000, public approval falls by 2 points (clamped at 0).
    /// Executes at most once per country per boundary.
    /// </summary>
    TreasuryConsequence? Apply(Country country, RepublicTime boundary);
}
