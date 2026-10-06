# ADR-0002: Authoritative Global Republic Simulation Clock (WAT / UTC+1)

## Status
Accepted

## Context
Republic operates in a persistent, shared multiplayer world where time advances continuously across all player-governed nations. The simulation calendar follows the real-world Gregorian calendar anchored to **West Africa Time (WAT / UTC+1)** starting at **Republic Day 0, 00:00 WAT**.

Future systems—such as National Yield cycles, infrastructure project construction, cabinet tenures, elections, military operations, and offline catch-up calculations—depend fundamentally on a reliable, deterministic time abstraction.

Previous code relied on scattered tick counters and arbitrary `DateTime` conversions without an authoritative model of Republic simulation days, calendar dates, hours, and timezone boundaries. Furthermore:
- The clock must be independent of host machine timezones (e.g., developer machines, servers).
- West Africa Time (WAT) does not observe Daylight Saving Time (DST); the clock must maintain a strictly invariant UTC+1 offset year-round.
- Simulation tests must run deterministically and instantaneously without waiting for real wall-clock time to pass.

## Decision
1. **Authoritative Simulation Clock Abstraction**:
   - `RepublicTime` is introduced as a high-performance, immutable value struct in `Republic.Core.Time`. It encapsulates the Gregorian calendar date (`DateOnly`), time of day (`TimeOnly`), day number from launch (`DayNumber`), and elapsed simulation duration (`ElapsedDuration`), all strictly evaluated in WAT (+01:00).
   - `IRepublicClock` defines the authoritative simulation clock interface, exposing `CurrentTime`, `LaunchEpoch`, and conversion methods (`ToRepublicTime`, `FromElapsed`, `FromDayAndTime`).
   - `IControlledRepublicClock` defines controlled advancement capabilities for deterministic testing, offline catch-up, and scenario simulation.

2. **Strict WAT (UTC+1) Representation**:
   - The time system uses a fixed offset `WatOffset = TimeSpan.FromHours(1)`.
   - UTC and arbitrary offset timestamps are normalized to WAT via `DateTimeOffset.ToOffset(WatOffset)` without referencing local machine timezone settings or DST rules.

3. **Configurable Launch Epoch**:
   - `RepublicClockConfiguration` manages the launch date (`LaunchDate`), computing the launch epoch at Day 0, 00:00 WAT (+01:00).
   - Defaults to the repository's existing configuration placeholder (`2027-02-10`) until an official public launch date is designated.

4. **Deterministic Testing & Production TimeProvider**:
   - In simulation and unit test environments, `RepublicClock.CreateControlled` provides step-by-step time control (`AdvanceDays`, `AdvanceHours`, `AdvanceMinutes`, `Reset`, etc.) without `Thread.Sleep`.
   - In live server environments, `RepublicClock.CreateRealTime` derives authoritative simulation time relative to the launch epoch using .NET's `TimeProvider`.

5. **Engine Integration**:
   - `ITimeSystem` and `TimeSystem` expose `Clock` (`IRepublicClock`) and `CurrentRepublicTime` (`RepublicTime`), maintaining seamless synchronization with the engine's tick loop while preserving backward compatibility with legacy `EpochStartDate` properties.
   - `ApplicationBootstrapper` registers `RepublicClockConfiguration`, `RepublicClock`, `IRepublicClock`, and `IControlledRepublicClock` in the dependency injection container.

## Consequences
- All gameplay systems can query "What is the current Republic time?" without coupling to presentation technologies, network clients, or host machine configurations.
- Boundary conditions (Day 0 00:00, 23:59 -> 00:00 midnight transitions, leap years, DST transition dates) are verified by automated xUnit tests.
- Systems developed in subsequent steps (National Yield, project timers, elections, offline progression) build on this deterministic foundation.
