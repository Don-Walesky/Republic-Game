# Time System Specification

**Feature**: Persistent Simulation Clock & Real-World Time System  
**Epic**: EPIC-01 Foundation  
**Version**: 2.0 (Canonical Revised Republic Design)  
**Status**: Specification  
**Priority**: P0 (Critical Core Architecture)  

---

## 1. Overview & Vision

Republic operates on a **persistent, real-world aligned simulation clock**.

Unlike traditional single-player strategy games that feature fictional calendars, arbitrary year leaps, or user-toggled pause and fast-forward buttons, Republic's world is a **persistent multiplayer universe** where time advances continuously on a shared global simulation timeline.

### Core Principles
1. **Real-World Date Alignment**: The in-game calendar strictly follows the **real-world calendar**. There is no fictional calendar disconnected from real-world date progression.
2. **Official Time Zone**: The authoritative game clock is anchored to **West Africa Time (WAT / UTC+1)**.
3. **Launch Moment Epoch**: The simulation timeline begins at official launch:
   > **Republic Day 0, 00:00 WAT**
4. **Shared Global Simulation Clock**: Every nation—player-governed and AI-governed—exists concurrently within the exact same global timeline.
5. **Synchronized National Yield Cycles**: National Yield cycles are globally synchronized to the simulated game clock, ensuring fair macroeconomic progression across all nations rather than fragmented cycles tied to individual player login times.
6. **Continuous Progression & Offline Persistence**:
   > *"Revenue is continuous. Development is continuous. Politics is continuous. The player is not."*  
   The simulation never halts when an individual player disconnects. Revenue collects, development projects build, institutions run, and international events unfold continuously.

---

## 2. System Architecture & Objectives

### Objectives
- Maintain continuous, deterministic simulation tick progression anchored to real-world time.
- Provide a synchronized world clock in West Africa Time (WAT / UTC+1).
- Synchronize global National Yield cycles to the world clock.
- Track precise nation founding metadata (Independence Day, time, country age).
- Power offline catch-up and event replay for the player's National Briefing upon login.
- Provide headless, high-performance time query interfaces free of presentation or platform dependencies.

### Architectural Structure

```
GlobalSimulationClock (authoritative world timeline)
├── RealWorldTimeProvider (WAT / UTC+1 time synchronization)
├── EpochManager (tracks elapsed ticks and days since Republic Day 0, 00:00 WAT)
├── YieldCycleCoordinator (triggers synchronized National Yield cycles)
├── IndependenceTracker (records founding timestamps and calculates National Age)
└── EventBus (emits global time and cycle events)
```

---

## 3. Detailed Specifications

### 3.1 Real-World Date Alignment & West Africa Time (WAT)
- The date in Republic matches the real-world Gregorian calendar date.
- All global timestamps, scheduled diplomatic summits, yield cycles, and legislative deadlines are expressed in **West Africa Time (WAT / UTC+1)**.
- Example timestamp: `2026-10-06 14:37:00 WAT`.

### 3.2 Republic Launch Epoch (Day 0)
- Epoch baseline: `Republic Day 0, 00:00 WAT`.
- All historical simulation records, national age calculations, and treaty timelines reference the continuous elapsed time since this baseline.

### 3.3 Synchronized National Yield Cycles
- National Yield is not calculated on arbitrary ad-hoc player timers.
- Instead, the global simulation clock coordinates periodic **Yield Cycles** across all nations concurrently.
- Macroeconomic parameters, tax collections in REPU (`R`), and capacity updates occur at deterministic cycle intervals tied to the global clock.

### 3.4 Country Founding & Independence Day
When a player creates their country, the time system permanently records:
- **Independence Date** (e.g., `6 October 2026`)
- **Independence Time** (e.g., `14:37 WAT`)
- **Founding Epoch Tick** (exact simulation tick)
- **National Age**: The exact duration elapsed since the nation's Independence Day.
  - *Design Rule*: **AGE MUST NOT AUTOMATICALLY EQUAL STRENGTH.** National Age conveys constitutional maturity and historical legitimacy, but governance and State Capacity determine actual strength.

