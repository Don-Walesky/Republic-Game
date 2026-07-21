# ADR-004: Procedural Countries

## Problem

How do we create unique, believable countries without hand-authoring each one?

## Context

Republic needs multiple countries in the world. Hand-crafting each would be:
- Time-consuming
- Limiting
- Repetitive

## Decision

**Countries are procedurally generated from seed parameters.**

Each country has:
- Procedural name generation
- Procedural geography (terrain, resources)
- Procedural demographics
- Procedural government type
- Procedural political culture
- Procedural starting relationships

## Why

1. **Replayability**: Different world each game
2. **Scale**: Unlimited countries possible
3. **Uniqueness**: Each country feels distinct
4. **Deterministic**: Same seed = same country (for multiplayer)
5. **Moddability**: Easy to tweak country generation

## Trade-offs

✅ Massive replayability
✅ Scales to unlimited worlds
❌ Risk of "generic" feeling countries
❌ Complex procedural generation system
❌ Harder to balance

## Consequences

- Each playthrough has a unique world
- Multiplayer uses shared seed for consistency
- Countries feel alive and unique
- Players can't "exploit" geography (it's randomized)
- Balance requires careful tuning of generation parameters

## Related ADRs

- None yet
