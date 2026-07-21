# AI Implementation: TimeEngine Class Structure

## Context
**Wave**: Wave 0 (Foundation)
**Epic**: Epic: Time Engine
**Issue**: Issue 6.1 - TimeEngine class structure
**Priority**: Critical - This is the heartbeat of all Republic simulation

## Objective
Create the TimeEngine class that manages deterministic tick scheduling at 60 Hz, independent of frame rate. This system announces ticks to all other systems via the Event Bus.

## Requirements
- [ ] TimeEngine class with complete public API
- [ ] Tick scheduling at 60 Hz (1/60 second per tick)
- [ ] Time scale support (pause, 0.5x, 1.0x, 2.0x, 4.0x)
- [ ] OnTick event fired every simulation tick
- [ ] Deterministic progression (same seed = same results)
- [ ] Zero garbage allocation per tick
- [ ] Event Bus integration
- [ ] Serializable state for save/load

## Acceptance Criteria
- [ ] TimeEngine.cs compiles without warnings
- [ ] All public methods work as documented
- [ ] TimeEngine.Initialize() completes without errors
- [ ] Pause/Resume functionality works
- [ ] TimeScale changes affect tick rate proportionally
- [ ] OnTick event fires at precisely 60 Hz
- [ ] 1000-tick benchmark runs at stable 60 Hz
- [ ] Zero GC allocations in Update() loop
- [ ] Unit tests pass with 95%+ coverage
- [ ] EventBus integration verified
- [ ] Determinism test passes (same seed = same progression)
- [ ] Code review approved
- [ ] Merged to main branch

## Technical Specification

### Architecture Overview

TimeEngine is a singleton MonoBehaviour that runs in the Update loop. It accumulates frame deltaTime and fires deterministic ticks at 60 Hz regardless of actual frame rate.

**Key Pattern**:
```
Each Update:
  accumulatedTime += Time.deltaTime * timeScale
  
While accumulatedTime >= TICK_DURATION:
  OnTick?.Invoke(tickEventArgs)
  EventBus.Publish(new TickEvent())
  tickCount++
  currentTime += TICK_DURATION
  accumulatedTime -= TICK_DURATION
```

### Data Model

```csharp
public class TimeEngine : MonoBehaviour 
{
    // Constants
    private const float TICK_DURATION = 1f / 60f; // 60 Hz
    private const float MIN_TIME_SCALE = 0f;
    private const float MAX_TIME_SCALE = 4f;
    
    // State
    private float currentTime = 0f;
    private int tickCount = 0;
    private float timeScale = 1f;
    private float accumulatedDeltaTime = 0f;
    private bool isPaused = false;
    
    // Events
    public event System.Action<TickEventArgs> OnTick;
    public event System.Action OnPause;
    public event System.Action OnResume;
    public event System.Action<float> OnTimeScaleChanged;
}
```

### Public API

```csharp
public class TimeEngine : MonoBehaviour
{
    /// <summary>
    /// Initialize the TimeEngine. Call this once at startup.
    /// </summary>
    public void Initialize()
    {
        // Setup initial state
    }
    
    /// <summary>
    /// Pause time progression. Ticks will not fire.
    /// </summary>
    public void Pause()
    {
        // Set isPaused = true
        // Fire OnPause event
    }
    
    /// <summary>
    /// Resume time progression. Ticks will fire again.
    /// </summary>
    public void Resume()
    {
        // Set isPaused = false
        // Fire OnResume event
    }
    
    /// <summary>
    /// Set the time scale multiplier.
    /// 0 = paused, 1 = normal, 2 = 2x speed, etc.
    /// Clamped to [0, 4].
    /// </summary>
    public void SetTimeScale(float scale)
    {
        // Clamp to valid range
        // Set timeScale
        // Fire OnTimeScaleChanged event
    }
    
    /// <summary>
    /// Get current game time in seconds.
    /// </summary>
    public float GetCurrentTime() => currentTime;
    
    /// <summary>
    /// Get number of ticks that have occurred.
    /// </summary>
    public int GetTickCount() => tickCount;
    
    /// <summary>
    /// Get current time scale multiplier.
    /// </summary>
    public float GetTimeScale() => timeScale;
    
    /// <summary>
    /// Get whether time is paused.
    /// </summary>
    public bool IsPaused() => isPaused;
    
    /// <summary>
    /// Get the duration of one tick in seconds.
    /// </summary>
    public float GetTickDuration() => TICK_DURATION;
    
    /// <summary>
    /// Get current frame's delta time (scaled).
    /// </summary>
    public float GetDeltaTime() => Time.deltaTime * timeScale;
}

public class TickEventArgs : System.EventArgs
{
    public float DeltaTime { get; set; }      // TICK_DURATION
    public int TickCount { get; set; }        // Which tick is this
    public float CurrentTime { get; set; }    // Game time in seconds
}
```

