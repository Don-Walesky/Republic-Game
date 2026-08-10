namespace Republic.Core.Cabinet.Events;

using Republic.Core.Cabinet.Models;
using Republic.Core.Events;

public sealed record MinisterAppointedEvent(Minister Minister, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record MinisterDismissedEvent(string MinisterId, CabinetPortfolio Portfolio, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record CabinetAdviceOfferedEvent(string MinisterName, CabinetPortfolio Portfolio, string AdviceSummary, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record MinisterIntrigueUncoveredEvent(string MinisterId, string MinisterName, string PlotType, DateTimeOffset OccurredAt) : IGameEvent;
