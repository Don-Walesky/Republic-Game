namespace Republic.Core.Workspace.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Workspace.Events;
using Republic.Core.Workspace.Models;

/// <summary>
/// Service implementing visitor arrival, meeting queueing, and status management.
/// </summary>
public sealed class VisitorService : IVisitorService
{
    private readonly List<Visitor> _visitors = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly object _lock = new();

    public VisitorService(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void RegisterVisitor(Visitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        lock (_lock)
        {
            _visitors.Add(visitor);
        }

        _logger.LogInfo($"Visitor arrived: {visitor.Name} ({visitor.Title} - {visitor.Faction})");
        _eventBus.PublishAsync(new VisitorArrivedEvent(visitor, DateTimeOffset.UtcNow));
    }

    public Visitor? GetNextVisitor()
    {
        lock (_lock)
        {
            return _visitors.FirstOrDefault(v => v.Status == VisitorStatus.Waiting);
        }
    }

    public bool StartMeeting(string visitorId)
    {
        lock (_lock)
        {
            var visitor = _visitors.FirstOrDefault(v => v.Id == visitorId);
            if (visitor == null || visitor.Status != VisitorStatus.Waiting)
            {
                return false;
            }

            visitor.Status = VisitorStatus.InMeeting;
            _logger.LogInfo($"Meeting started with visitor: {visitor.Name}");
            return true;
        }
    }

    public bool DecideVisitorAudience(string visitorId, bool granted)
    {
        lock (_lock)
        {
            var visitor = _visitors.FirstOrDefault(v => v.Id == visitorId);
            if (visitor == null)
            {
                return false;
            }

            visitor.Status = granted ? VisitorStatus.InMeeting : VisitorStatus.Dismissed;
            _logger.LogInfo($"Visitor audience decided for '{visitor.Name}': {(granted ? "Granted" : "Denied")}");
            return true;
        }
    }

    public bool DismissVisitor(string visitorId)
    {
        lock (_lock)
        {
            var visitor = _visitors.FirstOrDefault(v => v.Id == visitorId);
            if (visitor == null || visitor.Status == VisitorStatus.Departed)
            {
                return false;
            }

            visitor.Status = VisitorStatus.Dismissed;
            _logger.LogInfo($"Visitor dismissed: {visitor.Name}");
            _eventBus.PublishAsync(new VisitorDepartedEvent(visitor.Id, "Dismissed", DateTimeOffset.UtcNow));
            return true;
        }
    }

    public IReadOnlyList<Visitor> GetActiveVisitors()
    {
        lock (_lock)
        {
            return _visitors.Where(v => v.Status != VisitorStatus.Departed).ToList().AsReadOnly();
        }
    }
}
