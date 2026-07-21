# Architecture

## Canonical Delivery Model

Republic is organized by **player-experience waves** and implemented through **headless simulation layers**.

- Waves answer **what the player experiences next**.
- Layers answer **where code belongs and how systems interact**.

The authoritative decision is documented in:
- `/home/runner/work/Republic-Game/Republic-Game/Docs/Architecture/Decisions/ADR-0001-canonical-delivery-model.md`

## Wave 0 Runtime Layout

### Headless Core
- `Republic.Core.Configuration`
- `Republic.Core.Diagnostics`
- `Republic.Core.Events`
- `Republic.Core.Time`
- `Republic.Core.Persistence`
- `Republic.Core.World`
- `Republic.Core.Engine`

### Bootstrap Host
- `Republic.App`
- dependency injection container
- finite bootstrap run for CI and local verification

### Future Unity Boundary
- `/home/runner/work/Republic-Game/Republic-Game/unity`
- reserved for Wave 1 presentation work
- no Unity dependency is required for Wave 0 builds or tests

## System Relationships

1. Configuration loads first.
2. Logging is constructed from configuration.
3. Event bus uses logging for diagnostics.
4. Time system emits deterministic events through the event bus.
5. World manager tracks the current world shell and entity registry.
6. Engine coordinates time, world advancement, and queued event processing.
7. Persistence serializes world and time state without Unity dependencies.

## Design Rules

- Domain logic stays outside Unity.
- Every system exposes explicit inputs, outputs, and events.
- Every new feature must preserve deterministic simulation where possible.
- Every wave should ship as a playable or executable vertical slice.
