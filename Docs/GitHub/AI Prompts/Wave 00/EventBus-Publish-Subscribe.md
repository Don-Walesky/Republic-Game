# AI Implementation: Event Bus - Publish/Subscribe System

## Context
**Wave**: Wave 0 (Foundation)
**Epic**: Epic: Event Bus
**Issue**: Issue 5.2 - Publish/Subscribe System
**Priority**: Critical - This enables all decoupled communication between systems

## Objective
Create the EventBus singleton that implements a publish/subscribe pattern for all game events. This is the communication backbone for all Republic systems.

## Requirements
- [ ] EventBus singleton class
- [ ] Generic Publish<T> method
- [ ] Generic Subscribe<T> method
- [ ] Unsubscribe<T> method for cleanup
- [ ] Support multiple subscribers per event
- [ ] Event ordering (priority system)
- [ ] No memory leaks when unsubscribing
- [ ] Minimal garbage allocation
- [ ] Thread-safe basic operations
- [ ] Error handling and logging

## Acceptance Criteria
- [ ] EventBus.cs compiles without warnings
- [ ] Singleton pattern works (only one instance)
- [ ] Publish sends to all subscribers
- [ ] Subscribe registers callback correctly
- [ ] Unsubscribe removes callback completely
- [ ] Multiple subscribers work together
- [ ] Events fire in priority order
- [ ] Memory freed when unsubscribing
- [ ] Zero GC allocations per publish
- [ ] Unit tests pass with 95%+ coverage
- [ ] Supports generic event types
- [ ] Errors logged, not thrown
- [ ] Thread-safe for basic operations
- [ ] Code review approved
- [ ] Merged to main branch

## Technical Specification

### Architecture Overview

EventBus is a singleton that manages event subscriptions and publishes events to subscribers. It uses a dictionary-based approach for type safety and performance.

**Key Pattern**:
```
Subscribe<MyEvent>(callback):
  Store callback in subscribers[MyEvent]

Publish<MyEvent>(event):
  For each callback in subscribers[MyEvent]:
    callback(event)

Unsubscribe<MyEvent>(callback):
  Remove callback from subscribers[MyEvent]
```

### Data Model

```csharp
public class EventBus
{
    // Singleton instance
    private static EventBus instance;
    
    // Storage: Type -> List of Delegates
    private Dictionary<Type, List<Delegate>> subscribers 
        = new Dictionary<Type, List<Delegate>>();
    
    // Priority queue for ordered events
    private Dictionary<Delegate, int> subscriberPriorities 
        = new Dictionary<Delegate, int>();
    
    // Logging
    private Logger logger;
    
    // Options
    private bool throwExceptions = false;
    private bool logAllEvents = false;
}
```

### Public API

```csharp
public class EventBus
{
    /// <summary>
    /// Get singleton instance of EventBus.
    /// Creates if doesn't exist.
    /// </summary>
    public static EventBus Instance
    {
        get { /* return singleton */ }
    }
    
    /// <summary>
    /// Subscribe to events of type T.
    /// Callback will be called when event is published.
    /// </summary>
    public void Subscribe<T>(System.Action<T> callback, int priority = 0) 
        where T : GameEvent
    {
        // Add callback to subscribers[T]
        // Store priority
        // Prevent duplicates
    }
    
    /// <summary>
    /// Unsubscribe from events of type T.
    /// Callback will no longer be called.
    /// </summary>
    public void Unsubscribe<T>(System.Action<T> callback) 
        where T : GameEvent
    {
        // Remove callback from subscribers[T]
        // Remove priority
        // Log if callback wasn't found
    }
    
    /// <summary>
    /// Publish an event to all subscribers.
    /// </summary>
    public void Publish<T>(T gameEvent) 
        where T : GameEvent
    {
        // Fire all subscribers in priority order
        // Handle exceptions
    }
    
    /// <summary>
    /// Get count of subscribers for event type.
    /// </summary>
    public int GetSubscriberCount<T>() 
        where T : GameEvent
    {
        // Return count
    }
    
    /// <summary>
    /// Clear all subscribers (mostly for testing).
    /// </summary>
    public void Clear()
    {
        // Clear all subscriptions
    }
    
    /// <summary>
    /// Set whether exceptions in callbacks are thrown or logged.
    /// </summary>
    public void SetThrowExceptions(bool throwOnError)
    {
        // Set throwExceptions
    }
}

/// <summary>
/// Base class for all game events.
/// </summary>
public abstract class GameEvent
{
    public float Timestamp { get; set; }
}
```

