# Wave 0 Issue: TimeEngine class structure

## Epic
Epic: Time Engine

## Objective
Create the base TimeEngine class that manages tick scheduling, time progression, and event announcements. This is the heartbeat of all Republic simulation.

## Requirements
- [ ] TimeEngine class with public API
- [ ] Tick scheduling at 60 Hz target (1/60 second per tick)
- [ ] Time scale support (pause, 0.5x, 1.0x, 2.0x, 4.0x)
- [ ] OnTick event fired every simulation tick
- [ ] Deterministic progression (same seed = same results)
- [ ] No garbage allocation per tick
- [ ] Integration with Event Bus
- [ ] Serializable for save/load

## Acceptance Criteria
- [ ] TimeEngine class exists with all public methods
- [ ] TimeEngine.Initialize() sets up the system
- [ ] TimeEngine.Pause() stops time progression
- [ ] TimeEngine.Resume() resumes time
- [ ] TimeEngine.SetTimeScale(float) changes time speed
- [ ] OnTick event fires at correct times
- [ ] 1000-tick test runs at stable 60 Hz
- [ ] Zero GC allocations in hot path (Update loop)
- [ ] Unit tests pass with 95%+ coverage
- [ ] Integrates with Event Bus correctly
- [ ] Determinism test passes (same seed = same progression)

## Implementation Notes
- Use MonoBehaviour Update() loop for frame-independent stepping
- Accumulate deltaTime each frame
- Fire OnTick when accumulated time > (1/60) second
- Support timeScale by multiplying deltaTime
- Publish TickEvent to EventBus on each tick
- Store TickCount and CurrentTime as serializable state

## Files to Create/Modify
- `Assets/Code/Core/Time/TimeEngine.cs` (CREATE)
- `Assets/Code/Core/Time/TickEventArgs.cs` (CREATE)
- `Assets/Code/Core/Time/TimeEngine.Tests.cs` (CREATE)
- `Assets/Code/Core/Events/EventBus.cs` (MODIFY - add TickEvent support)

## Dependencies
- Issue 8.5: Dependency Injection unit tests (DI framework must be done first)
- Issue 5.7: Event Bus unit tests (EventBus must be done first)

## AI Implementation Prompt
See: `Docs/GitHub/AI Prompts/Wave 00/TimeEngine-Class-Structure.md`
