# AI Implementation Prompt Template

**How to convert a GitHub Issue into an AI Implementation Prompt.**

This is what you hand to Copilot or other AI coding assistants to implement the Issue.

---

## Prompt Template

```markdown
# AI Implementation: [Issue Name]

## Context
[What is this part of? What Wave? What Epic?]

## Objective
[What should the AI build?]

## Requirements
[Copy from Issue]

## Acceptance Criteria
[Copy from Issue]

## Technical Specification

### Architecture
[How should this be structured?]

### Data Model
[What data structures?]

### Public API
```csharp
// Show the interface the AI should create
public class Example {
  public void Method() { }
}
```

### Integration Points
- **Event Bus**: How does this publish/subscribe?
- **Time Engine**: How does this use time?
- **Save System**: What data needs serialization?
- **Other**: Other dependencies?

## Implementation Strategy

### Step 1: Create class structure
[Pseudo-code or description]

### Step 2: Implement core logic
[What's the main algorithm?]

### Step 3: Add event integration
[How does this talk to other systems?]

### Step 4: Write unit tests
[What should be tested?]

## Testing Strategy

### Unit Tests
[What unit tests are needed?]

### Integration Tests
[How does this integrate with other systems?]

### Performance Tests
[Any performance requirements?]

## Files to Create
- `path/to/file.cs` - [Description]
- `path/to/test.cs` - [Description]

## Files to Modify
- `path/to/existing/file.cs` - [What changes?]

## Success Checklist
- [ ] Code compiles without warnings
- [ ] All requirements implemented
- [ ] All acceptance criteria met
- [ ] Unit tests pass
- [ ] No memory leaks
- [ ] Performance targets met
- [ ] Code reviewed
- [ ] Merged to main

## References
- **Issue**: [Link to GitHub Issue]
- **Epic**: [Link to Epic]
- **Wave**: [Link to Wave Documentation]
- **TDD**: [Link to Technical Design Document]
```

---

## Example: Wave 0 AI Prompt

```markdown
# AI Implementation: TimeEngine class structure

## Context
Wave 0 (Foundation) - Epic: Time Engine

This is the core time management system for Republic. It must:
- Run at stable 60 Hz independent of frame rate
- Support time scaling (pause, fast-forward)
- Be deterministic for multiplayer/replay
- Announce ticks to other systems via Event Bus

## Objective
Create the TimeEngine class that manages tick scheduling, time progression, and event announcements.

## Requirements
- TimeEngine class with public API
- Tick scheduling at 60 Hz target
- Time scale support (pause, 0.5x, 1.0x, 2.0x, 4.0x)
- OnTick event fired every simulation tick
- Deterministic progression
- No garbage allocation per tick

## Acceptance Criteria
- [ ] TimeEngine.Start() initializes system
- [ ] TimeEngine.Tick() advances time correctly
- [ ] TimeEngine.TimeScale property works
- [ ] OnTick event fires at correct times
- [ ] Unit tests pass (95%+ coverage)
- [ ] No GC allocs in hot path (Update loop)
- [ ] 1000-tick test runs at stable 60 Hz

## Technical Specification

### Architecture
Singleton MonoBehaviour that runs in Update loop. Fires deterministic ticks at 60 Hz regardless of frame rate.

Pattern:
```
Accumulate deltaTime each Update
When accumulated > (1/60) seconds:
  - Fire OnTick event
  - Subtract tick duration from accumulated time
  - Repeat until accumulated < (1/60) seconds
```

### Data Model
```csharp
public class TimeEngine : MonoBehaviour {
  // Properties
  public float CurrentTime { get; }
  public int TickCount { get; }
  public float TimeScale { get; set; }
  public bool IsPaused { get; set; }
  
  // Events
  public event System.Action<TickEventArgs> OnTick;
  public event System.Action OnPause;
  public event System.Action OnResume;
  
  // Constants
  private const float TICK_DURATION = 1f / 60f; // 60 Hz
}
```

### Public API
```csharp
public class TimeEngine : MonoBehaviour {
  // Initialization
  public void Initialize();
  
  // Time control
  public void Pause();
  public void Resume();
  public void SetTimeScale(float scale); // 0.5, 1.0, 2.0, 4.0
  
