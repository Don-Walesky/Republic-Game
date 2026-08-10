namespace Republic.Core.Crises.Events;

using Republic.Core.Events;

public sealed record FiscalCrisisEvent(double TreasuryBalance, double InflationRate, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record CivilUnrestEvent(string RegionOrCountryId, double Stability, double Happiness, DateTimeOffset OccurredAt) : IGameEvent;

public sealed record HyperinflationWarningEvent(double InflationRate, DateTimeOffset OccurredAt) : IGameEvent;