### Integration Points

**Logger Integration**:
```csharp
// Log events and errors
if (logAllEvents)
    logger.Info($"Publishing event: {typeof(T).Name}");

if (error)
    logger.Error($"Error publishing {typeof(T).Name}: {ex.Message}");
```

**Dependency Injection** (optional):
```csharp
// Can be injected as singleton
var eventBus = serviceContainer.Resolve<EventBus>();
```

**Event Types**:
```csharp
// Various event types will inherit from GameEvent
public class TickEvent : GameEvent { }
public class InputEvent : GameEvent { }
public class GameStateChangedEvent : GameEvent { }
```

## Implementation Strategy

### Step 1: Create EventBus class structure
- Create `EventBus.cs`
- Implement singleton pattern (static Instance property)
- Add private fields for subscribers and priorities
- Add Logger field
- Make constructor private

### Step 2: Implement Subscribe method
- Accept generic type T (must inherit from GameEvent)
- Accept callback (System.Action<T>)
- Accept optional priority (default 0)
- Create Dictionary entry for T if not exists
- Add callback to list (prevent duplicates)
- Store priority in dictionary

### Step 3: Implement Unsubscribe method
- Accept generic type T
- Accept callback
- Find and remove callback from subscribers[T]
- Remove priority entry
- Log if callback not found (warning, not error)

### Step 4: Implement Publish method
- Accept generic type T
- Get subscribers for T
- Sort by priority (higher priority first)
- Call each callback with event
- Catch exceptions and handle per throwExceptions setting
- Log events if enabled

### Step 5: Add helper methods
- GetSubscriberCount<T>()
- Clear() for testing
- SetThrowExceptions()
- SetLogAllEvents()

### Step 6: Add comprehensive unit tests
- Test subscribe/unsubscribe
- Test publish to single subscriber
- Test publish to multiple subscribers
- Test priority ordering
- Test duplicate prevention
- Test memory cleanup
- Test no GC allocations
- Test error handling

## Testing Strategy

### Unit Tests

