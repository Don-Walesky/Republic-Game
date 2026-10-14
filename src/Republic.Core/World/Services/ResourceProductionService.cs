namespace Republic.Core.World.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;

/// <summary>
/// Service implementation for domestic physical resource production from associated resource nodes
/// aligned with six-hour Republic simulation clock cycle boundaries.
/// </summary>
public sealed class ResourceProductionService : IResourceProductionService
{
    private readonly INationalYieldSchedule _schedule;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceProductionService"/> class.
    /// </summary>
    public ResourceProductionService(INationalYieldSchedule? schedule = null)
    {
        _schedule = schedule ?? new NationalYieldSchedule();
    }

    /// <inheritdoc />
    public ResourceProductionResult Produce(Country country, ResourceNode node, RepublicTime boundary)
    {
        ArgumentNullException.ThrowIfNull(country);

        if (node == null)
        {
            return ResourceProductionResult.Failure(
                country.Id,
                string.Empty,
                string.Empty,
                boundary,
                "Resource node is missing.");
        }

        if (!node.IsActive)
        {
            return ResourceProductionResult.Failure(
                country.Id,
                node.Id,
                node.ResourceType,
                boundary,
                $"Resource node '{node.Id}' is inactive.");
        }

        // Check explicit association: node.CountryId matches or country.ResourceNodes contains node
        var isAssociated = (!string.IsNullOrWhiteSpace(node.CountryId) && string.Equals(node.CountryId.Trim(), country.Id.Trim(), StringComparison.OrdinalIgnoreCase))
                           || country.ResourceNodes.Any(n => n.Id == node.Id);

        if (!isAssociated)
        {
            return ResourceProductionResult.Failure(
                country.Id,
                node.Id,
                node.ResourceType,
                boundary,
                $"Resource node '{node.Id}' is not associated with country '{country.Id}'.");
        }

        // Validate cycle boundary alignment (00:00, 06:00, 12:00, 18:00 WAT)
        if (!NationalYieldSchedule.IsBoundary(boundary))
        {
            return ResourceProductionResult.Failure(
                country.Id,
                node.Id,
                node.ResourceType,
                boundary,
                $"Simulation time {boundary} does not fall on an authoritative six-hour production cycle boundary (00:00, 06:00, 12:00, 18:00 WAT).");
        }

        // Deterministic boundary key for duplicate execution protection
        var boundaryKey = $"prod-{country.Id}-{node.Id}-{boundary.DayNumber}-{boundary.Hour}";
        if (country.HasAppliedProductionBoundary(boundaryKey))
        {
            return ResourceProductionResult.Failure(
                country.Id,
                node.Id,
                node.ResourceType,
                boundary,
                $"Domestic resource production from node '{node.Id}' has already been credited for boundary {boundary}.",
                alreadyProduced: true);
        }

        // Determine extraction rate
        var rate = (double.IsFinite(node.ExtractionRate) && node.ExtractionRate > 0.0)
            ? node.ExtractionRate
            : ResourceNode.DefaultProductionRate;

        // Determine extractable amount based on abundance and renewability
        double quantityToExtract;
        if (node.IsRenewable)
        {
            quantityToExtract = rate;
        }
        else
        {
            if (node.Abundance <= 0.0)
            {
                return ResourceProductionResult.Failure(
                    country.Id,
                    node.Id,
                    node.ResourceType,
                    boundary,
                    $"Resource node '{node.Id}' abundance is completely depleted.");
            }

            quantityToExtract = Math.Min(rate, node.Abundance);
            node.Abundance -= quantityToExtract;
        }

        if (quantityToExtract <= 0.0)
        {
            return ResourceProductionResult.Failure(
                country.Id,
                node.Id,
                node.ResourceType,
                boundary,
                $"No goods could be extracted from node '{node.Id}'.");
        }

        // Deposit physical goods into sovereign inventory
        country.Inventory.Add(node.ResourceType, quantityToExtract);

        // Record boundary applied on country to prevent duplicate execution
        country.RecordProductionBoundaryApplied(boundaryKey);
        country.LastResourceProductionBoundary = boundary;

        return ResourceProductionResult.Succeeded(
            country.Id,
            node.Id,
            node.ResourceType,
            quantityToExtract,
            boundary,
            $"Successfully produced {quantityToExtract:N0} units of {node.ResourceType} from node '{node.Id}' at {boundary}.");
    }

    /// <inheritdoc />
    public IReadOnlyList<ResourceProductionResult> ProduceDueCycles(Country country, ResourceNode node, RepublicTime currentSimulationTime)
    {
        ArgumentNullException.ThrowIfNull(country);
        if (node == null)
        {
            return Array.Empty<ResourceProductionResult>();
        }

        var dueBoundaries = _schedule.GetDueBoundaries(country.LastResourceProductionBoundary, currentSimulationTime, NationalYieldSchedule.MaxBoundaryCap);
        if (dueBoundaries.Count == 0)
        {
            return Array.Empty<ResourceProductionResult>();
        }

        var results = new List<ResourceProductionResult>(dueBoundaries.Count);
        foreach (var boundary in dueBoundaries)
        {
            var res = Produce(country, node, boundary);
            results.Add(res);
        }

        return results;
    }

    /// <inheritdoc />
    public IReadOnlyList<ResourceProductionResult> ProduceDueCycles(Country country, RepublicTime currentSimulationTime)
    {
        ArgumentNullException.ThrowIfNull(country);
        var activeNodes = country.ResourceNodes.Where(n => n.IsActive).ToList();
        if (activeNodes.Count == 0)
        {
            return Array.Empty<ResourceProductionResult>();
        }

        var results = new List<ResourceProductionResult>();
        foreach (var node in activeNodes)
        {
            var nodeResults = ProduceDueCycles(country, node, currentSimulationTime);
            results.AddRange(nodeResults);
        }

        return results;
    }
}
