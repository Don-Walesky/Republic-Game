# Time System Specification

**Feature**: Time System  
**Epic**: EPIC-01 Foundation  
**Sprint**: Sprint 1  
**Status**: Design  
**Priority**: P0 (Critical)

---

## Overview

The Time System provides deterministic, fixed-rate time stepping for the simulation engine. It enables consistent, reproducible simulation behavior independent of real-world execution time. The system supports time control (pause/resume), time scaling, and a game calendar with day/month/year tracking.

---

## Objectives

- Provide deterministic simulation ticks at configurable fixed rate
- Enable time control (pause, resume, time scaling)
- Implement game calendar (day/month/year)
- Emit time-based events for other systems
- Support serialization for save/load functionality
- Require no UI or platform dependencies

---

## Acceptance Criteria

- [ ] Simulation ticks at fixed, configurable rate (default 60 ticks/second)
- [ ] Tick rate is deterministic (same inputs = same outputs)
- [ ] `Pause()` stops simulation advancement
- [ ] `Resume()` resumes simulation
- [ ] `SetTimeScale(float)` changes simulation speed (1.0 = normal)
- [ ] Game calendar tracks day, month, year
- [ ] `OnSimulationTickEvent` emitted every tick
- [ ] `OnSimulationDayEvent`, `OnSimulationMonthEvent`, `OnSimulationYearEvent` emitted on calendar transitions
- [ ] Time state is fully serializable (JSON/binary)
- [ ] No UI dependencies
- [ ] >90% unit test coverage
- [ ] All XML documentation complete

---

## Architecture

### Core Components

```
TimeSystem (main coordinator)
├── TickCounter (tracks ticks)
├── GameCalendar (day/month/year)
└── TimeScaleController (pause, scale)
```

### Dependencies

```
TimeSystem
├── EventBus (emits time events)
├── Logger (debug logging)
└── Configuration (default tick rate)
```

### Interfaces

```csharp
public interface ITimeSystem
{
    // Time control
    void Pause();
    void Resume();
    bool IsPaused { get; }
    
    // Time scaling
    void SetTimeScale(float scale);
    float TimeScale { get; }
    
    // Time queries
    ulong CurrentTick { get; }
    TimeSpan ElapsedTime { get; }
    float DeltaTime { get; } // in seconds
    
    // Calendar
    GameDate CurrentDate { get; }
    
    // Advancement
    void Tick(float realDeltaTime);
    
    // Serialization
    TimeSystemState Serialize();
    void Deserialize(TimeSystemState state);
}

public class GameDate
{
    public int Day { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}

public class TimeSystemState
{
    public ulong TickCount { get; set; }
    public GameDate CurrentDate { get; set; }
    public float TimeScale { get; set; }
    public bool IsPaused { get; set; }
}
```

---

## Detailed Specification

### 1. Fixed Time Stepping

**Requirement**: Simulation ticks at a fixed, configurable rate independent of real-world execution time.

**Implementation**:
```csharp
private float tickRate = 60f; // ticks per second
private float fixedDeltaTime = 1f / 60f; // ~0.0167 seconds

public void SetFixedTickRate(float ticksPerSecond)
{
    tickRate = ticksPerSecond;
    fixedDeltaTime = 1f / ticksPerSecond;
}

public void Tick(float realDeltaTime)
{
    if (IsPaused) return;
    
    accumulatedTime += realDeltaTime * timeScale;
    
    while (accumulatedTime >= fixedDeltaTime)
    {
        SimulationTick();
        accumulatedTime -= fixedDeltaTime;
    }
}
```

**Test Cases**:
- [ ] Tick rate can be set to 30, 60, 120 ticks/second
- [ ] Fixed delta time matches tick rate
- [ ] Ticks are consistent (1000 ticks = 1000/tickRate seconds)
- [ ] No accumulation errors over long periods

---

### 2. Time Control

**Requirement**: Support pause and resume functionality without losing state.

**Implementation**:
```csharp
private bool isPaused = false;

public void Pause()
{
    isPaused = true;
    logger.LogDebug("Time system paused");
    eventBus.Publish(new OnSimulationPausedEvent());
}

public void Resume()
{
    isPaused = false;
    logger.LogDebug("Time system resumed");
    eventBus.Publish(new OnSimulationResumedEvent());
}

public bool IsPaused => isPaused;
```

**Test Cases**:
- [ ] Pausing stops tick advancement
- [ ] Resuming continues tick advancement
- [ ] Calendar doesn't advance while paused
- [ ] Time scale is preserved after pause/resume

---

### 3. Time Scaling

**Requirement**: Allow simulation speed adjustment (0.5x, 1.0x, 2.0x, etc).

