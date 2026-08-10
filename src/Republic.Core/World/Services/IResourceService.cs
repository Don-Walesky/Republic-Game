namespace Republic.Core.World.Services;

using Republic.Core.World.Models;

/// <summary>
/// Service interface for natural resource nodes and extraction rates.
/// </summary>
public interface IResourceService
{
    ResourceNode RegisterNode(ResourceNode node);
    IReadOnlyList<ResourceNode> GetNodesForRegion(string regionId);
    IReadOnlyList<ResourceNode> GetAllNodes();
    double ExtractResource(string nodeId, double requestedAmount);
}