  // Time queries
  public float GetCurrentTime();
  public int GetTickCount();
  public float GetDeltaTime();
  
  // Events
  public event System.Action<TickEventArgs> OnTick;
}

public class TickEventArgs : System.EventArgs {
  public float DeltaTime { get; set; }
  public int TickCount { get; set; }
  public float CurrentTime { get; set; }
}
```

### Integration Points
- **Event Bus**: Publish TickEvent on each tick
- **Save System**: Serialize CurrentTime and TickCount
- **Other systems**: Subscribe to OnTick for simulation updates

## Implementation Strategy

### Step 1: Create TimeEngine class
- Inherit from MonoBehaviour
- Implement Singleton pattern
- Add properties and events

### Step 2: Implement tick scheduling
- Accumulate deltaTime in Update()
- Fire OnTick when threshold crossed
- Support time scaling

### Step 3: Add time scale support
- Multiply deltaTime by timeScale
- Clamp timeScale to valid range
- Fire OnTimeScaleChanged event

### Step 4: Add pause/resume
- Set timeScale to 0 when paused
- Fire OnPause/OnResume events
- Prevent tick firing when paused

### Step 5: Event Bus integration
- Publish TickEvent on each tick
- Include TickEventArgs
- Other systems subscribe and respond

## Testing Strategy

### Unit Tests
```csharp
// Test tick scheduling
public void Test_TicksFire_AtCorrectInterval() { }
public void Test_NoTicks_WhenPaused() { }
public void Test_TicksScale_WithTimeScale() { }
public void Test_NoGarbageAllocation_PerTick() { }

// Test time tracking
public void Test_CurrentTime_AdvancesCorrectly() { }
public void Test_TickCount_Increments() { }
public void Test_DeltaTime_ConsistentPerTick() { }

// Test serialization
public void Test_Serialize_TimeState() { }
public void Test_Deserialize_TimeState() { }
```

### Integration Tests
- TimeEngine starts without errors
- Event Bus receives tick events
- Other systems respond to ticks
- Time progression is deterministic

### Performance Tests
- Run 1000 ticks, measure time
- Verify 60 Hz achievement
- Check memory allocations (0 per tick)

## Files to Create
- `Assets/Code/Core/Time/TimeEngine.cs` - Main TimeEngine class
- `Assets/Code/Core/Time/TickEventArgs.cs` - Tick event data
- `Assets/Code/Core/Time/TimeEngine.Tests.cs` - Unit tests

## Files to Modify
- `Assets/Code/Core/Events/EventBus.cs` - Add TickEvent publishing
- `Assets/Code/Game.cs` - Initialize TimeEngine on startup

## Success Checklist
- [ ] TimeEngine.cs compiles without warnings
- [ ] All requirements implemented
- [ ] All acceptance criteria met
- [ ] Unit tests pass (95%+ coverage)
- [ ] Integration tests pass
- [ ] Performance targets met (60 Hz, 0 GC allocs/tick)
- [ ] Code review passed
- [ ] Merged to main branch

## References
- **Issue**: https://github.com/Don-Walesky/Republic-Game/issues/[ISSUE_NUMBER]
- **Epic**: Epic: Time Engine
- **Wave**: [Development Bible/Wave 00 - Foundation](../../Development%20Bible/Wave%2000%20-%20Foundation/README.md)
- **TDD**: [TDD/README.md](../../TDD/README.md)
```

---

## How to Use This Template

1. Take a GitHub Issue
2. Fill in all sections of this template
3. Make the prompt specific and detailed
4. Include code examples
5. Show expected behavior
6. List all success criteria
7. Pass to AI or developer
8. They implement and submit PR
9. PR review and merge

---

## Tips

✅ Be **specific** - "create TimeEngine" vs "create time management system"
✅ Show **code examples** - What should the API look like?
✅ List **dependencies** - What must be done first?
✅ Define **success** - How do we know it's done?
✅ Include **context** - Why does this matter?
✅ Add **gotchas** - What are common mistakes?

---

## The Pipeline

```
GitHub Issue
  └─ Convert to AI Prompt (this template)
      └─ Send to Copilot or developer
          └─ Implement
              └─ Submit PR
                  └─ Review
                      └─ Merge
                          └─ Close Issue
```