### Integration Points

**Event Bus Integration**:
```csharp
// On each tick, publish to EventBus
EventBus.Publish(new TickEvent 
{ 
    TickCount = tickCount,
    CurrentTime = currentTime,
    DeltaTime = TICK_DURATION
});
```

**Save System Integration**:
```csharp
// Serializable state for save/load
[System.Serializable]
public class TimeEngineState
{
    public float CurrentTime;
    public int TickCount;
}
```

**Dependency Injection** (optional):
```csharp
// Can be injected as singleton
var timeEngine = serviceContainer.Resolve<TimeEngine>();
```

## Implementation Strategy

### Step 1: Create TimeEngine class structure
- Create `TimeEngine.cs` inheriting from MonoBehaviour
- Add private fields for state
- Add public properties/getters
- Add events (OnTick, OnPause, OnResume, OnTimeScaleChanged)
- Make it a singleton (Awake pattern)

### Step 2: Implement tick scheduling
- In Update():
  - Get Time.deltaTime
  - Multiply by timeScale
  - Accumulate in accumulatedDeltaTime
  - While accumulated >= TICK_DURATION:
    - Create TickEventArgs
    - Fire OnTick event
    - Publish TickEvent to EventBus
    - Increment tickCount
    - Add TICK_DURATION to currentTime
    - Subtract TICK_DURATION from accumulated

### Step 3: Implement time scale support
- SetTimeScale(float scale):
  - Clamp scale to [0, 4]
  - Set timeScale
  - Fire OnTimeScaleChanged event
  - If scale == 0, fire OnPause
  - If scale > 0 and was paused, fire OnResume

### Step 4: Implement pause/resume
- Pause():
  - Set isPaused = true
  - Set timeScale = 0
  - Fire OnPause event
- Resume():
  - Set isPaused = false
  - Set timeScale = 1 (or previous value)
  - Fire OnResume event

### Step 5: Implement serialization
- Create TimeEngineState class
- Add GetState() method
- Add SetState(TimeEngineState) method
- Ensure all state can be saved/loaded

### Step 6: Add comprehensive unit tests
- Test tick scheduling
- Test time scale
- Test pause/resume
- Test event firing
- Test no GC allocations
- Test determinism
- Test edge cases

## Testing Strategy

### Unit Tests

```csharp
[TestFixture]
public class TimeEngineTests
{
    private TimeEngine timeEngine;
    
    [SetUp]
    public void Setup()
    {
        // Create GameObject with TimeEngine
    }
    
    // Tick scheduling tests
    [Test]
    public void Test_TicksFire_AtCorrectInterval()
    {
        // Run 60 Updates
        // Verify OnTick fires ~60 times
        // Verify currentTime advances by ~1 second
    }
    
    [Test]
    public void Test_FrameRateIndependent()
    {
        // Simulate variable frame rates
        // Verify tick count is consistent
    }
    
    [Test]
    public void Test_TimeScale_AffectsTicks()
    {
        // SetTimeScale(2.0)
        // Run 30 Updates
        // Verify ~60 ticks fired (2x speed)
    }
    
    [Test]
    public void Test_Pause_StopsTicks()
    {
        // Pause()
        // Run Updates
        // Verify OnTick doesn't fire
    }
    
    [Test]
    public void Test_NoGarbageAllocation()
    {
        // Track GC allocs
        // Run 1000 ticks
        // Verify 0 allocations in Update loop
    }
    
    [Test]
    public void Test_Determinism()
    {
        // Run 100 ticks, record times
        // Reset and run again
        // Verify identical progression
    }
}
```

