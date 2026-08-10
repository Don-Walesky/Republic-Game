namespace Republic.Core.Diplomacy.Events;

using Republic.Core.Diplomacy.Models;
using Republic.Core.Events;

public sealed record TreatyProposedEvent(DiplomaticTreaty Treaty, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record TreatySignedEvent(DiplomaticTreaty Treaty, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record TreatyViolatedEvent(DiplomaticTreaty Treaty, string ViolatorCountryId, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record DiplomaticStatusChangedEvent(string CountryA, string CountryB, DiplomaticStatus NewStatus, DateTimeOffset OccurredAt) : IGameEvent;