**Implementation**:
```csharp
private float timeScale = 1.0f;

public void SetTimeScale(float scale)
{
    if (scale < 0f)
    {
        logger.LogWarning("Time scale cannot be negative, clamping to 0");
        scale = 0f;
    }
    
    timeScale = scale;
    logger.LogDebug($"Time scale set to {scale}x");
    eventBus.Publish(new OnTimeScaleChangedEvent { Scale = scale });
}

public float TimeScale => timeScale;
```

**Test Cases**:
- [ ] 1.0x = normal speed (60 ticks/second real time)
- [ ] 2.0x = double speed (120 ticks/second real time)
- [ ] 0.5x = half speed (30 ticks/second real time)
- [ ] 0.0x = no advancement (effectively paused)
- [ ] Negative scales are rejected or clamped

---

### 4. Game Calendar

**Requirement**: Track in-game time with day, month, year.

**Implementation**:
```csharp
public class GameCalendar
{
    public int Day { get; set; } = 1;
    public int Month { get; set; } = 1;
    public int Year { get; set; } = 1;
    
    public int DaysPerMonth { get; set; } = 30;
    public int MonthsPerYear { get; set; } = 12;
    
    public void AdvanceDay()
    {
        Day++;
        
        if (Day > DaysPerMonth)
        {
            Day = 1;
            AdvanceMonth();
        }
    }
    
    private void AdvanceMonth()
    {
        Month++;
        
        if (Month > MonthsPerYear)
        {
            Month = 1;
            AdvanceYear();
        }
    }
    
    private void AdvanceYear()
    {
        Year++;
    }
}
```

**Configuration**:
- Days per month: Configurable (default 30)
- Months per year: Configurable (default 12)
- Calendar can be reset to arbitrary date

**Test Cases**:
- [ ] Day advances correctly (1 to 30, wraps to 1)
- [ ] Month advances on day overflow
- [ ] Year advances on month overflow
- [ ] Custom calendar settings work
- [ ] Calendar state is persisted correctly

---

### 5. Event Emissions

**Requirement**: Emit events for time-based triggers that other systems can listen to.

**Events**:

```csharp
public class OnSimulationTickEvent : IEvent
{
    public ulong TickNumber { get; set; }
    public float DeltaTime { get; set; }
    public GameDate CurrentDate { get; set; }
}

public class OnSimulationSecondEvent : IEvent
{
    public ulong TickNumber { get; set; }
    public int ElapsedSeconds { get; set; }
}

public class OnSimulationDayEvent : IEvent
{
    public GameDate NewDate { get; set; }
    public int DaysPassed { get; set; }
}

public class OnSimulationMonthEvent : IEvent
{
    public GameDate NewDate { get; set; }
    public int MonthsPassed { get; set; }
}

public class OnSimulationYearEvent : IEvent
{
    public GameDate NewDate { get; set; }
    public int YearsPassed { get; set; }
}
```

**Emission Rules**:
- `OnSimulationTickEvent` emitted on every simulation tick
- `OnSimulationSecondEvent` emitted every N ticks (where N = tickRate)
- `OnSimulationDayEvent` emitted when day changes
- `OnSimulationMonthEvent` emitted when month changes
- `OnSimulationYearEvent` emitted when year changes

**Test Cases**:
- [ ] OnSimulationTickEvent fires every tick
- [ ] OnSimulationSecondEvent fires every second
- [ ] OnSimulationDayEvent fires when day changes
- [ ] Event data is accurate
- [ ] Events include necessary context

---

### 6. Serialization

**Requirement**: Save and restore time system state without losing precision.

**Implementation**:
```csharp
public class TimeSystemState
{
    public ulong TickCount { get; set; }
    public int Day { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public float TimeScale { get; set; }
    public bool IsPaused { get; set; }
    public float AccumulatedTime { get; set; }
}

public TimeSystemState Serialize()
{
    return new TimeSystemState
    {
        TickCount = currentTick,
        Day = calendar.Day,
        Month = calendar.Month,
        Year = calendar.Year,
        TimeScale = timeScale,
        IsPaused = isPaused,
        AccumulatedTime = accumulatedTime
    };
}

public void Deserialize(TimeSystemState state)
{
    currentTick = state.TickCount;
    calendar.Day = state.Day;
    calendar.Month = state.Month;
    calendar.Year = state.Year;
    timeScale = state.TimeScale;
    isPaused = state.IsPaused;
    accumulatedTime = state.AccumulatedTime;
    
    logger.LogInfo($"Time system restored to tick {currentTick}, {state.Year}/{state.Month}/{state.Day}");
}
```

**Test Cases**:
- [ ] Serialization produces valid state object
- [ ] Deserialization restores exact state
- [ ] JSON serialization works
- [ ] Binary serialization works
- [ ] No precision loss in large numbers

