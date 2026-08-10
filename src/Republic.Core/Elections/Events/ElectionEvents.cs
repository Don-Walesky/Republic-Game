namespace Republic.Core.Elections.Events;

using Republic.Core.Elections.Models;
using Republic.Core.Events;

public sealed record CampaignSeasonBeganEvent(DateTimeOffset OccurredAt) : IGameEvent;

public sealed record ElectionPollingUpdatedEvent(PollingData Polling, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record ElectionConductedEvent(ElectionResult Result, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record PresidentialTransitionEvent(string WinnerName, bool IsReelection, DateTimeOffset OccurredAt) : IGameEvent;
