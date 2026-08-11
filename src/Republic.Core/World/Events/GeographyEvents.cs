namespace Republic.Core.World.Events;

using System;
using Republic.Core.Events;

/// <summary>
/// Emitted when a province's local stability rating changes.
/// </summary>
public sealed record ProvinceStabilityChangedEvent(string ProvinceId, string ProvinceName, double PreviousStability, double NewStability, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Emitted when regional infrastructure investment is applied to a province.
/// </summary>
public sealed record RegionalInfrastructureBuiltEvent(string ProvinceId, string ProvinceName, double NewInfrastructureIndex, decimal InvestmentAmount, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Emitted when a province's rebellion risk reaches critical levels (>50%).
/// </summary>
public sealed record RebellionRiskElevatedEvent(string ProvinceId, string ProvinceName, double RiskLevel, DateTimeOffset OccurredAt) : IGameEvent;