```csharp
[TestFixture]
public class EventBusTests
{
    private EventBus eventBus;
    
    [SetUp]
    public void Setup()
    {
        // Get fresh EventBus instance
        eventBus = EventBus.Instance;
        eventBus.Clear();
    }
    
    // Subscribe/Unsubscribe tests
    [Test]
    public void Test_Subscribe_AddCallback()
    {
        int callCount = 0;
        eventBus.Subscribe<TestEvent>(e => callCount++);
        
        eventBus.Publish(new TestEvent());
        
        Assert.AreEqual(1, callCount);
    }
    
    [Test]
    public void Test_Unsubscribe_RemovesCallback()
    {
        int callCount = 0;
        System.Action<TestEvent> callback = e => callCount++;
        
        eventBus.Subscribe(callback);
        eventBus.Unsubscribe(callback);
        eventBus.Publish(new TestEvent());
        
        Assert.AreEqual(0, callCount);
    }
    
    // Multiple subscriber tests
    [Test]
    public void Test_MultipleSubscribers_AllCalled()
    {
        int count1 = 0, count2 = 0;
        eventBus.Subscribe<TestEvent>(e => count1++);
        eventBus.Subscribe<TestEvent>(e => count2++);
        
        eventBus.Publish(new TestEvent());
        
        Assert.AreEqual(1, count1);
        Assert.AreEqual(1, count2);
    }
    
    // Priority tests
    [Test]
    public void Test_Priority_HigherFirst()
    {
        var order = new List<int>();
        eventBus.Subscribe<TestEvent>(e => order.Add(1), priority: 1);
        eventBus.Subscribe<TestEvent>(e => order.Add(2), priority: 2);
        eventBus.Subscribe<TestEvent>(e => order.Add(3), priority: 0);
        
        eventBus.Publish(new TestEvent());
        
        Assert.AreEqual(new List<int> { 2, 1, 3 }, order);
    }
    
    // Duplicate prevention
    [Test]
    public void Test_NoDuplicates_SameCallbackTwice()
    {
        int callCount = 0;
        System.Action<TestEvent> callback = e => callCount++;
        
        eventBus.Subscribe(callback);
        eventBus.Subscribe(callback); // Try to subscribe again
        
        eventBus.Publish(new TestEvent());
        
        Assert.AreEqual(1, callCount); // Called once, not twice
    }
    
    // Exception handling
    [Test]
    public void Test_Exception_Logged_NotThrown()
    {
        eventBus.SetThrowExceptions(false);
        
        eventBus.Subscribe<TestEvent>(e => throw new System.Exception("Test"));
        eventBus.Subscribe<TestEvent>(e => { /* should still be called */ });
        
        Assert.DoesNotThrow(() => eventBus.Publish(new TestEvent()));
    }
    
    // Memory tests
    [Test]
    public void Test_NoGarbageAllocation()
    {
        eventBus.Subscribe<TestEvent>(e => { });
        
        // Measure GC allocations during publish
        // Should be zero
    }
}
```

### Integration Tests

- EventBus works with multiple event types
- Subscribers from different systems don't interfere
- Events can carry data
- Exceptions don't crash EventBus

## Files to Create

**Primary Implementation**:
- `Assets/Code/Core/Events/EventBus.cs` - EventBus singleton (~250-350 lines)
- `Assets/Code/Core/Events/GameEvent.cs` - Base event class (~30 lines)

**Test Files**:
- `Assets/Tests/Core/Events/EventBus.Tests.cs` - Unit tests (~350-450 lines)

**Supporting Files** (these will be created later by other issues):
- `Assets/Code/Core/Events/TickEvent.cs` - Specific event types
- `Assets/Code/Core/Events/InputEvent.cs` - etc.

## Files to Modify

None for this issue. Other systems will be modified to use EventBus.

## Success Checklist

- [ ] EventBus.cs created and compiles
- [ ] Singleton pattern implemented
- [ ] Subscribe method works
- [ ] Unsubscribe method works
- [ ] Publish sends to all subscribers
- [ ] Priority ordering works
- [ ] Duplicate prevention works
- [ ] Multiple event types work
- [ ] Memory properly freed
- [ ] No GC allocations per publish
- [ ] Exception handling works
- [ ] Unit tests pass (95%+ coverage)
- [ ] Integration tests pass
- [ ] Code review approved
- [ ] No compiler warnings
- [ ] Merged to main

## References

- **GitHub Issue**: https://github.com/Don-Walesky/Republic-Game/issues/[ISSUE_NUMBER]
- **Epic**: Epic: Event Bus
- **Wave Documentation**: [Development Bible/Wave 00 - Foundation](../../../../Development%20Bible/Wave%2000%20-%20Foundation/README.md)
- **TDD**: [TDD/README.md](../../../../TDD/README.md)
- **Related Issue**: Issue 2.6 (Logger should be done first)

## Notes for Implementer

1. **Do this early** - Many systems depend on EventBus
2. **Zero allocation is critical** - This is in the hot path
3. **Make it robust** - It must handle exceptions gracefully
4. **Test thoroughly** - This is infrastructure; bugs here cascade
5. **Document well** - Developers will use this extensively
6. **Keep it simple** - Don't over-engineer. KISS principle.