### 3.5 Founding Period Duration
- The time system tracks the active window of the nation's temporary **Founding Period** (e.g., initial founding duration from Independence Day).
- During this window, the country receives the **10x National Yield Multiplier** and temporary founding momentum buffs, after which the time system transitions the nation smoothly into standard sovereign status.

### 3.6 Continuous Offline Persistence
- Because the world operates on a shared real-world clock, a player logging off does **not** stop their nation's timeline.
- When a player returns:
  1. The time system determines the elapsed duration between the last active session and the current global WAT timestamp.
  2. The simulation engine processes all accumulated yield cycles, completed development projects, and diplomatic events that occurred during the absence.
  3. The Presidential Office compiles these events into a concise **National Briefing**.

---

## 4. Interfaces & Data Contracts

```csharp
namespace Republic.Core.Time;

/// <summary>
/// Authoritative simulation clock operating on West Africa Time (WAT / UTC+1)
/// aligned with real-world calendar dates.
/// </summary>
public interface ISimulationClock
{
    /// <summary>
    /// Current real-world aligned simulation time in West Africa Time (WAT / UTC+1).
    /// </summary>
    DateTime CurrentWatTime { get; }

    /// <summary>
    /// Current calendar date in the global simulation.
    /// </summary>
    DateOnly CurrentDate { get; }

    /// <summary>
    /// Total simulation days elapsed since Republic Day 0, 00:00 WAT.
    /// </summary>
    long DaysSinceLaunch { get; }

    /// <summary>
    /// Total deterministic ticks elapsed since Republic Day 0.
    /// </summary>
    ulong TotalSimulationTicks { get; }

    /// <summary>
    /// Timestamp when Republic Day 0 was initialized.
    /// </summary>
    DateTime LaunchEpochWat { get; }

    /// <summary>
    /// Calculates national age for a country founded at the specified Independence timestamp.
    /// </summary>
    TimeSpan CalculateNationalAge(DateTime independenceTimestampWat);

    /// <summary>
    /// Determines whether a country is within its initial temporary Founding Period.
    /// </summary>
    bool IsWithinFoundingPeriod(DateTime independenceTimestampWat, TimeSpan foundingDuration);
}

/// <summary>
/// Domain model recording immutable nation founding metadata.
/// </summary>
public sealed record IndependenceMetadata(
    string CountryId,
    DateOnly IndependenceDate,
    TimeOnly IndependenceTime,
    DateTime IndependenceTimestampWat,
    long FoundingSimulationTick,
    string FoundingGlobalConditionSummary
);
```

---

## 5. Global Time Events

The Time System emits high-performance domain events via the `IEventBus`:

1. `OnGlobalSimulationTickEvent`: Fired every deterministic simulation tick with current WAT timestamp and tick count.
2. `OnGlobalYieldCycleEvent`: Fired synchronously when a global National Yield cycle executes across all nations.
3. `OnGlobalDayTransitionEvent`: Fired at `00:00 WAT` as the real-world calendar day advances.
4. `OnCountryFoundingEvent`: Fired when a new player nation is founded and its Independence Day is stamped.
5. `OnFoundingPeriodExpiredEvent`: Fired when a nation's temporary 10x founding boost and momentum buffs conclude.

---

## 6. Testing & Determinism Strategy

- **Time Provider Abstraction**: Unit and integration tests utilize an injectable `ITimeProvider` or `ISimulationClock` mock to verify deterministic behavior without waiting for real-world hours.
- **Leap Year and Calendar Accuracy**: Validates standard real-world Gregorian calendar rules across months, leap years, and daylight-independent WAT (UTC+1).
- **Concurrency & Reconnection**: Verifies that offline periods accurately calculate all completed yield cycles, infrastructure project completions, and scheduled events upon player return.
