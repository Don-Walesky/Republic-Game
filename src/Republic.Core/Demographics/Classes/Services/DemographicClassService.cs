namespace Republic.Core.Demographics.Classes.Services;

using Republic.Core.Demographics.Classes.Models;
using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.World;

/// <summary>
/// Service implementation tracking approvals across working class, oligarchs, military staff, intellectuals, and rural voters.
/// </summary>
public sealed class DemographicClassService : IDemographicClassService
{
    private readonly List<ClassApproval> _approvals = new();
    private readonly IWorldManager _worldManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public DemographicClassService(IWorldManager worldManager, IEventBus eventBus, ILogger? logger = null)
    {
        _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger;

        InitializeClasses();
    }

    private void InitializeClasses()
    {
        foreach (DemographicClass classType in Enum.GetValues(typeof(DemographicClass)))
        {
            _approvals.Add(new ClassApproval
            {
                ClassType = classType,
                ApprovalRating = 75.0,
                InfluenceWeight = 0.2
            });
        }
    }

    public IReadOnlyList<ClassApproval> GetClassApprovals()
    {
        lock (_lock)
        {
            return _approvals.ToList().AsReadOnly();
        }
    }

    public void AdjustClassApproval(DemographicClass classType, double delta)
    {
        lock (_lock)
        {
            var target = _approvals.FirstOrDefault(a => a.ClassType == classType);
            if (target != null)
            {
                target.ApprovalRating = Math.Clamp(target.ApprovalRating + delta, 0.0, 100.0);
                _logger?.LogInfo($"Class Approval adjusted [{classType}]: {target.ApprovalRating:0.0}% ({delta:+#;-#;0})");
            }
        }
    }

    public double GetWeightedOverallApproval()
    {
        lock (_lock)
        {
            return _approvals.Sum(a => a.ApprovalRating * a.InfluenceWeight);
        }
    }
}
