namespace Republic.Core.Workspace.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Workspace.Models;

/// <summary>
/// Aggregates all workspace channels and handles top-level office state transitions.
/// </summary>
public sealed class WorkspaceManager : IWorkspaceManager
{
    private readonly OfficeRoomState _roomState = new();
    private readonly ILogger _logger;
    private readonly IEventBus _eventBus;

    public IVisitorService Visitors { get; }
    public IPhoneService Phone { get; }
    public IEmailService Email { get; }
    public INewsService News { get; }
    public ICalendarService Calendar { get; }

    public WorkspaceManager(
        IVisitorService visitorService,
        IPhoneService phoneService,
        IEmailService emailService,
        INewsService newsService,
        ICalendarService calendarService,
        IEventBus eventBus,
        ILogger logger)
    {
        Visitors = visitorService ?? throw new ArgumentNullException(nameof(visitorService));
        Phone = phoneService ?? throw new ArgumentNullException(nameof(phoneService));
        Email = emailService ?? throw new ArgumentNullException(nameof(emailService));
        News = newsService ?? throw new ArgumentNullException(nameof(newsService));
        Calendar = calendarService ?? throw new ArgumentNullException(nameof(calendarService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public WorkspaceState GetCurrentState()
    {
        return new WorkspaceState
        {
            RoomState = new OfficeRoomState
            {
                ActiveRoomName = _roomState.ActiveRoomName,
                LightingMode = _roomState.LightingMode,
                AmbientAudioZone = _roomState.AmbientAudioZone
            },
            Visitors = Visitors.GetActiveVisitors().ToList(),
            PhoneCalls = Phone.GetCallHistory().ToList(),
            Emails = Email.GetInbox().ToList(),
            NewsArticles = News.GetNewsFeed().ToList(),
            Appointments = Calendar.GetUpcomingAppointments(DateTime.MinValue).ToList(),
            LastUpdated = DateTime.UtcNow
        };
    }

    public void UpdateRoomState(string? roomName = null, string? lightingMode = null, string? audioZone = null)
    {
        if (!string.IsNullOrWhiteSpace(roomName))
        {
            _roomState.ActiveRoomName = roomName;
        }

        if (!string.IsNullOrWhiteSpace(lightingMode))
        {
            _roomState.LightingMode = lightingMode;
        }

        if (!string.IsNullOrWhiteSpace(audioZone))
        {
            _roomState.AmbientAudioZone = audioZone;
        }

        _logger.LogInfo($"Workspace environment updated: Room='{_roomState.ActiveRoomName}', Lighting='{_roomState.LightingMode}', Audio='{_roomState.AmbientAudioZone}'");
    }

    public void ProcessTimeTick(long totalTicks)
    {
        // Periodic check for environmental lighting changes based on simulated ticks
        var hourOfDay = (int)((totalTicks / 10) % 24);
        if (hourOfDay >= 6 && hourOfDay < 18 && _roomState.LightingMode != "Day")
        {
            UpdateRoomState(lightingMode: "Day");
        }
        else if (hourOfDay >= 18 && hourOfDay < 22 && _roomState.LightingMode != "Dusk")
        {
            UpdateRoomState(lightingMode: "Dusk");
        }
        else if ((hourOfDay >= 22 || hourOfDay < 6) && _roomState.LightingMode != "Night")
        {
            UpdateRoomState(lightingMode: "Night");
        }
    }
}
