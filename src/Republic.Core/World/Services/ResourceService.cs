namespace Republic.Core.World.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.World.Events;
using Republic.Core.World.Models;

/// <summary>
/// Service implementation for natural resource extraction and node management.
/// </summary>
public sealed class ResourceService : IResourceService
{
    private readonly List<ResourceNode> _nodes = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public ResourceService(IEventBus eventBus, ILogger? logger = null)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger;
    }

    public ResourceNode RegisterNode(ResourceNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        lock (_lock)
        {
            _nodes.Add(node);
        }

        _logger?.LogInfo($"Resource node registered: '{node.ResourceType}' (Abundance: {node.Abundance})");
        return node;
    }

    public ResourceNode? GetNode(string nodeId)
    {
        lock (_lock)
        {
            return _nodes.FirstOrDefault(n => n.Id == nodeId);
        }
    }

    public IReadOnlyList<ResourceNode> GetNodesForRegion(string regionId)
    {
        lock (_lock)
        {
            return _nodes.Where(n => n.LocationRegionId == regionId).ToList().AsReadOnly();
        }
    }

    public IReadOnlyList<ResourceNode> GetAllNodes()
    {
        lock (_lock)
        {
            return _nodes.ToList().AsReadOnly();
        }
    }

    public double ExtractResource(string nodeId, double requestedAmount)
    {
        lock (_lock)
        {
            var node = _nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node == null || node.Abundance <= 0)
            {
                return 0.0;
            }

            var actualExtracted = Math.Min(requestedAmount, node.Abundance);
            if (!node.IsRenewable)
            {
                node.Abundance -= actualExtracted;
            }

            _logger?.LogInfo($"Extracted {actualExtracted:0.0} units of {node.ResourceType} (Remaining: {node.Abundance:0.0})");
            _eventBus.PublishAsync(new ResourceExtractedEvent(node.Id, actualExtracted, DateTimeOffset.UtcNow));
            return actualExtracted;
        }
    }
}
