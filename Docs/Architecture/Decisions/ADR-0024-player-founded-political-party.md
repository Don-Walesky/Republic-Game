# ADR-0024: Player-Founded Political Party

## Status
Accepted

## Context
Following the completion of Phase 5 (country profile in [ADR-0019](ADR-0019-readable-country-profile.md), national dashboard in [ADR-0020](ADR-0020-read-only-national-dashboard.md), national strength in [ADR-0021](ADR-0021-comparable-national-strength-score.md), and low-treasury approval consequence in [ADR-0023](ADR-0023-low-treasury-approval-consequence.md)), Phase 6 begins institutional power balancing.

Item 6.1 introduces the ability for a sovereign nation's leadership to establish a foundational political party. Political parties are the central vehicle through which democratic legitimacy, public sentiment, and parliamentary organization will flow in subsequent milestones.

## Decision

1. **Party Record Specification**:
   - Implemented as an immutable domain record [`PoliticalParty`](../../../src/Republic.Core/Politics/PoliticalParty.cs) in `Republic.Core.Politics`.
   - Attributes:
     - `Name`: trimmed string, non-empty. An empty or whitespace-only name is rejected.
     - `CountryId`: the unique identifier of the owning sovereign nation.
     - `FoundedBoundary`: the authoritative Republic simulation clock boundary (`RepublicTime`) when the party was founded.
     - `Support`: public support rating from `0.0` to `100.0`, starting at baseline `10.0`.

2. **Single-Party Constraint & Rejection**:
   - In this slice, a sovereign nation can have at most one political party.
   - Evaluated via `FoundParty` on [`Country`](../../../src/Republic.Core/World/Models/Country.cs).
   - If a party has already been founded for the nation, any subsequent `FoundParty` invocation is rejected (returns `null`), preserving the initial party and its support intact.
   - An empty or whitespace name is rejected and does not instantiate a party (returns `null`).

3. **Country Isolation**:
   - Country B does not receive Country A's party. Each sovereign country owns an independent, isolated `Party` property.
   - Founding a party in Country A induces zero mutations or side-effects in Country B.

4. **Temporal Determinism**:
   - The party founding moment is anchored strictly to the Republic simulation clock (`RepublicTime` / `IRepublicClock`).
   - Never calls `DateTime.Now` or `DateTime.UtcNow`.

5. **CLI Presentation**:
   - Printed in [`Program.cs`](../../../src/Republic.Cli/Program.cs) in the Presidential Desk dashboard directly beneath the systemic consequence line.
   - Displays `Party: {partyText}` where `partyText` is the formatted party description (name and support rating), or `"No party"` if none exists.
   - Does not add an interactive directive menu number.

6. **Scope Boundaries**:
   - Strictly confined to Phase 6 item 6.1 (founding one political party).
   - Does not implement elections, a legislature, interest groups, diplomacy, or military systems.
   - Does not rewrite National Yield, treasury, systemic consequences, or national strength.

## Consequences
- Establishes Republic's initial political institution model with full encapsulation and value equality.
- Maintains sovereign country isolation, idempotency, and temporal determinism.
- Prepares the domain for future campaign and electoral dynamics without premature complexity.
