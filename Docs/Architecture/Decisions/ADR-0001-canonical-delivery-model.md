# ADR-0001: Canonical Delivery Model

## Status
Accepted

## Context
The repository contained two conflicting planning models:
- a wave-oriented player-experience model
- a layered technical model presented as an alternative roadmap

Those models were being interpreted as competing sources of truth, which made Sprint 1 and Wave 0 scope ambiguous.

## Decision
Republic will use the **10-wave player-experience roadmap** as the canonical delivery model.

Technical layers remain valid, but only as an implementation aid beneath the wave plan.

Wave 0 will be implemented as a **headless deterministic .NET core** with a reserved Unity shell boundary rather than a Unity-dependent runtime from day one.

The first playable milestone after Wave 0 is **Wave 1: Executive Workspace**, followed by **Wave 2: World Simulation**.

## Consequences
- roadmap, feature ordering, and milestone gates follow the 10-wave model
- simulation logic stays testable without Unity runtime dependencies
- future Unity work adapts the core instead of owning it
- legacy revised planning documents are historical reference, not canonical planning authority
