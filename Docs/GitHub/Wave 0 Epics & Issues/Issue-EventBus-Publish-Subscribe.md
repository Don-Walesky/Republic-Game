# Wave 0 Issue: Event Bus - Publish/Subscribe System

## Epic
Epic: Event Bus

## Objective
Create the core event bus (pub/sub) system that enables decoupled communication between all game systems. This is critical for Wave 0 and all subsequent waves.

## Requirements
- [ ] EventBus singleton class
- [ ] Publish(event) method for broadcasting events
- [ ] Subscribe<T>(callback) method for listeners
- [ ] Unsubscribe<T>(callback) method for cleanup
- [ ] Support multiple subscribers per event type
- [ ] Event ordering (priority system)
- [ ] No memory leaks (proper cleanup)
- [ ] Minimal garbage allocation

## Acceptance Criteria
- [ ] EventBus is a singleton
- [ ] Publish sends event to all subscribers
- [ ] Subscribe registers a callback
- [ ] Unsubscribe removes callback
- [ ] Multiple subscribers work together
- [ ] Events fire in correct order
- [ ] Unsubscribe prevents memory leaks
- [ ] 0 GC allocations per publish (hot path)
- [ ] Unit tests pass with 95%+ coverage
- [ ] Supports generic event types
- [ ] Thread-safe (basic synchronization)

## Implementation Notes
- Use generic Subscribe<T> for type safety
- Store subscribers in Dictionary<Type, List<Delegate>>
- Support priority ordering (higher priority fires first)
- Prevent double-subscription of same callback
- Clear subscriber list on Unsubscribe
- Log all errors to Logger system

## Files to Create/Modify
- `Assets/Code/Core/Events/EventBus.cs` (CREATE)
- `Assets/Code/Core/Events/GameEvent.cs` (CREATE - base event class)
- `Assets/Code/Core/Events/EventBus.Tests.cs` (CREATE)

## Dependencies
- Issue 2.6: Logger system must be done first
- Issue 8.5: Dependency Injection optional but helpful

## AI Implementation Prompt
See: `Docs/GitHub/AI Prompts/Wave 00/EventBus-Publish-Subscribe.md`
