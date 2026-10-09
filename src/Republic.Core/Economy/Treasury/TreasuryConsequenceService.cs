namespace Republic.Core.Economy.Treasury;

using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Service implementation for evaluating and applying systemic consequences resulting from sovereign treasury state at cycle boundaries.
/// Delegates evaluation to <see cref="TreasuryConsequence.Apply(Country, RepublicTime)"/>.
/// </summary>
public sealed class TreasuryConsequenceService : ITreasuryConsequenceService
{
    /// <inheritdoc />
    public TreasuryConsequence? Apply(Country country, RepublicTime boundary)
    {
        return TreasuryConsequence.Apply(country, boundary);
    }
}
