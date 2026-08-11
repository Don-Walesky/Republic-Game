namespace Republic.Core.Military.Events;

using System;
using Republic.Core.Events;
using Republic.Core.Military.Models;

/// <summary>
/// Emitted when the republic's DEFCON threat alert level changes.
/// </summary>
public sealed record DefconLevelChangedEvent(DefconLevel PreviousLevel, DefconLevel NewLevel, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Emitted when military personnel are recruited for a specific branch.
/// </summary>
public sealed record TroopsRecruitedEvent(MilitaryBranch Branch, int Count, decimal TotalCost, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Emitted when military equipment or ordnance is procured.
/// </summary>
public sealed record EquipmentProcuredEvent(MilitaryBranch Branch, int Units, decimal TotalCost, DateTimeOffset OccurredAt) : IGameEvent;

/// <summary>
/// Emitted when a military operation or strategic directive is executed.
/// </summary>
public sealed record MilitaryDirectiveExecutedEvent(MilitaryDirectiveResult Result, DateTimeOffset OccurredAt) : IGameEvent;