### Integration Tests

- TimeEngine works with EventBus
- TimeEngine can be saved/loaded
- TimeEngine works in full game loop
- Multiple systems can subscribe to OnTick

### Performance Tests

```csharp
[Test]
public void Benchmark_1000Ticks()
{
    var sw = Stopwatch.StartNew();
    for (int i = 0; i < 1000; i++)
    {
        timeEngine.Tick(); // Simulate one tick worth of Updates
    }
    sw.Stop();
    
    // Should complete in ~16.67 seconds (60 Hz)
    Assert.Less(sw.ElapsedMilliseconds, 17000);
}
```

## Files to Create

**Primary Implementation**:
- `Assets/Code/Core/Time/TimeEngine.cs` - Main TimeEngine class
  - ~200-300 lines
  - Full public API
  - Singleton pattern
  - Event firing logic

**Supporting Files**:
- `Assets/Code/Core/Time/TickEventArgs.cs` - Event args (~20 lines)
- `Assets/Code/Core/Time/TimeEngineState.cs` - Serializable state (~30 lines)
- `Assets/Code/Core/Events/TickEvent.cs` - Event type (~20 lines)

**Test Files**:
- `Assets/Tests/Core/Time/TimeEngine.Tests.cs` - Unit tests (~400-500 lines)

## Files to Modify

- `Assets/Code/Core/Events/EventBus.cs`
  - Add support for TickEvent publishing
  - Ensure OnTick calls EventBus.Publish()
  - ~10-20 lines of changes

- `Assets/Code/Game.cs` or main game manager
  - Initialize TimeEngine on startup
  - Subscribe to TimeEngine events if needed
  - ~5-10 lines of changes

## Success Checklist

- [ ] TimeEngine.cs created and compiles
- [ ] All public methods implemented
- [ ] Initialize() sets up system
- [ ] Pause()/Resume() work correctly
- [ ] SetTimeScale() changes tick rate
- [ ] OnTick fires at 60 Hz
- [ ] OnTick fires correct number of times
- [ ] EventBus receives TickEvent
- [ ] No GC allocations in hot path
- [ ] Determinism test passes
- [ ] TimeScale edge cases handled (0, 4, negative values)
- [ ] Unit tests pass (95%+ coverage)
- [ ] Integration tests pass
- [ ] Performance test passes
- [ ] Serialization works
- [ ] Code review approved
- [ ] No compiler warnings
- [ ] Merged to main

## References

- **GitHub Issue**: https://github.com/Don-Walesky/Republic-Game/issues/[ISSUE_NUMBER]
- **Epic**: Epic: Time Engine
- **Wave Documentation**: [Development Bible/Wave 00 - Foundation](../../../../Development%20Bible/Wave%2000%20-%20Foundation/README.md)
- **TDD**: [TDD/README.md](../../../../TDD/README.md)
- **Related Issue**: Issue 5.7 (EventBus must be done first)

## Notes for Implementer

1. **Do this after EventBus is complete** - You need EventBus.Publish() to work
2. **Zero allocation is critical** - This fires every frame. Any GC allocs will kill performance
3. **Determinism matters** - Same seed must produce same tick progression (for multiplayer replay)
4. **Test edge cases** - What happens if frame rate dips below 60 FPS? What if it's 200 FPS?
5. **Comment thoroughly** - This is a core system others depend on
6. **Make it fast** - This is in the hot path. Performance matters.
