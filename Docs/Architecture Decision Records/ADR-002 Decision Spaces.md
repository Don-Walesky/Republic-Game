# ADR-002: Decision Spaces

## Problem

How do we present complex political decisions without overwhelming the player?

## Context

Republic has dozens of interconnected systems. Each creates decisions. Without structure, the UI becomes a nightmare.

## Decision

**Decisions happen in dedicated "Decision Spaces" within the office.**

Each Decision Space:
- Focuses on one type of decision
- Presents information clearly
- Shows consequences before committing
- Connects to the right advisors/ministers

## Decision Space Examples

- **Cabinet Room**: Government decisions
- **Economics Office**: Economic policy
- **War Room**: Military strategy
- **Parliament Chamber**: Legislation
- **Diplomatic Suite**: International relations

## Why

1. **Clarity**: Each space is focused
2. **Context**: Right information for the right decision
3. **Consequence Preview**: See impact before deciding
4. **Roleplay**: Different spaces feel different
5. **Scalability**: Easy to add new Decision Spaces

## Trade-offs

✅ Prevents UI overload
✅ Creates natural pacing
❌ Requires more environment design
❌ Movement between spaces adds navigation

## Consequences

- Players learn by exploring Decision Spaces
- Office becomes a "hub" that connects all gameplay
- Each Wave can add new Decision Spaces
- Multiplayer players share Decision Spaces

## Related ADRs

- ADR-001: Office First Gameplay (where Decision Spaces live)
- ADR-003: Communication Driven Gameplay (how advisors drive decisions)
