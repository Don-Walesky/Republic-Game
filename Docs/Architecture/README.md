# Architecture

## Wave 0 Technical Baseline

Republic starts as a headless deterministic simulation core.

### Core Namespaces
- `Republic.Core.Configuration`
- `Republic.Core.Diagnostics`
- `Republic.Core.Events`
- `Republic.Core.Time`
- `Republic.Core.Persistence`
- `Republic.Core.World`
- `Republic.Core.Engine`

### Host Namespace
- `Republic.App`

## Unity Separation

The Unity shell is intentionally deferred behind `unity/` so Wave 0 can build and test without engine coupling.

## Decision Record

See `Decisions/ADR-0001-canonical-delivery-model.md`.