---

### 7. No UI Dependency

**Requirement**: Time System must be completely independent of UI frameworks.

**Constraints**:
- No Unity MonoBehaviour references
- No WPF/WinForms references
- Pure C# implementation
- Can run in headless mode

**Verification**:
- [ ] No UI framework imports
- [ ] Works in console application
- [ ] Works in headless simulation
- [ ] No graphics or rendering calls

---

## Usage Examples

### Basic Time Stepping

```csharp
ITimeSystem timeSystem = container.Resolve<ITimeSystem>();
timeSystem.SetFixedTickRate(60); // 60 ticks/second

while (gameRunning)
{
    float realDeltaTime = GetRealDeltaTime();
    timeSystem.Tick(realDeltaTime);
}
```

### Listening for Time Events

```csharp
eventBus.Subscribe<OnSimulationDayEvent>(e => 
{
    Console.WriteLine($"Day changed to {e.NewDate.Day}/{e.NewDate.Month}/{e.NewDate.Year}");
});

eventBus.Subscribe<OnSimulationTickEvent>(e => 
{
    // Update simulation logic every tick
    UpdateSimulation(e.DeltaTime);
});
```

### Time Control

```csharp
// Speed up
timeSystem.SetTimeScale(2.0f);

// Pause
timeSystem.Pause();

// Resume at normal speed
timeSystem.SetTimeScale(1.0f);
timeSystem.Resume();
```

### Save/Load

```csharp
// Save
TimeSystemState state = timeSystem.Serialize();
string json = JsonConvert.SerializeObject(state);
File.WriteAllText("save.json", json);

// Load
string json = File.ReadAllText("save.json");
TimeSystemState state = JsonConvert.DeserializeObject<TimeSystemState>(json);
timeSystem.Deserialize(state);
```

---

## Testing Strategy

### Unit Tests

**Test Coverage**: >90%

**Test Categories**:
1. Fixed time stepping accuracy
2. Pause/resume functionality
3. Time scaling behavior
4. Calendar advancement
5. Event emission
6. Serialization/deserialization
7. Edge cases (overflow, negative values, etc)

**Example Tests**:
```csharp
[Test]
public void Tick_AdvancesTimeCorrectly()
{
    timeSystem.Tick(0.0167f); // 1 frame at 60fps
    Assert.AreEqual(1, timeSystem.CurrentTick);
}

[Test]
public void Pause_StopsTimeAdvancement()
{
    timeSystem.Pause();
    timeSystem.Tick(0.0167f);
    Assert.AreEqual(0, timeSystem.CurrentTick);
}

[Test]
public void TimeScale_DoublesAdvancement()
{
    timeSystem.SetTimeScale(2.0f);
    timeSystem.Tick(0.0167f);
    Assert.AreEqual(2, timeSystem.CurrentTick);
}

[Test]
public void Calendar_AdvancesCorrectly()
{
    for (int i = 0; i < 30; i++)
    {
        calendar.AdvanceDay();
    }
    Assert.AreEqual(1, calendar.Day);
    Assert.AreEqual(2, calendar.Month);
}
```

### Integration Tests

- TimeSystem with EventBus
- TimeSystem with Logger
- TimeSystem serialization with SaveSystem (in Sprint 2)

---

## Performance Considerations

- **Tick rate**: 60 ticks/second is standard; configurable for faster/slower sims
- **Event overhead**: Events should be minimal (<0.1ms per tick)
- **Memory**: TimeSystemState is small (<1KB)
- **No allocations**: Minimize GC allocations in Tick() method

---

## Dependencies

- **EventBus** (for event publishing)
- **Logger** (for debugging)
- **Configuration** (for default tick rate)
- **JSON serializer** (for save/load)

---

## Files to Create

```
/Source/Core/Time/
├── ITimeSystem.cs
├── TimeSystem.cs
├── GameCalendar.cs
├── TimeSystemState.cs
├── TimeSystemEvents.cs
└── TimeSystemConfiguration.cs

/Source/Tests/Core/Time/
├── TimeSystemTests.cs
├── GameCalendarTests.cs
└── TimeSystemSerializationTests.cs
```

---

## Deliverables

- [ ] Full implementation of TimeSystem
- [ ] Full implementation of GameCalendar
- [ ] All event types defined
- [ ] Unit tests (>90% coverage)
- [ ] XML documentation
- [ ] Integration with EventBus
- [ ] Integration with Logger
- [ ] Usage examples
- [ ] Code review approved

---

## Definition of Done

- All acceptance criteria met
- All unit tests passing
- All XML documentation complete
- Code review approved
- No compiler warnings
- Builds successfully
- Ready for integration with other Sprint 1 systems

---

*Time System Specification - Version 1.0*
