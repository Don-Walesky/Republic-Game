# ADR-001: Office First Gameplay

## Problem

How do we make the player feel like a government leader from the moment they start playing?

## Context

Traditional political games start with a map, stats, or abstract interface. Players don't feel immersed.

We wanted the opposite: **immersion first**.

## Decision

**We start in an office.**

The first thing the player sees is their workspace. A desk. A chair. Windows. A phone ringing.

## Why

1. **Immersion**: The player immediately feels like a leader
2. **Scale**: The office is the "office" - the command center
3. **Intimacy**: Personal space makes decisions feel personal
4. **Agency**: Phone calls and visitors create interactive moments
5. **Pacing**: Office interactions naturally pace information delivery

## Trade-offs

✅ Higher development cost (3D environment)
✅ More complex interaction model
❌ Requires strong UX/UI design
❌ Performance considerations (always-loaded office)

## Consequences

- All game information flows through the office
- Players understand their role through office interactions
- Campaign/Government feel like natural extensions of "getting work done"
- Multiplayer can use offices as player hubs

## Related ADRs

- ADR-003: Communication Driven Gameplay (how office communication works)
