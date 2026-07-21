# Code Style

## C# Conventions

- Use file-scoped namespaces.
- Enable nullable reference types.
- Prefer small focused types.
- Keep public APIs explicit and documented.
- Prefer immutable records for event payloads and snapshots.

## Architecture Conventions

- Simulation code must not depend on Unity runtime types.
- Time-based state changes should flow through the event bus.
- Persisted state should use typed envelopes with version metadata.

## Testing Conventions

- Add unit tests alongside new core behavior.
- Cover success paths, edge cases, and failure handling.
- Prefer deterministic tests with no real-time sleeps.
