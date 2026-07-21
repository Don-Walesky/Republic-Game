# GitHub Issue Template

**How to break an Epic into actionable GitHub Issues.**

Issues are specific, implementable tasks. Each Issue becomes an AI Implementation Prompt.

---

## Issue Template

```markdown
# Issue: [Component Name]

## Epic
[Link to parent Epic]

## Objective
[What does this Issue accomplish?]

## Requirements
- [ ] Requirement 1
- [ ] Requirement 2
- [ ] Requirement 3

## Acceptance Criteria
- [ ] Can measure this
- [ ] Can test this
- [ ] Works in context

## Implementation Notes
[Technical hints, gotchas, integration points]

## Files to Create/Modify
- `path/to/file.cs`
- `path/to/test.cs`

## Dependencies
- [ ] Issue: [Other Issue](#) must be done first

## AI Implementation Prompt
[See AI Prompt Template for full prompt]
```

---

## Example: Wave 0 Issue

```markdown
# Issue: TimeEngine class structure

## Epic
[Epic: Time Engine](#)

## Objective
Create the base TimeEngine class that manages tick scheduling, time scaling, and event announcements.

## Requirements
- [ ] TimeEngine class with public API
- [ ] Tick scheduling at 60 Hz target
- [ ] Time scale support (pause, 0.5x, 1.0x, 2.0x, 4.0x)
- [ ] OnTick event fired every frame
- [ ] Deterministic progression
- [ ] No garbage allocation per tick

## Acceptance Criteria
- [ ] TimeEngine.Start() initializes system
- [ ] TimeEngine.Tick() advances time
- [ ] TimeEngine.TimeScale property works
- [ ] OnTick event fires at correct times
- [ ] Unit tests pass (95%+ coverage)
- [ ] No GC allocs in hot path

## Implementation Notes
- Use MonoBehaviour lifecycle (Awake, Update, LateUpdate)
- Fire OnTick in Update loop
- Maintain frame delta time for frame-rate independence
- Store accumulated delta time for next tick
- Integrate with Event Bus: EventBus.Publish(new TickEvent())

## Files to Create/Modify
- `Assets/Code/Core/Time/TimeEngine.cs` (CREATE)
- `Assets/Code/Core/Time/TimeEngine.Tests.cs` (CREATE)
- `Assets/Code/Core/Events/TickEvent.cs` (CREATE)

## Dependencies
- [ ] Issue: Event Bus implementation must be done first

## AI Implementation Prompt
[See AI Prompt Template]
```

---

## How to Break Epics into Issues

For each Epic:

1. **Identify components** - What are the main parts?
2. **Define dependencies** - Which must come first?
3. **Create Issues in order** - One per component
4. **Link them** - Show dependency chain
5. **Assign to Epic** - All Issues belong to Epic

---

## Issue Breakdown Example

**Epic: Time Engine (1-2 weeks)**
```
Issue 1: TimeEngine class structure (2-3 days)
Issue 2: TickManager implementation (2-3 days)
Issue 3: Time scaling system (1-2 days)
Issue 4: Deterministic RNG (1-2 days)
Issue 5: Performance monitoring (1 day)
Issue 6: Unit tests (2-3 days)
```

---

## Tips

✅ Each Issue should take 1-3 days
✅ Break into smaller pieces if larger
✅ Show dependencies clearly
✅ Requirements must be specific
✅ Acceptance criteria must be testable
✅ Include technical hints for implementer

---

## Next Steps

Once Issue is created:
1. Convert to AI prompt using [AI Prompt Template](../AI%20Implementation%20Prompt%20Template/README.md)
2. Assign to developer (human or AI)
3. Move to "In Progress" when started
4. Move to "Review" when complete
5. Close when merged
