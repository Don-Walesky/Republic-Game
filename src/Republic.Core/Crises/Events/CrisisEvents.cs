namespace Republic.Core.Crises.Events;

using Republic.Core.Crises.Models;
using Republic.Core.Events;

public sealed record FiscalCrisisEvent(double TreasuryBalance, double InflationRate, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record CivilUnrestEvent(string RegionOrCountryId, double Stability, double Happiness, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record HyperinflationWarningEvent(double InflationRate, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record NaturalDisasterOccurredEvent(string DisasterType, string RegionId, CrisisSeverity Severity, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record InsurgencyBeganEvent(string FactionOrRegionId, CrisisSeverity Severity, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record CoupThreatTriggeredEvent(string CountryId, double MilitaryApproval, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record InterPlayerAttackLaunchedEvent(string AttackerId, string TargetId, string AttackType, CrisisSeverity Severity, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record SupplyChainDisruptedEvent(string ResourceType, string ImpactedRegionId, DateTimeOffset OccurredAt) : IGameEvent;
