namespace Republic.Core.World.Services;

using System.Collections.Generic;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;

/// <summary>
/// Service interface for domestic physical resource production from associated resource nodes
/// aligned with six-hour Republic simulation clock cycle boundaries.
/// </summary>
public interface IResourceProductionService
{
    /// <summary>
    /// Executes a single six-hour domestic resource production cycle for the specified country and resource node.
    /// </summary>
    /// <param name="country">The sovereign country receiving the physical goods.</param>
    /// <param name="node">The domestic resource node being extracted.</param>
    /// <param name="boundary">The authoritative six-hour cycle boundary (00:00, 06:00, 12:00, 18:00 WAT).</param>
    /// <returns>The production outcome.</returns>
    ResourceProductionResult Produce(Country country, ResourceNode node, RepublicTime boundary);

    /// <summary>
    /// Executes all due six-hour domestic resource production cycles up to the current simulation time
    /// for the specified resource node.
    /// </summary>
    IReadOnlyList<ResourceProductionResult> ProduceDueCycles(Country country, ResourceNode node, RepublicTime currentSimulationTime);

    /// <summary>
    /// Executes all due six-hour domestic resource production cycles up to the current simulation time
    /// across all active resource nodes associated with the specified country.
    /// </summary>
    IReadOnlyList<ResourceProductionResult> ProduceDueCycles(Country country, RepublicTime currentSimulationTime);
}
