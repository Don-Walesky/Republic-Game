# ADR-0029: Two-Country Diplomatic Relation Opening

## Status
Accepted

## Context
Following the completion of domestic political governance in Phase 6—including founding political parties ([ADR-0024](ADR-0024-player-founded-political-party.md)), ruling and opposition party mandates ([ADR-0025](ADR-0025-ruling-and-opposition-parties.md)), civil society interest groups ([ADR-0026](ADR-0026-country-interest-group.md)), legislative bill enactment ([ADR-0027](ADR-0027-country-bill-vote.md)), and democratic national elections ([ADR-0028](ADR-0028-country-two-party-election.md))—Phase 7 initiates Republic's external sovereign interaction model.

Phase 7, item 7.1 establishes the foundational primitive of international statecraft: two sovereign countries opening a diplomatic relation. In order to model realistic, sovereign-driven external policy without prematurely introducing complex multi-party systems, diplomatic relations begin as unilateral, directed linkages registered upon the source sovereign country.

## Decision

1. **DiplomaticRelation Domain Record**:
   - Implemented as an immutable domain record [`DiplomaticRelation`](../../../src/Republic.Core/World/DiplomaticRelation.cs) located under `Republic.Core.World`.
   - Core Properties:
     - `FromCountryId`: unique identifier of the source sovereign country opening the relation.
     - `ToCountryId`: unique identifier of the target country.
     - `Status`: status enum [`DiplomaticRelationStatus`](../../../src/Republic.Core/World/DiplomaticRelation.cs) with values `Unrecognized` and `Recognized`.
     - `OpenedBoundary`: authoritative Republic simulation clock boundary ([`RepublicTime`](../../../src/Republic.Core/Time/RepublicTime.cs)) when the relation was opened.

2. **Source Country Storage**:
   - Stored directly on the initiating source country in [`Country`](../../../src/Republic.Core/World/Models/Country.cs).
   - Exposed through `Relation`, `DiplomaticRelation`, and `Relations` collection properties.

3. **Prerequisites & Self-Relation Prohibition**:
   - `OpenRelation` requires two different countries.
   - A country cannot open a diplomatic relation with itself (`FromCountryId != ToCountryId`). Self-relation attempts are rejected and return `null`.

4. **Ordered-Pair Idempotence & Uniqueness**:
   - A relation is uniquely identified by its directed ordered pair `(FromCountryId, ToCountryId)`.
   - Attempting to open a second relation between the same ordered pair is rejected, leaving the existing relation intact.

5. **Directed Asymmetry (Reverse Pair Separation)**:
   - In this slice, the reverse ordered pair `(ToCountryId, FromCountryId)` represents a distinct relation.
   - Opening a relation from Country A to Country B does not automatically create or alter a relation on Country B. Country B may open its own relation toward Country A independently.

6. **Sovereign Country Isolation**:
   - External nations (such as Country C) remain completely untouched and isolated when Country A opens a relation with Country B.

7. **Deterministic Simulation Time**:
   - Anchored strictly to authoritative Republic clock boundaries (`RepublicTime` / `IRepublicClock`).
   - Never calls `DateTime.Now` or `DateTime.UtcNow`.

8. **CLI Dashboard Presentation**:
   - Rendered in [`Program.cs`](../../../src/Republic.Cli/Program.cs) in the Presidential Desk dashboard directly beneath the national election line.
   - Displays `" Relation: No relation"` if none exists, or `" Relation: {relation}"` when present.
   - Does not add an interactive directive menu number.

9. **Scope Boundaries**:
   - Strictly confined to Phase 7 item 7.1 (opening one diplomatic relation between two countries).
   - Does not implement trade agreements, military alliances, diplomatic messaging, or multiplayer.
   - Does not modify domestic political parties, elections, or legislative bills.

## Consequences
- Establishes the core domain primitive for sovereign bilateral diplomacy.
- Preserves deterministic clock alignment, ordered pair uniqueness, and sovereign country isolation.
- Prepares Republic for subsequent bilateral diplomatic mechanics in Phase 7.
