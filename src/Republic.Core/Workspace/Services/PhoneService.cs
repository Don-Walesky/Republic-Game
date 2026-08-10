namespace Republic.Core.Workspace.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Workspace.Events;
using Republic.Core.Workspace.Models;

/// <summary>
/// Service implementing phone call processing, active call tracking, and history logging.
/// </summary>
public sealed class PhoneService : IPhoneService
{
    private readonly List<PhoneCall> _calls = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly object _lock = new();

    public PhoneService(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void ReceiveCall(PhoneCall call)
    {
        ArgumentNullException.ThrowIfNull(call);
        lock (_lock)
        {
            _calls.Add(call);
        }

        _logger.LogInfo($"Incoming phone call from {call.CallerName} ({call.Organization}) - Urgency: {call.Urgency}");
        _eventBus.PublishAsync(new PhoneCallReceivedEvent(call, DateTimeOffset.UtcNow));
    }

    public PhoneCall? GetActiveCall()
    {
        lock (_lock)
        {
            return _calls.FirstOrDefault(c => c.IsActive && !c.IsAnswered);
        }
    }

    public bool AnswerCall(string callId)
    {
        lock (_lock)
        {
            var call = _calls.FirstOrDefault(c => c.Id == callId);
            if (call == null || !call.IsActive)
            {
                return false;
            }

            call.IsAnswered = true;
            call.IsActive = false;
            _logger.LogInfo($"Answered phone call from {call.CallerName}");
            _eventBus.PublishAsync(new PhoneCallEndedEvent(call.Id, true, DateTimeOffset.UtcNow));
            return true;
        }
    }

    public bool RejectCall(string callId)
    {
        lock (_lock)
        {
            var call = _calls.FirstOrDefault(c => c.Id == callId);
            if (call == null || !call.IsActive)
            {
                return false;
            }

            call.IsAnswered = false;
            call.IsActive = false;
            _logger.LogInfo($"Rejected phone call from {call.CallerName}");
            _eventBus.PublishAsync(new PhoneCallEndedEvent(call.Id, false, DateTimeOffset.UtcNow));
            return true;
        }
    }

    public IReadOnlyList<PhoneCall> GetCallHistory()
    {
        lock (_lock)
        {
            return _calls.ToList().AsReadOnly();
        }
    }
}
